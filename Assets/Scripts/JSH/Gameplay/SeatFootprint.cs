using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>
    /// Put on the root of an NPC prefab that is wider than one seat (e.g. a lying villain).
    /// TrainCar seats it where the neighbouring seats (by x order) are free and keeps those seats empty,
    /// so nobody overlaps it. NPCs without this component use one seat.
    /// </summary>
    public class SeatFootprint : MonoBehaviour
    {
        [Tooltip("Seats kept empty on each side (left and right) of this NPC.")]
        [SerializeField, Min(0)] private int _extraSeatsEachSide = 1;

        public int ExtraSeatsEachSide => _extraSeatsEachSide;
    }
}
