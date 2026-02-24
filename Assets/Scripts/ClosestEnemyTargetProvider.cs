using System.Collections.Generic;
using UnityEngine;

namespace AutoBattler.Core
{
    /// <summary>
    /// Chooses the closest valid enemy by center distance.
    /// </summary>
    public sealed class ClosestEnemyTargetProvider : ITargetProvider
    {
        /// <inheritdoc />
        public Unit FindTarget(Unit owner, BattleManager manager)
        {
            if (owner == null || manager == null)
            {
                return null;
            }

            IReadOnlyList<Unit> enemies = manager.GetEnemies(owner.Team);
            Unit bestTarget = null;
            float bestDistanceSqr = float.MaxValue;
            Vector3 ownerPosition = owner.transform.position;

            for (int i = 0; i < enemies.Count; i++)
            {
                Unit candidate = enemies[i];
                if (!IsValidTarget(candidate))
                {
                    continue;
                }

                Vector3 delta = candidate.transform.position - ownerPosition;
                float distanceSqr = delta.x * delta.x + delta.y * delta.y;
                if (distanceSqr < bestDistanceSqr)
                {
                    bestDistanceSqr = distanceSqr;
                    bestTarget = candidate;
                }
            }

            return bestTarget;
        }

        private static bool IsValidTarget(Unit unit)
        {
            if (unit == null || !unit.gameObject.activeInHierarchy)
            {
                return false;
            }

            Health health = unit.Health;
            if (health != null && health.IsDead)
            {
                return false;
            }

            return true;
        }
    }
}
