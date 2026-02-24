using System.Collections.Generic;
using UnityEngine;

namespace AutoBattler.Core
{
    /// <summary>
    /// Scene-level registry for active battle units.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BattleManager : MonoBehaviour
    {
        private readonly List<Unit> team1Units = new List<Unit>(32);
        private readonly List<Unit> team2Units = new List<Unit>(32);

        /// <summary>
        /// Gets the active battle manager instance in the current scene.
        /// </summary>
        public static BattleManager Instance { get; private set; }

        /// <summary>
        /// Gets currently registered Team1 units.
        /// </summary>
        public IReadOnlyList<Unit> Team1Units => team1Units;

        /// <summary>
        /// Gets currently registered Team2 units.
        /// </summary>
        public IReadOnlyList<Unit> Team2Units => team2Units;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError($"[{nameof(BattleManager)}] Multiple instances found in scene. Keep exactly one active instance.", this);
                enabled = false;
                return;
            }

            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Registers a unit into the team registry.
        /// </summary>
        /// <param name="unit">The unit to register.</param>
        public void Register(Unit unit)
        {
            if (unit == null)
            {
                Debug.LogError($"[{nameof(BattleManager)}] Cannot register a null unit.", this);
                return;
            }

            Health health = unit.Health;
            if (health != null && health.IsDead)
            {
                return;
            }

            List<Unit> targetList = GetList(unit.Team);
            if (targetList.Contains(unit))
            {
                return;
            }

            targetList.Add(unit);
        }

        /// <summary>
        /// Unregisters a unit from the team registry.
        /// </summary>
        /// <param name="unit">The unit to unregister.</param>
        public void Unregister(Unit unit)
        {
            if (unit == null)
            {
                Debug.LogError($"[{nameof(BattleManager)}] Cannot unregister a null unit.", this);
                return;
            }

            List<Unit> targetList = GetList(unit.Team);
            targetList.Remove(unit);
        }

        /// <summary>
        /// Gets the current enemy unit list for the provided team without allocating.
        /// </summary>
        /// <param name="team">The team requesting enemies.</param>
        /// <returns>The existing enemy list reference.</returns>
        public IReadOnlyList<Unit> GetEnemies(UnitTeam team)
        {
            return team == UnitTeam.Team1 ? team2Units : team1Units;
        }

        private List<Unit> GetList(UnitTeam team)
        {
            return team == UnitTeam.Team1 ? team1Units : team2Units;
        }
    }
}
