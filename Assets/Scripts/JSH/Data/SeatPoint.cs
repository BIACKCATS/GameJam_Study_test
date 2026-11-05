using System;
using UnityEngine;

namespace JSH.Data
{
    /// <summary>One seat in a train car, relative to the car's transform (TrainCar root).</summary>
    [Serializable]
    public struct SeatPoint
    {
        [Tooltip("Position relative to the car root (TrainCar transform). Match it to the seat in the car sprite.")]
        public Vector2 LocalPosition;
        [Tooltip("Flip the seated NPC's sprite horizontally (seat facing).")]
        public bool FlipX;
    }
}
