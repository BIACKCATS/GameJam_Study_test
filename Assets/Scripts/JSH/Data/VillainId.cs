namespace JSH.Data
{
    /// <summary>
    /// Which named villain a Villain prefab is. Used by the true ending to show only the villains embraced in this run.
    /// None = generic villain (never shown in the true ending).
    /// </summary>
    public enum VillainId
    {
        None,
        Danso,     // 단소
        SleepyMan, // 오래된 남자
        HipHop,    // 힙합 소년
        Jarvan,    // 자르반84세
        Knight     // 기사
    }
}
