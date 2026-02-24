using UnityEngine;

namespace AutoBattler.Core
{
    /// <summary>
    /// Maintains and updates the unit's current target.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Unit))]
    public sealed class TargetScanner : MonoBehaviour
    {
        private Unit unit;
        private ITargetProvider targetProvider;

        /// <summary>
        /// Gets the currently selected target.
        /// </summary>
        public Unit CurrentTarget { get; private set; }

        /// <summary>
        /// Gets whether the scanner currently has a valid target.
        /// </summary>
        public bool HasTarget => CurrentTarget != null;

        private void Awake()
        {
            unit = GetComponent<Unit>();
            targetProvider = new ClosestEnemyTargetProvider();

            if (unit == null)
            {
                Debug.LogError($"[{nameof(TargetScanner)}] Missing required {nameof(Unit)} on '{name}'.", this);
            }
        }

        /// <summary>
        /// Refreshes the current target using the active targeting strategy.
        /// </summary>
        public void UpdateTarget()
        {
            if (IsTargetStillValid(CurrentTarget))
            {
                return;
            }

            BattleManager manager = BattleManager.Instance;
            if (manager == null || targetProvider == null || unit == null)
            {
                CurrentTarget = null;
                return;
            }

            CurrentTarget = targetProvider.FindTarget(unit, manager);
        }

        private static bool IsTargetStillValid(Unit target)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                return false;
            }

            Health health = target.Health;
            if (health != null && health.IsDead)
            {
                return false;
            }

            return true;
        }
    }
}
