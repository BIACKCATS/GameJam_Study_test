using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// OBSOLETE: boundary trigger from the old multi-platform layout. Replaced by CarExit (one-screen car x N).
    /// Kept only so Platform.prefab still compiles; it does nothing. Delete via the Editor during cleanup.
    /// </summary>
    public class CarEntrance : MonoBehaviour
    {
        private void Awake() => Debug.LogWarning($"[CarEntrance] {name}: obsolete (use CarExit). This component does nothing.", this);
    }
}
