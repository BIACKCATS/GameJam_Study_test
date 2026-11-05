namespace JSH.Data
{
    /// <summary>Game-side meaning of a dialogue choice. Listeners (minigames, villain, lives, ending) react to it.</summary>
    public enum DialogueEffect
    {
        None = 0,
        Convert = 1,  // instantly reform a villain (no minigame) - kept for scripted cases
        Accuse = 2,   // "퇴치": judge as villain -> Reflex minigame
        Persuade = 3, // "설득": -> Timing minigame (fail -> Reflex minigame)
    }
}
