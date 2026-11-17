using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Vida de Mino = su cantidad de RELLENO (0 a 100). Implementa IDamageable: las amenazas solo
    /// conocen esa interfaz. Tambien implementa IKillable para morir de golpe.
    ///
    /// Regla del hilo: mientras Mino tenga hilo, los golpes le gastan
    /// HILO y su relleno no se toca. Solo cuando el hilo esta en 0 los golpes bajan el relleno.
    /// Sin hilo, ademas, el relleno se va perdiendo solo (ver StarvationDrain).
    /// Esta clase NO sabe nada de barras de vida, sonidos ni animaciones:
    /// solo gestiona el numero y publica eventos para que otros sistemas reaccionen.
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable, IKillable
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float invulnerabilityTime = 1f; // margen tras recibir dano

        private float invulnerableUntil;
        private ThreadSupply thread;   // opcional: si no existe, los golpes van directo a la vida

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsAlive => CurrentHealth > 0;
        public bool IsInvulnerable => Time.time < invulnerableUntil;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            thread = GetComponent<ThreadSupply>();
        }

        private void Start()
        {
            // Avisa el valor inicial para que la barra de vida arranque llena.
            GameEvents.RaiseHealthChanged(CurrentHealth, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || IsInvulnerable || amount <= 0) return;

            // Con hilo, el golpe lo absorbe el hilo (y da el mismo margen de invulnerabilidad).
            if (thread != null && !thread.IsEmpty)
            {
                thread.Drain(amount);
                GameEvents.RaisePlayerDamaged(amount);
                invulnerableUntil = Time.time + invulnerabilityTime;
                return;
            }

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

        /// <summary>
        /// Perdida de relleno por quedarse sin hilo. No da invulnerabilidad ni cuenta como golpe:
        /// es un desgaste continuo que lo llama StarvationDrain.
        /// </summary>
        public void ApplyStarvation(int amount)
        {
            if (!IsAlive || amount <= 0) return;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            GameEvents.RaiseHealthChanged(CurrentHealth, maxHealth);

            if (CurrentHealth == 0)
                GameEvents.RaisePlayerDied();
        }

        /// <summary>Muerte inmediata: ignora la vida restante y la invulnerabilidad.</summary>
        public void Kill()
        {
            if (!IsAlive) return;

            CurrentHealth = 0;
            GameEvents.RaiseHealthChanged(CurrentHealth, maxHealth);
            GameEvents.RaisePlayerDied();
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