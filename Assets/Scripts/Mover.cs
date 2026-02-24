using UnityEngine;

namespace AutoBattler.Core
{
    /// <summary>
    /// Handles physics-based top-down movement toward a target unit.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Unit))]
    public sealed class Mover : MonoBehaviour
    {
        private Unit unit;
        private Unit moveTarget;
        private bool hasMoveTarget;
        private float fixedZ;

        private void Awake()
        {
            unit = GetComponent<Unit>();
            fixedZ = transform.position.z;

            if (unit == null)
            {
                Debug.LogError($"[{nameof(Mover)}] Missing required {nameof(Unit)} on '{name}'.", this);
            }
        }

        /// <summary>
        /// Sets the unit to move toward the provided target.
        /// </summary>
        /// <param name="target">Unit to move toward.</param>
        public void SetMoveTarget(Unit target)
        {
            moveTarget = target;
            hasMoveTarget = target != null;
        }

        /// <summary>
        /// Stops movement immediately.
        /// </summary>
        public void Stop()
        {
            hasMoveTarget = false;
            moveTarget = null;

            if (unit != null && unit.Body != null)
            {
                unit.Body.linearVelocity = Vector2.zero;
            }
        }

        private void FixedUpdate()
        {
            if (unit == null || unit.Body == null || unit.Stats == null)
            {
                return;
            }

            if (!hasMoveTarget || !IsValidMoveTarget(moveTarget))
            {
                unit.Body.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 currentPosition = unit.Body.position;
            Vector3 targetPosition3 = moveTarget.transform.position;
            Vector2 toTarget = new Vector2(targetPosition3.x - currentPosition.x, targetPosition3.y - currentPosition.y);
            float sqrMagnitude = toTarget.sqrMagnitude;

            if (sqrMagnitude <= 0.000001f)
            {
                unit.Body.linearVelocity = Vector2.zero;
            }
            else
            {
                float invMagnitude = 1f / Mathf.Sqrt(sqrMagnitude);
                Vector2 direction = new Vector2(toTarget.x * invMagnitude, toTarget.y * invMagnitude);
                unit.Body.linearVelocity = direction * unit.Stats.MoveSpeed;
            }

            Vector3 position = transform.position;
            if (position.z != fixedZ)
            {
                position.z = fixedZ;
                transform.position = position;
            }
        }

        private static bool IsValidMoveTarget(Unit target)
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
