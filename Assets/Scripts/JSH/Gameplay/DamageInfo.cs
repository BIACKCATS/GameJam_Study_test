using UnityEngine;

namespace JSH.Gameplay
{
    /// <summary>Data for a single hit. Passed by `in` to avoid copies.</summary>
    public readonly struct DamageInfo
    {
        public readonly int Amount;
        public readonly GameObject Source;
        public readonly Vector2 HitPoint;
        public readonly Vector2 Knockback;

        public DamageInfo(int amount, GameObject source, Vector2 hitPoint, Vector2 knockback)
        {
            Amount = amount;
            Source = source;
            HitPoint = hitPoint;
            Knockback = knockback;
        }

        public DamageInfo(int amount, GameObject source = null)
            : this(amount, source, Vector2.zero, Vector2.zero)
        {
        }
    }
}
