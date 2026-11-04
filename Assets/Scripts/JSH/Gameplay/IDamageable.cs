namespace JSH.Gameplay
{
    /// <summary>Anything that can be hit (Health, breakable props, shields...).</summary>
    public interface IDamageable
    {
        /// <summary>Returns true if the hit was applied (false if invincible / dead).</summary>
        bool TakeDamage(in DamageInfo info);
    }
}
