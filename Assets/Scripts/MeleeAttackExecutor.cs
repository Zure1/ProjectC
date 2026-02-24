namespace AutoBattler.Core
{
    /// <summary>
    /// Executes melee attacks as instant-hit damage.
    /// </summary>
    public sealed class MeleeAttackExecutor : IAttackExecutor
    {
        /// <inheritdoc />
        public void Execute(Unit source, Unit target, UnitStats stats)
        {
            if (source == null || target == null || stats == null)
            {
                return;
            }

            if (!target.gameObject.activeInHierarchy)
            {
                return;
            }

            Health health = target.Health;
            if (health == null || health.IsDead)
            {
                return;
            }

            health.TakeDamage(stats.Damage, source);
        }
    }
}
