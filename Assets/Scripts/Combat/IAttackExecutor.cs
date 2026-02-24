namespace AutoBattler.Core
{
    /// <summary>
    /// Executes an attack effect once an attack action has started.
    /// </summary>
    public interface IAttackExecutor
    {
        /// <summary>
        /// Executes attack behavior for the given source/target pair.
        /// </summary>
        /// <param name="source">Attacking unit.</param>
        /// <param name="target">Target unit.</param>
        /// <param name="stats">Source stats used by the execution logic.</param>
        void Execute(Unit source, Unit target, UnitStats stats);
    }
}
