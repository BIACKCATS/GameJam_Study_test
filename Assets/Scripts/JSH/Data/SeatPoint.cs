using System;
using UnityEngine;

namespace JSH.Data
{
    /// <summary>One seat in a train car, relative to the car's transform (TrainCar root).</summary>
    [Serializable]
    public struct SeatPoint
    {
        [Tooltip("Position relative to the car root (TrainCar transform). With TrainCar 'Align Feet To Seat' on, " +
                 "this is where the NPC's feet (sprite bottom) go — put it on the floor in front of the seat.")]
        public Vector2 LocalPosition;
        [Tooltip("Flip the seated NPC's sprite horizontally (seat facing).")]
        public bool FlipX;
    }
}
