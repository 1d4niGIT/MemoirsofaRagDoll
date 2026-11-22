using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Camara lenta mientras Mino apunta el alfiler: le da mas tiempo para decidir el tiro.
    ///
    /// No conoce SwingAbility: solo escucha GameEvents.OnAimChanged, que ya publica al apuntar y al
    /// soltar. Opcionalmente solo se activa cuando Mino esta colgado del hilo (ISwingState).
    ///
    /// Baja Time.timeScale rapido al empezar a apuntar y vuelve a 1 de golpe al soltar, para que el
    /// alfiler salga a velocidad normal. El input no se frena: se lee en tiempo real.
    ///
    /// No toca Time.fixedDeltaTime a proposito: asi la guia de trayectoria (ProjectileMath) sigue
    /// coincidiendo con el vuelo real. Para que el movimiento se vea fluido en camara lenta, activa
    /// Interpolate en el Rigidbody2D de Mino y en el del prefab del alfiler.
    ///
    /// Va en el mismo objeto que el jugador.
    /// </summary>
    public class AimSlowMotion : MonoBehaviour
    {
        [SerializeField, Range(0.05f, 1f)] private float slowTimeScale = 0.3f;
        [SerializeField] private float enterSpeed = 8f;
        [SerializeField] private bool onlyWhileSwinging = false;

        private ISwingState swingState;
        private bool aiming;
        private bool paused;
        private bool slowed;

        private void Awake()
        {
            swingState = GetComponent<ISwingState>();
        }

        private void OnEnable()
        {
            GameEvents.OnAimChanged += HandleAimChanged;
            GameEvents.OnGamePaused += HandleGamePaused;
            GameEvents.OnPlayerDied += HandlePlayerDied;
        }

        private void OnDisable()
        {
            GameEvents.OnAimChanged -= HandleAimChanged;
            GameEvents.OnGamePaused -= HandleGamePaused;
            GameEvents.OnPlayerDied -= HandlePlayerDied;

            aiming = false;
            RestoreTime();
        }

        private void Update()
        {
            if (paused)
            {
                slowed = false;
                return;
            }

            if (ShouldSlow())
            {
                slowed = true;
                Time.timeScale = Mathf.MoveTowards(Time.timeScale, slowTimeScale, enterSpeed * Time.unscaledDeltaTime);
            }
            else if (slowed)
            {
                RestoreTime();
            }
        }

        private bool ShouldSlow()
        {
            if (!aiming) return false;
            if (!onlyWhileSwinging) return true;

            return swingState != null && swingState.IsSwinging;
        }

        private void RestoreTime()
        {
            if (!slowed) return;

            slowed = false;
            Time.timeScale = 1f;
        }

        private void HandleAimChanged(bool isAiming, float power01)
        {
            aiming = isAiming;
        }

        private void HandleGamePaused(bool isPaused)
        {
            paused = isPaused;
        }

        private void HandlePlayerDied()
        {
            aiming = false;
            RestoreTime();
        }
    }
}