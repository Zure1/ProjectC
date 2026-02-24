using UnityEngine;

namespace AutoBattler.Core
{
    /// <summary>
    /// Coordinates targeting, movement intent, and attack attempts.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TargetScanner))]
    [RequireComponent(typeof(Mover))]
    [RequireComponent(typeof(Attacker))]
    [RequireComponent(typeof(Unit))]
    public sealed class UnitBrain : MonoBehaviour
    {
        private TargetScanner targetScanner;
        private Mover mover;
        private Attacker attacker;
        private Unit unit;

        private void Awake()
        {
            targetScanner = GetComponent<TargetScanner>();
            mover = GetComponent<Mover>();
            attacker = GetComponent<Attacker>();
            unit = GetComponent<Unit>();

            if (targetScanner == null || mover == null || attacker == null || unit == null)
            {
                Debug.LogError($"[{nameof(UnitBrain)}] Missing required dependencies on '{name}'.", this);
            }
        }

        private void OnDisable()
        {
            if (mover != null)
            {
                mover.Stop();
            }
        }

        private void Update()
        {
            if (targetScanner == null || mover == null || attacker == null || unit == null)
            {
                return;
            }

            // Keep dead units from drifting while still allowing same-step tie outcomes
            // through an already selected target before deactivation.
            Health selfHealth = unit.Health;
            if (selfHealth != null && selfHealth.IsDead)
            {
                mover.Stop();
                targetScanner.UpdateTarget();
                Unit deadUnitTarget = targetScanner.CurrentTarget;
                if (deadUnitTarget != null)
                {
                    attacker.TryStartAttack(deadUnitTarget);
                }

                return;
            }

            targetScanner.UpdateTarget();
            Unit target = targetScanner.CurrentTarget;

            if (target == null)
            {
                mover.Stop();
                return;
            }

            if (attacker.IsWithinStoppingDistance(target))
            {
                mover.Stop();
                attacker.TryStartAttack(target);
                return;
            }

            mover.SetMoveTarget(target);
        }

        private void HandleDeadUnit()
        {
            Health selfHealth = unit.Health;
            if (selfHealth != null && selfHealth.IsDead)
            {
                mover.Stop();
                targetScanner.UpdateTarget();
                Unit deadUnitTarget = targetScanner.CurrentTarget;
                if (deadUnitTarget != null)
                {
                    attacker.TryStartAttack(deadUnitTarget);
                }

                return;
            }
        }
    }
}
