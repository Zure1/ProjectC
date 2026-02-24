namespace AutoBattler.Core
{
    /// <summary>
    /// Placeholder executor for future projectile-based attacks.
    /// </summary>
    public sealed class RangedAttackExecutor : IAttackExecutor
    {
        /// <inheritdoc />
        public void Execute(Unit source, Unit target, UnitStats stats)
        {
            // Reserved for future projectile/windup implementation.
            // Intentionally no-op 
        }
    }
}
