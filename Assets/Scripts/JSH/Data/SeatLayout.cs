using UnityEngine;

namespace JSH.Data
{
    /// <summary>
    /// Seat coordinates of a train car sprite (array order = seat number). Shared by every car that uses the
    /// same sprite. Select a TrainCar in the Scene view to see the seats drawn as gizmos.
    /// </summary>
    [CreateAssetMenu(fileName = "SeatLayout", menuName = "JSH/Train/Seat Layout")]
    public class SeatLayout : ScriptableObject
    {
        [SerializeField] private SeatPoint[] _seats = new SeatPoint[0];

        public int SeatCount => _seats != null ? _seats.Length : 0;

        public SeatPoint GetSeat(int index) => _seats[index];
    }
}
