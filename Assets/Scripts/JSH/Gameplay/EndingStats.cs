namespace JSH.Gameplay
{
    /// <summary>
    /// Result numbers for the ending message. The gameplay scene stores them with SetLast before loading the
    /// ending scene; the ending UI reads them with TryGetLast (static, survives scene loads).
    /// </summary>
    public readonly struct EndingStats
    {
        public readonly int ReportCount;      // related reports received
        public readonly int EmbracedCount;    // villains embraced
        public readonly int NotEmbracedCount; // villains not embraced
        public readonly int MistakeCount;     // mistaken investigations
        public readonly PeaceGrade Grade;     // peace grade of the line

        public EndingStats(int reportCount, int embracedCount, int notEmbracedCount, int mistakeCount, PeaceGrade grade)
        {
            ReportCount = reportCount;
            EmbracedCount = embracedCount;
            NotEmbracedCount = notEmbracedCount;
            MistakeCount = mistakeCount;
            Grade = grade;
        }

        private static EndingStats _last;
        private static bool _hasLast;

        public static void SetLast(EndingStats stats)
        {
            _last = stats;
            _hasLast = true;
        }

        public static bool TryGetLast(out EndingStats stats)
        {
            stats = _last;
            return _hasLast;
        }

        public static void ClearLast() => _hasLast = false;
    }
}
