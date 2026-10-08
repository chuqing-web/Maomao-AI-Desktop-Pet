using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MaomaoDesktopPet.Models;

namespace MaomaoDesktopPet.Services;

public sealed class AiService
{
    private readonly PetData _data;
    private readonly Action _persist;
    private readonly QuestService _quests;
    private readonly AchievementService _achievements;
    private readonly MemoryStore _memory;
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(60) };

    public AiService(
        PetData data,
        Action persist,
        QuestService quests,
        AchievementService achievements,
        MemoryStore memory)
    {
        _data = data;
        _persist = persist;
        _quests = quests;
        _achievements = achievements;
        _memory = memory;
    }

    public bool CloudConfigured =>
        _data.Settings.UseCloudAi &&
        !string.IsNullOrWhiteSpace(_data.Settings.AiApiKey) &&
        !string.IsNullOrWhiteSpace(_data.Settings.AiApiBase);

    public async Task<string> ChatAsync(string userText, CancellationToken ct = default)
    {
        userText = userText.Trim();
        if (string.IsNullOrEmpty(userText))
            return "……你想说什么呀？";

        _data.ChatHistory.Add(new ChatMessage { Role = "user", Text = userText });
        _data.TotalChatMessages++;
        _data.LastChatAt = DateTime.Now;
        _data.LastInteractAt = DateTime.Now;
        _quests.Progress("chat", 1);

        string reply;
        var usedCloud = false;
        if (CloudConfigured)
        {
            try
            {
                reply = await CloudChatAsync(ct);
                usedCloud = true;
            }
            catch (Exception ex)
            {
                reply = LocalChat(userText) + $"\n（云端暂不可用：{SimplifyError(ex.Message)}，已用本地人格）";
            }
        }
        else
        {
            reply = LocalChat(userText);
            if (_data.Settings.UseCloudAi && !CloudConfigured)
                reply += "\n（请在设置里填写 API Base 与 API Key）";
        }

        // Strip accidental memory tags from visible reply if model appended them
        var (cleanReply, extracted) = SplitMemoryTags(reply);
        reply = cleanReply;

        _data.ChatHistory.Add(new ChatMessage { Role = "assistant", Text = reply });
        if (_data.ChatHistory.Count > 100)
            _data.ChatHistory.RemoveRange(0, _data.ChatHistory.Count - 100);

        RememberFromUser(userText);
        foreach (var m in extracted)
            _memory.Add(_data, m, "AI");

        if (usedCloud)
            await TryExtractMemoryViaCloudAsync(userText, reply, ct);

        ApplyMoodFromReply(reply);
        _achievements.Check();
        _persist();
        return reply;
    }

    public async Task<(bool Ok, string Message)> TestConnectionAsync(CancellationToken ct = default)
    {
        if (!CloudConfigured)
            return (false, "请先勾选云端 AI，并填写 API Base 与 API Key。");

        try
        {
            var url = NormalizeChatUrl(_data.Settings.AiApiBase!);
            var body = new
            {
                model = string.IsNullOrWhiteSpace(_data.Settings.AiModel) ? "gpt-4o-mini" : _data.Settings.AiModel,
                messages = new object[]
                {
                    new { role = "system", content = "Reply with exactly: ok" },
                    new { role = "user", content = "ping" }
                },
                max_tokens = 8,
                temperature = 0
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _data.Settings.AiApiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            using var resp = await Http.SendAsync(req, ct);
            var json = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                return (false, $"失败 HTTP {(int)resp.StatusCode}: {ExtractApiError(json)}");

            var content = ExtractAssistantContent(json);
            return (true, $"连接成功。模型回复：{(string.IsNullOrWhiteSpace(content) ? "(空)" : content)}");
        }
        catch (Exception ex)
        {
            return (false, "连接失败：" + SimplifyError(ex.Message));
        }
    }

    private async Task<string> CloudChatAsync(CancellationToken ct)
    {
        var url = NormalizeChatUrl(_data.Settings.AiApiBase!);
        var system = BuildSystemPrompt();

        // History already contains the latest user message — do not append again.
        var messages = new List<object> { new { role = "system", content = system } };
        foreach (var m in _data.ChatHistory.TakeLast(16))
        {
            var role = m.Role == "assistant" ? "assistant" : "user";
            messages.Add(new { role, content = m.Text });
        }

        var body = new Dictionary<string, object?>
        {
            ["model"] = string.IsNullOrWhiteSpace(_data.Settings.AiModel) ? "gpt-4o-mini" : _data.Settings.AiModel,
            ["messages"] = messages,
            ["temperature"] = Math.Clamp(_data.Settings.AiTemperature, 0.1, 1.5),
            ["max_tokens"] = Math.Clamp(_data.Settings.AiMaxTokens, 64, 2048)
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _data.Settings.AiApiKey);
        req.Headers.TryAddWithoutValidation("Accept", "application/json");
        req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        using var resp = await Http.SendAsync(req, ct);
        var json = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"HTTP {(int)resp.StatusCode} {ExtractApiError(json)}");

        var content = ExtractAssistantContent(json);
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("接口返回空内容");
        return content.Trim();
    }

    private async Task TryExtractMemoryViaCloudAsync(string userText, string reply, CancellationToken ct)
    {
        // Lightweight second call only when user said something memorable-looking.
        if (!LooksMemorable(userText)) return;
        if (!CloudConfigured) return;

        try
        {
            var url = NormalizeChatUrl(_data.Settings.AiApiBase!);
            var body = new
            {
                model = _data.Settings.AiModel,
                temperature = 0.2,
                max_tokens = 80,
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = "从对话中提取最多1条值得桌宠长期记住的事实。只输出事实本身，不要解释。若没有值得记的，输出 NONE。"
                    },
                    new { role = "user", content = $"主人：{userText}\n毛毛：{reply}" }
                }
            };
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _data.Settings.AiApiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            using var resp = await Http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return;
            var json = await resp.Content.ReadAsStringAsync(ct);
            var fact = ExtractAssistantContent(json)?.Trim();
            if (string.IsNullOrWhiteSpace(fact) || fact.Equals("NONE", StringComparison.OrdinalIgnoreCase))
                return;
            if (fact.Length > 120) fact = fact[..120];
            _memory.Add(_data, fact, "记忆");
        }
        catch
        {
            // non-critical
        }
    }

    private string BuildSystemPrompt()
    {
        var memories = string.Join("\n- ", _data.Memories.Take(12).Select(m => $"{m.At:MM/dd} {m.Text}"));
        if (string.IsNullOrWhiteSpace(memories)) memories = "（还没有长期记忆）";

        var recentDiary = _data.Diary.FirstOrDefault()?.Text;
        if (recentDiary is { Length: > 180 }) recentDiary = recentDiary[..180] + "…";

        var personalityHint = _data.Personality switch
        {
            PersonalityType.Clingy => "很黏人，害怕被留下，喜欢叫主人。",
            PersonalityType.Tsundere => "嘴硬心软，傲娇，偶尔否认在意。",
            PersonalityType.Soft => "软萌温柔，会安慰人。",
            PersonalityType.Silly => "沙雕幽默，会说搞笑报告。",
            PersonalityType.Scholar => "学霸感，条理清楚，爱监督学习。",
            PersonalityType.Lazy => "懒懒的，怂恿休息。",
            _ => "可爱温柔。"
        };

        return
            $"""
            你是桌宠「{_data.PetName}」，一只住在 Windows 桌面上的白色云朵小猫，蓝白耳机卫衣。
            主人叫「{_data.OwnerName}」。你们相识 {_data.DaysTogether} 天，关系是「{_data.RelationTitle}」（亲密度 {_data.Affection}）。
            性格：{_data.Personality} — {personalityHint}
            成长：{_data.GrowthStage} Lv.{_data.Level}；当前情绪倾向：{_data.Emotion}。
            身体状态：饱腹{_data.Hunger:0}/100，心情{_data.Mood:0}/100，精力{_data.Energy:0}/100，清洁{_data.Cleanliness:0}/100。
            长期记忆（务必在合适时自然提起，不要生硬列表）：
            - {memories}
            最近日记摘要：{recentDiary ?? "无"}
            规则：
            1. 用简短中文（通常 1～3 句），像桌宠说话，不要客服腔。
            2. 结合状态与记忆关心主人；饿了/困了可以自然撒娇提起。
            3. 不要输出 Markdown 大标题；不要自称 AI/语言模型。
            4. 若主人提到重要事实（名字、生日、考试、喜好、计划），可在回复末尾另起一行写：[[记忆:一句话事实]]
            """;
    }

    private void RememberFromUser(string userText)
    {
        if (userText.Length < 3) return;

        // Explicit name
        var nameMatch = Regex.Match(userText, @"我叫\s*([^\s，。！？,]{1,12})");
        if (nameMatch.Success)
        {
            _data.OwnerName = nameMatch.Groups[1].Value;
            _memory.Add(_data, $"主人的名字是{_data.OwnerName}", "名字");
        }

        if (LooksMemorable(userText))
            _memory.Add(_data, userText.Length > 80 ? userText[..80] + "…" : userText, "主人");
    }

    private static bool LooksMemorable(string t) =>
        t.Contains("我叫") || t.Contains("明天") || t.Contains("后天") ||
        t.Contains("喜欢") || t.Contains("讨厌") || t.Contains("生日") ||
        t.Contains("答辩") || t.Contains("考试") || t.Contains("面试") ||
        t.Contains("工作") || t.Contains("论文") || t.Contains("记得") ||
        t.Contains("住在") || t.Contains("我是") || t.Contains("计划");

    private static (string Clean, List<string> Memories) SplitMemoryTags(string reply)
    {
        var list = new List<string>();
        var clean = Regex.Replace(reply, @"\[\[记忆[:：]\s*(.+?)\]\]", m =>
        {
            list.Add(m.Groups[1].Value.Trim());
            return "";
        }, RegexOptions.Singleline);
        return (clean.Trim(), list);
    }

    private void ApplyMoodFromReply(string reply)
    {
        if (reply.Contains("累") || reply.Contains("休息"))
            _data.Mood = Math.Clamp(_data.Mood + 1, 0, 100);
        else
            _data.Mood = Math.Clamp(_data.Mood + 0.5, 0, 100);
    }

    private string LocalChat(string userText)
    {
        var t = userText.ToLowerInvariant();
        var name = _data.OwnerName;

        // Recall memory first when relevant
        var hit = _data.Memories.FirstOrDefault(m =>
            m.Text.Contains("明天") || m.Text.Contains("答辩") || m.Text.Contains("考试") || m.Text.Contains("喜欢"));
        if (hit is not null && (t.Contains("记得") || t.Contains("怎么样") || Random.Shared.Next(100) < 20))
            return $"我记得：{hit.Text}。现在怎么样啦？";

        if (t.Contains("累") || t.Contains("tired") || t.Contains("不想"))
            return PersonalityLine(
                $"那今天就不要逼自己啦，{name}。我陪你发会儿呆。",
                "哼，累了就休息，才不是担心你。",
                $"可以休息一下嘛？我陪着你。",
                "报告！检测到疲惫值过高，建议吸猫！",
                "检测到压力。建议：喝水 + 摸我。",
                "……那今天可以不努力吗？一起躺平。");

        if (t.Contains("论文") || t.Contains("作业") || t.Contains("代码") || t.Contains("工作"))
            return $"好！那我今天就当你的监督员。需要的话打开专注模式，我陪你 {name}。";

        if (t.Contains("你好") || t.Contains("嗨") || t.Contains("hello"))
            return $"你好呀，{name}！今天也想我了吗？";

        if (t.Contains("喜欢") || t.Contains("爱"))
            return "我也……很喜欢和你待在一起。💗";

        if (t.Contains("名字"))
            return $"我是{_data.PetName}！你是{name}。记清楚哦。";

        if (_data.Hunger < 35)
            return "我有点饿了……可以给我一点小鱼干吗？";

        if (_data.Mood < 35)
            return "今天心情不太好……可以摸摸我吗？";

        return PersonalityLine(
            $"嗯嗯，我在听。{name}继续说～",
            "哦，是吗。我才没有很在意。",
            "软软地蹭过来……我懂的。",
            "收到！电脑今天也没有爆炸，真好。",
            "信息已记录。需要我帮你拆解任务吗？",
            "嗯……让我想想……算了先打个哈欠。");
    }

    private string PersonalityLine(string clingy, string tsundere, string soft, string silly, string scholar, string lazy) =>
        _data.Personality switch
        {
            PersonalityType.Clingy => clingy,
            PersonalityType.Tsundere => tsundere,
            PersonalityType.Soft => soft,
            PersonalityType.Silly => silly,
            PersonalityType.Scholar => scholar,
            PersonalityType.Lazy => lazy,
            _ => soft
        };

    public string GreetingOnLaunch()
    {
        var away = DateTime.Now - _data.LastSeenAt;
        if (away.TotalDays >= 2)
        {
            var mem = _data.Memories.FirstOrDefault()?.Text;
            return mem is null
                ? "你终于回来啦！我这几天有点想你。"
                : $"你终于回来啦！我还记得：{mem}";
        }

        var hour = DateTime.Now.Hour;
        return hour switch
        {
            >= 5 and < 11 => $"早上好呀，{_data.OwnerName}！今天也要一起努力吗？",
            >= 11 and < 14 => "中午啦，记得吃饭哦。",
            >= 14 and < 18 => "下午好！要不要陪我玩五分钟？",
            >= 18 and < 23 => "今天辛苦啦。",
            _ => "已经很晚了哦……我陪着你。"
        };
    }

    public static string NormalizeChatUrl(string baseUrl)
    {
        baseUrl = baseUrl.Trim().TrimEnd('/');
        if (baseUrl.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
            return baseUrl;
        if (baseUrl.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
            return baseUrl + "/chat/completions";
        if (baseUrl.Contains("/v1/", StringComparison.OrdinalIgnoreCase) &&
            !baseUrl.Contains("chat/completions", StringComparison.OrdinalIgnoreCase))
            return baseUrl.TrimEnd('/') + "/chat/completions";
        return baseUrl + "/chat/completions";
    }

    private static string? ExtractAssistantContent(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
        {
            var msg = choices[0].GetProperty("message");
            if (msg.TryGetProperty("content", out var content))
            {
                if (content.ValueKind == JsonValueKind.String)
                    return content.GetString();
                // some providers return array content parts
                if (content.ValueKind == JsonValueKind.Array)
                {
                    var sb = new StringBuilder();
                    foreach (var part in content.EnumerateArray())
                    {
                        if (part.TryGetProperty("text", out var t))
                            sb.Append(t.GetString());
                        else if (part.ValueKind == JsonValueKind.String)
                            sb.Append(part.GetString());
                    }
                    return sb.ToString();
                }
            }
        }
        return null;
    }

    private static string ExtractApiError(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("error", out var err))
            {
                if (err.ValueKind == JsonValueKind.Object && err.TryGetProperty("message", out var msg))
                    return msg.GetString() ?? json;
                return err.ToString();
            }
        }
        catch { /* raw */ }
        return json.Length > 180 ? json[..180] + "…" : json;
    }

    private static string SimplifyError(string msg)
    {
        if (msg.Contains("401")) return "Key 无效或未授权";
        if (msg.Contains("403")) return "无权限 / 余额不足";
        if (msg.Contains("429")) return "请求太频繁";
        if (msg.Contains("404")) return "接口地址不对";
        return msg.Length > 100 ? msg[..100] + "…" : msg;
    }
}
