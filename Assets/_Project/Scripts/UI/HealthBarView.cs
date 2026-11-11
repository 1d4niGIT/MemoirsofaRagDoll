using UnityEngine;
using UnityEngine.UI;
using RagDoll.Core;

namespace RagDoll.UI
{
    /// <summary>
    /// Barra de vida. Solo ESCUCHA el evento OnHealthChanged: no conoce a PlayerHealth
    /// ni al jugador, asi que funciona sin referencias directas entre ambos.
    ///
    /// Configuracion: una Image con "Image Type = Filled" y "Fill Method = Horizontal"
    /// (necesita un Source Image asignado, aunque sea un cuadrado blanco).
    /// </summary>
    public class HealthBarView : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private float smoothSpeed = 4f;

        private float targetFill = 1f;
        private bool initialized;

        private void OnEnable()  => GameEvents.OnHealthChanged += HandleHealthChanged;
        private void OnDisable() => GameEvents.OnHealthChanged -= HandleHealthChanged;

        private void HandleHealthChanged(int current, int max)
        {
            targetFill = max > 0 ? (float)current / max : 0f;

            // El primer valor se aplica directo para que la barra no "baje desde 1" al empezar.
            if (!initialized && fillImage != null)
            {
                fillImage.fillAmount = targetFill;
                initialized = true;
            }
        }

        private void Update()
        {
            if (fillImage == null) return;

            // unscaledDeltaTime: la barra sigue animandose aunque el juego este en pausa.
            fillImage.fillAmount = Mathf.MoveTowards(
                fillImage.fillAmount, targetFill, smoothSpeed * Time.unscaledDeltaTime);
        }
    }
}
