using System;
using UnityEngine;

namespace RagDoll.Core
{
    /// <summary>
    /// Bus de eventos global. Los sistemas publican (Raise...) y escuchan (+= / -=) sin conocerse.
    ///
    /// Encapsulacion: los eventos son "event", asi que desde afuera solo se puede suscribir o
    /// desuscribir. Invocarlos solo es posible a traves de los metodos Raise... de esta clase.
    ///
    /// Regla de oro: quien se suscribe en OnEnable SIEMPRE se desuscribe en OnDisable.
    /// </summary>
    public static class GameEvents
    {
        // ---------- Jugador ----------
        /// <summary>Se publico dano al jugador. Parametro: cantidad de dano.</summary>
        public static event Action<int> OnPlayerDamaged;

        /// <summary>Un golpe le gasto HILO a Mino (todavia tenia hilo). Parametro: cantidad.</summary>
        public static event Action<int> OnThreadHit;

        /// <summary>Un golpe le quito RELLENO a Mino (ya no tenia hilo). Parametro: cantidad.</summary>
        public static event Action<int> OnStuffingHit;

        /// <summary>Cambio la vida. Parametros: vida actual, vida maxima.</summary>
        public static event Action<int, int> OnHealthChanged;

        /// <summary>El jugador se quedo sin hilos de vida.</summary>
        public static event Action OnPlayerDied;

        /// <summary>El jugador entro (true) o salio (false) de un escondite.</summary>
        public static event Action<bool> OnPlayerHidden;

        /// <summary>
        /// Una amenaza enganchó a alguien. Parametros: collider de la victima, punto del contacto,
        /// cantidad de hilos que se le sueltan (los dibuja ThreadSnagVFX).
        /// </summary>
        public static event Action<Collider2D, Vector2, int> OnSnagged;

        // ---------- Hilo y apuntado ----------
        /// <summary>Cambio el hilo. Parametros: hilo actual, hilo maximo.</summary>
        public static event Action<float, float> OnThreadChanged;

        /// <summary>Apuntando el alfiler. Parametros: esta apuntando, fuerza de 0 a 1.</summary>
        public static event Action<bool, float> OnAimChanged;

        // ---------- Boss ----------
        /// <summary>El boss Ekeko aparecio en escena.</summary>
        public static event Action OnBossSpawned;

        /// <summary>El boss Ekeko se retiro de la escena.</summary>
        public static event Action OnBossLeft;

        // ---------- Flujo de juego ----------
        /// <summary>Pausa activada (true) o desactivada (false).</summary>
        public static event Action<bool> OnGamePaused;

        /// <summary>Empezo un nivel. Parametro: indice del nivel (0, 1, 2).</summary>
        public static event Action<int> OnLevelStarted;

        /// <summary>Se completo el nivel actual.</summary>
        public static event Action OnLevelCompleted;

        // ---------- Metodos para publicar ----------
        public static void RaisePlayerDamaged(int amount) => OnPlayerDamaged?.Invoke(amount);
        public static void RaiseThreadHit(int amount) => OnThreadHit?.Invoke(amount);
        public static void RaiseStuffingHit(int amount) => OnStuffingHit?.Invoke(amount);
        public static void RaiseHealthChanged(int current, int max) => OnHealthChanged?.Invoke(current, max);
        public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
        public static void RaisePlayerHidden(bool isHidden) => OnPlayerHidden?.Invoke(isHidden);
        public static void RaiseSnagged(Collider2D victim, Vector2 contactPoint, int lineCount) => OnSnagged?.Invoke(victim, contactPoint, lineCount);

        public static void RaiseThreadChanged(float current, float max) => OnThreadChanged?.Invoke(current, max);
        public static void RaiseAimChanged(bool isAiming, float power01) => OnAimChanged?.Invoke(isAiming, power01);

        public static void RaiseBossSpawned() => OnBossSpawned?.Invoke();
        public static void RaiseBossLeft() => OnBossLeft?.Invoke();

        public static void RaiseGamePaused(bool isPaused) => OnGamePaused?.Invoke(isPaused);
        public static void RaiseLevelStarted(int levelIndex) => OnLevelStarted?.Invoke(levelIndex);
        public static void RaiseLevelCompleted() => OnLevelCompleted?.Invoke();

        // ---------- Limpieza ----------
        /// <summary>
        /// Los campos estaticos sobreviven entre ejecuciones en el editor si el "Domain Reload"
        /// esta desactivado. Esto limpia todos los suscriptores al iniciar Play.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetAll()
        {
            OnPlayerDamaged = null;
            OnThreadHit = null;
            OnStuffingHit = null;
            OnHealthChanged = null;
            OnPlayerDied = null;
            OnPlayerHidden = null;
            OnSnagged = null;
            OnThreadChanged = null;
            OnAimChanged = null;
            OnBossSpawned = null;
            OnBossLeft = null;
            OnGamePaused = null;
            OnLevelStarted = null;
            OnLevelCompleted = null;
        }
    }
}