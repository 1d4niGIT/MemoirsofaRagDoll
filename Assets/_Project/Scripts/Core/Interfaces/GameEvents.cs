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

        /// <summary>Cambio la vida. Parametros: vida actual, vida maxima.</summary>
        public static event Action<int, int> OnHealthChanged;

        /// <summary>El jugador se quedo sin hilos.</summary>
        public static event Action OnPlayerDied;

        /// <summary>El jugador entro (true) o salio (false) de un escondite.</summary>
        public static event Action<bool> OnPlayerHidden;

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
        public static void RaiseHealthChanged(int current, int max) => OnHealthChanged?.Invoke(current, max);
        public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
        public static void RaisePlayerHidden(bool isHidden) => OnPlayerHidden?.Invoke(isHidden);

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
            OnHealthChanged = null;
            OnPlayerDied = null;
            OnPlayerHidden = null;
            OnBossSpawned = null;
            OnBossLeft = null;
            OnGamePaused = null;
            OnLevelStarted = null;
            OnLevelCompleted = null;
        }
    }
}
