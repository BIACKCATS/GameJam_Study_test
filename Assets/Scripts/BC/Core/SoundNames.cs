namespace BC.Core
{
    /// <summary>
    /// FMOD event names in SFX.fspro (all under the root folder, so path = "event:/" + name).
    /// Names are case-sensitive: the train music event is lowercase "train".
    /// </summary>
    public static class SoundNames
    {
        // Looping music (both use the Attack + MasterVolume parameters)
        public const string Train = "train";
        public const string Attack = "Attack";

        // One-shots (MasterVolume parameter only)
        public const string UI = "UI";
        public const string Naga = "NAGA";
        public const string MetroDoorOpen = "Metro_door_open";
    }
}
