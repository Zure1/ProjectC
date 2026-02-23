using UnityEngine;

namespace AutoBattler.Core
{
    /// <summary>
    /// Root component for a battle unit.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class Unit : MonoBehaviour
    {
        [SerializeField] private UnitTeam team = UnitTeam.Team1;
        [SerializeField] private UnitStats stats;

        private bool isRegistered;

        /// <summary>
        /// Gets the team this unit belongs to.
        /// </summary>
        public UnitTeam Team => team;

        /// <summary>
        /// Gets the authoring stats assigned to this unit.
        /// </summary>
        public UnitStats Stats => stats;

        /// <summary>
        /// Gets the cached rigidbody component.
        /// </summary>
        public Rigidbody2D Body { get; private set; }

        /// <summary>
        /// Gets the cached collider component.
        /// </summary>
        public Collider2D Collider { get; private set; }

        private void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            Collider = GetComponent<Collider2D>();

            if (Body == null)
            {
                Debug.LogError($"[{nameof(Unit)}] Missing required {nameof(Rigidbody2D)} on '{name}'.", this);
            }

            if (Collider == null)
            {
                Debug.LogError($"[{nameof(Unit)}] Missing required {nameof(Collider2D)} on '{name}'.", this);
            }

            if (stats == null)
            {
                Debug.LogError($"[{nameof(Unit)}] No {nameof(UnitStats)} assigned on '{name}'.", this);
            }
        }

        private void OnEnable()
        {
            TryRegister();
        }

        private void Start()
        {
            TryRegister();
        }

        private void OnDisable()
        {
            TryUnregister();
        }

        private void TryRegister()
        {
            if (isRegistered)
            {
                return;
            }

            BattleManager manager = BattleManager.Instance;
            if (manager == null)
            {
                return;
            }

            manager.Register(this);
            isRegistered = true;
        }

        private void TryUnregister()
        {
            if (!isRegistered)
            {
                return;
            }

            BattleManager manager = BattleManager.Instance;
            if (manager == null)
            {
                isRegistered = false;
                return;
            }

            manager.Unregister(this);
            isRegistered = false;
        }
    }
}
