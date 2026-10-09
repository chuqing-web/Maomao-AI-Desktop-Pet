namespace MaomaoDesktopPet.Services;

public static class FeatureGate
{
    /// <summary>All features are unlocked — play freely.</summary>
    public static bool Can(string feature, out string? reason)
    {
        reason = null;
        return true;
    }
}
