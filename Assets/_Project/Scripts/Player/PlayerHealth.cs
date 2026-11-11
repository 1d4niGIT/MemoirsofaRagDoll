using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Vida del jugador (los "hilos" de Mino). Implementa IDamageable: las amenazas solo conocen
    /// esa interfaz. Esta clase NO sabe nada de barras de vida, sonidos ni animaciones:
    /// solo gestiona el numero y publica eventos para que otros sistemas reaccionen (S de SOLID).
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 5;
        [SerializeField] private float invulnerabilityTime = 1f; // margen tras recibir dano

        private float invulnerableUntil;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsAlive => CurrentHealth > 0;
        public bool IsInvulnerable => Time.time < invulnerableUntil;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        private void Start()
        {
            // Avisa el valor inicial para que la barra de vida arranque llena.
            GameEvents.RaiseHealthChanged(CurrentHealth, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || IsInvulnerable || amount <= 0) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

            GameEvents.RaisePlayerDamaged(amount);
            GameEvents.RaiseHealthChanged(CurrentHealth, maxHealth);

            if (CurrentHealth == 0)
            {
                GameEvents.RaisePlayerDied();
                return;
            }

            invulnerableUntil = Time.time + invulnerabilityTime;
        }

        /// <summary>Para curarse con aguja e hilo mas adelante.</summary>
        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0) return;

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            GameEvents.RaiseHealthChanged(CurrentHealth, maxHealth);
        }
    }
}
