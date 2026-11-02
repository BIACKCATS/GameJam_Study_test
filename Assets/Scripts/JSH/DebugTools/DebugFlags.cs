using UnityEngine;

namespace JSH.DebugTools
{
    /// <summary>Global debug switches read by gameplay code (e.g. Health checks GodMode).</summary>
    public static class DebugFlags
    {
        public static bool GodMode;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            GodMode = false;
        }
    }
}
