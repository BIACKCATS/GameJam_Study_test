using JSH.Data;

namespace JSH.Gameplay
{
    /// <summary>
    /// Named villains embraced (converted) during the current run. EndingStatsTracker clears it when a run starts
    /// and marks it on VillainConvertedEvent / VillainDefeatedEvent; EndingController reads it for the true ending lineup.
    /// A villain defeated even once in the run is excluded from the true ending, even if embraced later.
    /// </summary>
    public static class EmbracedVillains
    {
        private static readonly bool[] _embraced = new bool[System.Enum.GetValues(typeof(VillainId)).Length];
        private static readonly bool[] _defeated = new bool[System.Enum.GetValues(typeof(VillainId)).Length];

        public static void Clear()
        {
            System.Array.Clear(_embraced, 0, _embraced.Length);
            System.Array.Clear(_defeated, 0, _defeated.Length);
        }

        public static void Mark(VillainId id)
        {
            if (id == VillainId.None) return;
            _embraced[(int)id] = true;
        }

        public static void MarkDefeated(VillainId id)
        {
            if (id == VillainId.None) return;
            _defeated[(int)id] = true;
        }

        /// <summary>True if embraced this run and never defeated this run.</summary>
        public static bool IsEmbraced(VillainId id) => id != VillainId.None && _embraced[(int)id] && !_defeated[(int)id];
    }
}
