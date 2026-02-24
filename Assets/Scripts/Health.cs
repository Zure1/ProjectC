using System;
using UnityEngine;

namespace AutoBattler.Core
{
    /// <summary>
    /// Owns hit point state and death lifecycle for a unit.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Unit))]
    public sealed class Health : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float fallbackMaxHp = 1f;
        [SerializeField] private bool enableDebugLogs;
#if UNITY_EDITOR
        [SerializeField, Min(0f)] private float debugDamageAmount = 1f;
#endif

        private Unit unit;
        private bool deactivationQueued;
        private int deactivationFrame = -1;

        /// <summary>
        /// Raised whenever damage is applied.
        /// </summary>
        /// <remarks>Parameters: damage amount, new HP, damage source.</remarks>
        public event Action<float, float, Unit> OnDamaged;

        /// <summary>
        /// Raised once when this unit dies.
        /// </summary>
        /// <remarks>Parameter: killer unit.</remarks>
        public event Action<Unit> OnDied;

        /// <summary>
        /// Gets the current hit points.
        /// </summary>
        public float CurrentHp { get; private set; }

        /// <summary>
        /// Gets the max hit points.
        /// </summary>
        public float MaxHp { get; private set; }

        /// <summary>
        /// Gets whether this unit is dead.
        /// </summary>
        public bool IsDead { get; private set; }

        private void Awake()
        {
            unit = GetComponent<Unit>();
            RefreshMaxHpFromUnit();
            CurrentHp = MaxHp;
            IsDead = false;
            deactivationQueued = false;
            deactivationFrame = -1;

            if (unit == null)
            {
                Debug.LogError($"[{nameof(Health)}] Missing required {nameof(Unit)} on '{name}'.", this);
            }
        }

        private void OnDisable()
        {
            // If an external system disables this object before the scheduled pass,
            // clear the pending flag so a later re-enable does not auto-disable again.
            deactivationQueued = false;
            deactivationFrame = -1;
        }

        private void Update()
        {
            if (!deactivationQueued || Time.frameCount < deactivationFrame)
            {
                return;
            }

            deactivationQueued = false;
            deactivationFrame = -1;

            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Restores HP to max and clears dead state.
        /// </summary>
        public void ResetToMax()
        {
            RefreshMaxHpFromUnit();
            CurrentHp = MaxHp;
            IsDead = false;
            deactivationQueued = false;
            deactivationFrame = -1;

            if (enableDebugLogs)
            {
                Debug.Log($"[{nameof(Health)}] '{name}' reset to {CurrentHp:0.##}/{MaxHp:0.##} HP.", this);
            }

            if (unit != null && gameObject.activeInHierarchy)
            {
                unit.RegisterIfPossible();
            }
        }

        /// <summary>
        /// Applies incoming damage to this unit.
        /// </summary>
        /// <param name="amount">Incoming damage amount (clamped to 0+).</param>
        /// <param name="source">The source unit dealing damage.</param>
        public void TakeDamage(float amount, Unit source)
        {
            if (IsDead)
            {
                return;
            }

            float clampedAmount = SanitizeDamage(amount);
            float newHp = CurrentHp - clampedAmount;
            if (newHp < 0f)
            {
                newHp = 0f;
            }

            CurrentHp = newHp;

            OnDamaged?.Invoke(clampedAmount, CurrentHp, source);

            if (enableDebugLogs)
            {
                string sourceName = source != null ? source.name : "None";
                Debug.Log($"[{nameof(Health)}] '{name}' took {clampedAmount:0.##} damage from '{sourceName}'. HP: {CurrentHp:0.##}/{MaxHp:0.##}.", this);
            }

            if (CurrentHp <= 0f)
            {
                HandleDeath(source);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Debug/Apply Test Damage")]
        private void DebugApplyTestDamage()
        {
            TakeDamage(debugDamageAmount, null);
        }
#endif

        private void HandleDeath(Unit killer)
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;

            if (unit == null)
            {
                unit = GetComponent<Unit>();
            }

            if (unit != null)
            {
                BattleManager manager = BattleManager.Instance;
                if (manager != null)
                {
                    manager.Unregister(unit);
                }

                unit.MarkUnregisteredFromBattleManager();
            }

            OnDied?.Invoke(killer);

            deactivationQueued = true;
            deactivationFrame = Time.frameCount + 1;

            if (enableDebugLogs)
            {
                string killerName = killer != null ? killer.name : "None";
                Debug.Log($"[{nameof(Health)}] '{name}' died. Killer: '{killerName}'.", this);
            }
        }

        private void RefreshMaxHpFromUnit()
        {
            if (unit != null && unit.Stats != null)
            {
                MaxHp = Mathf.Max(0f, unit.Stats.MaxHP);
                return;
            }

            MaxHp = Mathf.Max(0f, fallbackMaxHp);
        }

        private static float SanitizeDamage(float amount)
        {
            if (float.IsNaN(amount) || float.IsInfinity(amount))
            {
                return 0f;
            }

            return amount < 0f ? 0f : amount;
        }
    }
}
