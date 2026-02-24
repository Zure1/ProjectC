using System;
using UnityEngine;

namespace AutoBattler.Core
{
    /// <summary>
    /// Owns attack cooldown and attack start validation.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Unit))]
    public sealed class Attacker : MonoBehaviour
    {
        private const float StopEpsilon = 0.05f;

        [SerializeField] private bool showAttackRangeGizmo;

        private Unit unit;
        private IAttackExecutor attackExecutor;
        private float cooldownRemaining;

        /// <summary>
        /// Raised when an attack action starts.
        /// </summary>
        public event Action<Unit> OnAttackStarted;

        /// <summary>
        /// Gets whether this unit can start an attack right now.
        /// </summary>
        public bool CanAttack => cooldownRemaining <= 0f;

        private void Awake()
        {
            unit = GetComponent<Unit>();
            attackExecutor = CreateExecutor();

            if (unit == null)
            {
                Debug.LogError($"[{nameof(Attacker)}] Missing required {nameof(Unit)} on '{name}'.", this);
            }
        }

        private void Update()
        {
            if (cooldownRemaining <= 0f)
            {
                return;
            }

            cooldownRemaining -= Time.deltaTime;
            if (cooldownRemaining < 0f)
            {
                cooldownRemaining = 0f;
            }
        }

        /// <summary>
        /// Checks whether a target is within attack range.
        /// </summary>
        /// <param name="target">Target to evaluate.</param>
        /// <returns>True if in attack range.</returns>
        public bool IsInRange(Unit target)
        {
            if (!CanEvaluateTarget(target))
            {
                return false;
            }

            ColliderDistance2D distance = unit.Collider.Distance(target.Collider);
            return distance.distance <= unit.Stats.AttackRange;
        }

        /// <summary>
        /// Checks whether a target is close enough to stop movement and avoid jitter.
        /// </summary>
        /// <param name="target">Target to evaluate.</param>
        /// <returns>True when movement should stop near the target.</returns>
        public bool IsWithinStoppingDistance(Unit target)
        {
            if (!CanEvaluateTarget(target))
            {
                return false;
            }

            float stopRange = unit.Stats.AttackRange - StopEpsilon;
            if (stopRange < 0f)
            {
                stopRange = 0f;
            }

            ColliderDistance2D distance = unit.Collider.Distance(target.Collider);
            return distance.distance <= stopRange;
        }

        /// <summary>
        /// Attempts to start an attack action against the given target.
        /// </summary>
        /// <param name="target">Target to attack.</param>
        public void TryStartAttack(Unit target)
        {
            if (!CanAttack || !CanEvaluateTarget(target) || !IsInRange(target))
            {
                return;
            }

            cooldownRemaining = unit.Stats.AttackCooldown;
            OnAttackStarted?.Invoke(target);

            if (attackExecutor != null)
            {
                attackExecutor.Execute(unit, target, unit.Stats);
            }
        }

        private IAttackExecutor CreateExecutor()
        {
            if (unit == null || unit.Stats == null)
            {
                return new MeleeAttackExecutor();
            }

            return unit.Stats.AttackType == AttackType.Ranged
                ? new RangedAttackExecutor()
                : new MeleeAttackExecutor();
        }

        private bool CanEvaluateTarget(Unit target)
        {
            if (unit == null || unit.Stats == null || unit.Collider == null)
            {
                return false;
            }

            if (target == null || !target.gameObject.activeInHierarchy || target.Collider == null)
            {
                return false;
            }

            if (target.Team == unit.Team)
            {
                return false;
            }

            Health targetHealth = target.Health;
            if (targetHealth != null && targetHealth.IsDead)
            {
                return false;
            }

            return true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!showAttackRangeGizmo)
            {
                return;
            }

            Unit currentUnit = unit != null ? unit : GetComponent<Unit>();
            if (currentUnit == null || currentUnit.Stats == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, currentUnit.Stats.AttackRange);
        }
#endif
    }
}
