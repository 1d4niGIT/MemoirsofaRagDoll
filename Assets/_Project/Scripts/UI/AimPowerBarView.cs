using UnityEngine;
using UnityEngine.UI;
using RagDoll.Core;

namespace RagDoll.UI
{
    /// <summary>
    /// Medidor de fuerza del lanzamiento. Solo ESCUCHA el evento OnAimChanged: se muestra
    /// mientras Mino apunta y se oculta al soltar. No conoce a SwingAbility.
    ///
    /// IMPORTANTE: este script debe ir en un objeto que SIEMPRE este activo (por ejemplo el Canvas).
    /// La barra que se muestra y oculta es el objeto que arrastras al campo "Root". Si el script
    /// estuviera en el propio Root, al ocultarlo dejaria de escuchar y no volveria a aparecer.
    /// Configuracion de la imagen: Image Type = Filled, Fill Method = Horizontal.
    /// </summary>
    public class AimPowerBarView : MonoBehaviour
    {
        [SerializeField] private GameObject root;     // la barra completa (fondo + relleno)
        [SerializeField] private Image fillImage;     // el relleno

        private void Awake()
        {
            if (root != null) root.SetActive(false);
        }

        private void OnEnable()  => GameEvents.OnAimChanged += HandleAimChanged;
        private void OnDisable() => GameEvents.OnAimChanged -= HandleAimChanged;

        private void HandleAimChanged(bool isAiming, float power01)
        {
            if (root != null) root.SetActive(isAiming);

            if (isAiming && fillImage != null)
                fillImage.fillAmount = power01;
        }
    }
}
