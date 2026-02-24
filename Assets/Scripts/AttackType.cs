namespace AutoBattler.Core
{
    /// <summary>
    /// Defines how a unit delivers attacks.
    /// </summary>
    public enum AttackType
    {
        /// <summary>
        /// Instant hit directly on the target.
        /// </summary>
        Melee = 0,

        /// <summary>
        /// Projectile-based attack (reserved for future implementation).
        /// </summary>
        Ranged = 1
    }
}
