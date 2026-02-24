namespace AutoBattler.Core
{
    /// <summary>
    /// Provides a target selection strategy for a unit.
    /// </summary>
    public interface ITargetProvider
    {
        /// <summary>
        /// Finds a target for the owner unit.
        /// </summary>
        /// <param name="owner">The unit requesting a target.</param>
        /// <param name="manager">Battle manager registry.</param>
        /// <returns>Selected target unit, or null if none is valid.</returns>
        Unit FindTarget(Unit owner, BattleManager manager);
    }
}
