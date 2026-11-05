namespace JSH.Data
{
    /// <summary>Game-side meaning of a dialogue choice. Listeners (villain, lives, ending) react to it.</summary>
    public enum DialogueEffect
    {
        None = 0,
        Convert = 1, // villain is reformed
        Accuse = 2,  // player judges this person to be a villain
    }
}
