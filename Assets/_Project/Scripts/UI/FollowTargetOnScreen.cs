using UnityEngine;

namespace RagDoll.UI
{
    /// <summary>
    /// Mantiene un elemento de UI de un Canvas en "Screen Space - Overlay" encima de un objetivo del
    /// mundo (por ejemplo, la barra de fuerza sobre la cabeza de Mino). Convierte la posicion de la
    /// cabeza a pixeles de pantalla cada frame.
    ///
    /// Va en el propio elemento de UI (BarForce), que debe ser hijo directo del Canvas
    /// (no dentro de un panel con Layout Group, que le cambiaria la posicion).
    /// El tamano de la barra no cambia con el zoom de la camara: se mantiene legible.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class FollowTargetOnScreen : MonoBehaviour
    {
        [SerializeField] private Collider2D target;            // el collider de Mino
        [SerializeField] private float worldMarginAbove = 0.3f; // espacio sobre la cabeza, en unidades del mundo
        [SerializeField] private Vector2 pixelOffset;           // ajuste fino en pixeles de pantalla

        private RectTransform rectTransform;
        private Camera cam;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            cam = Camera.main;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            Bounds bounds = target.bounds;
            Vector3 headWorld = new Vector3(bounds.center.x, bounds.max.y + worldMarginAbove, 0f);

            Vector3 screen = cam.WorldToScreenPoint(headWorld);
            if (screen.z < 0f) return; // detras de la camara

            rectTransform.position = new Vector3(screen.x + pixelOffset.x, screen.y + pixelOffset.y, 0f);
        }
    }
}
