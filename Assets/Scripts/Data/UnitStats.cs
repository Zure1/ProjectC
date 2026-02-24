using UnityEngine;

namespace AutoBattler.Core
{
    /// <summary>
    /// Immutable authoring data for a combat unit.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitStats", menuName = "AutoBattler/Unit Stats")]
    public sealed class UnitStats : ScriptableObject
    {
        private const float MinCooldown = 0.01f;
        private const float MinProjectileSpeed = 0.01f;

        /// <summary>
        /// Gets the maximum health value for the unit.
        /// </summary>
        [field: SerializeField]
        public float MaxHP { get; private set; } = 1f;

        /// <summary>
        /// Gets the base damage dealt by the unit.
        /// </summary>
        [field: SerializeField]
        public float Damage { get; private set; } = 1f;

        /// <summary>
        /// Gets the movement speed for the unit.
        /// </summary>
        [field: SerializeField]
        public float MoveSpeed { get; private set; } = 1f;

        /// <summary>
        /// Gets the attack range for the unit.
        /// </summary>
        [field: SerializeField]
        public float AttackRange { get; private set; } = 1f;

        /// <summary>
        /// Gets the minimum time between attacks for the unit.
        /// </summary>
        [field: SerializeField]
        public float AttackCooldown { get; private set; } = 1f;

        /// <summary>
        /// Gets which attack execution type this unit uses.
        /// </summary>
        [field: SerializeField]
        public AttackType AttackType { get; private set; } = AttackType.Melee;

        /// <summary>
        /// Gets projectile speed for future ranged attacks.
        /// </summary>
        [field: SerializeField]
        public float ProjectileSpeed { get; private set; } = 6f;

        /// <summary>
        /// Gets projectile prefab for future ranged attacks.
        /// </summary>
        [field: SerializeField]
        public GameObject ProjectilePrefab { get; private set; }

        private void OnValidate()
        {
            MaxHP = Mathf.Max(0f, MaxHP);
            Damage = Mathf.Max(0f, Damage);
            MoveSpeed = Mathf.Max(0f, MoveSpeed);
            AttackRange = Mathf.Max(0f, AttackRange);
            AttackCooldown = Mathf.Max(MinCooldown, AttackCooldown);
            ProjectileSpeed = Mathf.Max(MinProjectileSpeed, ProjectileSpeed);
        }
    }
}
