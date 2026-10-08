using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using MaomaoDesktopPet.Models;

namespace MaomaoDesktopPet.Services;

public sealed class DesktopCompanionService
{
    private readonly PetData _data;
    private bool? _lastNetwork;

    public DesktopCompanionService(PetData data) => _data = data;

    public string? TickReminders()
    {
        var s = _data.Settings;
        if (s.DisturbMode == DisturbMode.Quiet) return null;

        if (s.WaterReminder &&
            (DateTime.Now - _data.LastWaterRemindAt).TotalMinutes >= 55 &&
            GetIdleMinutes() < 2)
        {
            _data.LastWaterRemindAt = DateTime.Now;
            return "偷偷提醒一下～喝口水吧 💧";
        }

        if (s.WorkReminder && GetIdleMinutes() < 1 && GetBusyMinutesApprox() >= 50)
            return "已经坐很久啦。看看远处，起来走走～";

        if (s.SystemStatusReact)
        {
            var net = NetworkInterface.GetIsNetworkAvailable();
            if (_lastNetwork is true && !net)
            {
                _lastNetwork = net;
                return "网络好像跑掉了。";
            }
            if (_lastNetwork is false && net)
            {
                _lastNetwork = net;
                return "网络回来啦！";
            }
            _lastNetwork ??= net;
        }

        return null;
    }

    public static double GetIdleMinutes()
    {
        var info = new LastInputInfo { CbSize = (uint)Marshal.SizeOf<LastInputInfo>() };
        if (!GetLastInputInfo(ref info)) return 0;
        var idleMs = Environment.TickCount - (int)info.DwTime;
        return Math.Max(0, idleMs / 60000.0);
    }

    // Rough: if user was active recently for a long session we can't measure perfectly;
    // use companion hours modulo as a soft proxy + low idle.
    private double GetBusyMinutesApprox() =>
        (_data.TotalCompanionHours * 60) % 90;

    [StructLayout(LayoutKind.Sequential)]
    private struct LastInputInfo
    {
        public uint CbSize;
        public uint DwTime;
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LastInputInfo plii);
}
