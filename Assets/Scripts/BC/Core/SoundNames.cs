namespace BC.Core
{
    /// <summary>
    /// FMOD event names in SFX.fspro (all under the root folder, so path = "event:/" + name).
    /// Names are case-sensitive and must match the built banks exactly.
    /// </summary>
    public static class SoundNames
    {
        // Music loops (Attack + MasterVolume) → SoundManager.PlayTrain / StartAttack
        public const string Train = "Train";
        public const string Attack = "Attack";

        // One-shots → SoundManager.PlaySound
        public const string UI = "UI";
        public const string Naga = "NAGA";
        public const string MetroDoorOpen = "Metro_door_open";
        public const string Talk = "Talk";
        public const string StoppingTrain = "Stopping_Train";
        public const string CloseTrainDoor = "Close_Train_door";
        public const string KickTheVillain = "Kick_the_villain";
        public const string TurnOffTheLight = "Turn_Off_The_light";
        public const string MessageSendingAlarm = "Message_Sending_Alarm";
        public const string MessageComingAlarm = "Message_Coming_Alarm";
        public const string HealthLose = "Health_lose";
        public const string HealthAdd = "Health_add";
        public const string Spark = "Spark";
        public const string Aim = "Aim";
        public const string YeYeItso = "YeYeITSO";
        public const string PaperStitching = "Paper_stitching";

        // One-shots without a MasterVolume parameter (volume argument has no effect)
        public const string HecktopascalKick = "Hecktopascal_Kick";
        public const string HecktopascalKicked = "Hecktopascal_Kicked";
        public const string PassingDark = "Passing_Dark";

        // Looping SFX → SoundManager.PlaySound to start, SoundManager.StopSound to stop
        public const string TurnOnTheLight = "Turn_On_The_light";
        public const string Writing = "Writing";             // no MasterVolume parameter
        public const string LetterAdding = "Letter_adding";  // no MasterVolume parameter
    }
}
