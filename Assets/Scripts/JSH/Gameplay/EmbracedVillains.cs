using JSH.Data;

namespace JSH.Gameplay
{
    /// <summary>
    /// Named villains embraced (converted) during the current run. EndingStatsTracker clears it when a run starts
    /// and marks it on VillainConvertedEvent; EndingController reads it for the true ending lineup.
    /// </summary>
    public static class EmbracedVillains
    {
        private static readonly bool[] _embraced = new bool[System.Enum.GetValues(typeof(VillainId)).Length];

        public static void Clear() => System.Array.Clear(_embraced, 0, _embraced.Length);

        public static void Mark(VillainId id)
        {
            if (id == VillainId.None) return;
            _embraced[(int)id] = true;
        }

        public static bool IsEmbraced(VillainId id) => id != VillainId.None && _embraced[(int)id];
    }
}
