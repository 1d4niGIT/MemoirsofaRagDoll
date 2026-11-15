using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RagDoll.Core;

namespace RagDoll.UI
{
    /// <summary>
    /// Contador de hilo en pantalla. Solo ESCUCHA el evento OnThreadChanged: no conoce a ThreadSupply
    /// ni a Mino, asi que se conecta sin referencias directas (igual que la barra de vida).
    ///
    /// Muestra el hilo con una imagen de relleno (Image Type = Filled: Horizontal, Vertical o Radial 360,
    /// segun la forma de tu contador) y, opcionalmente, con un numero.
    /// </summary>
    public class ThreadBarView : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private float smoothSpeed = 4f;

        [Header("Contador numerico (opcional)")]
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private string format = "{0}"; // "{0}" = hilo actual; "{0}/{1}" = actual/maximo

        private float targetFill = 1f;
        private bool initialized;
        private int lastShownCurrent = -1;
        private int lastShownMax = -1;

        private void OnEnable()  => GameEvents.OnThreadChanged += HandleThreadChanged;
        private void OnDisable() => GameEvents.OnThreadChanged -= HandleThreadChanged;

        private void HandleThreadChanged(float current, float max)
        {
            targetFill = max > 0f ? Mathf.Clamp01(current / max) : 0f;

            // El primer valor se aplica directo para que el relleno no "baje desde lleno" al empezar.
            if (!initialized && fillImage != null)
            {
                fillImage.fillAmount = targetFill;
                initialized = true;
            }

            UpdateText(current, max);
        }

        private void Update()
        {
            if (fillImage == null) return;

            // unscaledDeltaTime: el relleno sigue animandose aunque el juego este en pausa.
            fillImage.fillAmount = Mathf.MoveTowards(
                fillImage.fillAmount, targetFill, smoothSpeed * Time.unscaledDeltaTime);
        }

        private void UpdateText(float current, float max)
        {
            if (amountText == null) return;

            int shownCurrent = Mathf.CeilToInt(current);
            int shownMax = Mathf.RoundToInt(max);

            // El texto solo se reconstruye si el numero cambio (evita crear basura cada frame).
            if (shownCurrent == lastShownCurrent && shownMax == lastShownMax) return;

            lastShownCurrent = shownCurrent;
            lastShownMax = shownMax;
            amountText.text = string.Format(format, shownCurrent, shownMax);
        }
    }
}
