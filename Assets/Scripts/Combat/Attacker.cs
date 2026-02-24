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
        [SerializeField] private bool showGizmoWhenNotSelected;
        [SerializeField] private Color attackRangeGizmoColor = Color.red;
        [SerializeField] private Color stopRangeGizmoColor = Color.yellow;
        [SerializeField] private bool showColliderDistanceDebug = true;
        [SerializeField] private Color inRangeDebugColor = Color.green;
        [SerializeField] private Color outOfRangeDebugColor = Color.cyan;

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
        private void OnDrawGizmos()
        {
            if (!showGizmoWhenNotSelected)
            {
                return;
            }

            DrawRangeGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            DrawRangeGizmos();
        }

        private void DrawRangeGizmos()
        {
            Unit currentUnit = unit != null ? unit : GetComponent<Unit>();
            if (currentUnit == null || currentUnit.Stats == null)
            {
                return;
            }

            Collider2D ownCollider = currentUnit.Collider;
            if (ownCollider == null)
            {
                ownCollider = GetComponent<Collider2D>();
            }

            if (showAttackRangeGizmo)
            {
                float attackRange = currentUnit.Stats.AttackRange;
                float stopRange = Mathf.Max(0f, attackRange - StopEpsilon);
                float ownRadius = GetApproxColliderRadius(ownCollider);

                float attackVisualRadius = attackRange + ownRadius;
                float stopVisualRadius = stopRange + ownRadius;

                Gizmos.color = attackRangeGizmoColor;
                Gizmos.DrawWireSphere(transform.position, attackVisualRadius);

                Gizmos.color = stopRangeGizmoColor;
                Gizmos.DrawWireSphere(transform.position, stopVisualRadius);
            }

            DrawColliderDistanceDebug(currentUnit);
        }

        private static float GetApproxColliderRadius(Collider2D collider)
        {
            if (collider == null)
            {
                return 0f;
            }

            if (collider is CircleCollider2D circle)
            {
                Vector3 lossyScale = circle.transform.lossyScale;
                float maxScale = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));
                return circle.radius * maxScale;
            }

            Bounds bounds = collider.bounds;
            return Mathf.Max(bounds.extents.x, bounds.extents.y);
        }

        private void DrawColliderDistanceDebug(Unit currentUnit)
        {
            if (!showColliderDistanceDebug || currentUnit == null)
            {
                return;
            }

            Collider2D ownCollider = currentUnit.Collider;
            if (ownCollider == null)
            {
                ownCollider = GetComponent<Collider2D>();
            }

            if (ownCollider == null)
            {
                return;
            }

            TargetScanner scanner = currentUnit.GetComponent<TargetScanner>();
            if (scanner == null)
            {
                return;
            }

            Unit target = scanner.CurrentTarget;
            if (target == null || !target.gameObject.activeInHierarchy || target.Collider == null)
            {
                return;
            }

            ColliderDistance2D distance = ownCollider.Distance(target.Collider);
            float attackRange = currentUnit.Stats.AttackRange;
            bool inRange = distance.distance <= attackRange;

            Gizmos.color = inRange ? inRangeDebugColor : outOfRangeDebugColor;
            Gizmos.DrawLine(distance.pointA, distance.pointB);
            Gizmos.DrawWireSphere(distance.pointA, 0.04f);
            Gizmos.DrawWireSphere(distance.pointB, 0.04f);
        }
#endif
    }
}
