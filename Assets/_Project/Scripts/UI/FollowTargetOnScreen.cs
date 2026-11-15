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
    ///
    /// ANTI-TEMBLOR:
    /// 1) [DefaultExecutionOrder(100)]: corre DESPUES de CameraFollow2D (orden 0). Si corriera antes,
    ///    usaria la posicion de la camara del frame anterior y la barra vibraria.
    /// 2) La cabeza se calcula con el TRANSFORM (que Unity interpola) y no con collider.bounds
    ///    (que solo cambia en cada paso de fisica, 50 veces por segundo, y se ve a saltos).
    /// 3) La posicion se redondea a pixeles enteros para que no "brille" con subpixeles.
    /// </summary>
    [DefaultExecutionOrder(100)]
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

            Vector3 headWorld = HeadWorldPosition();

            Vector3 screen = cam.WorldToScreenPoint(headWorld);
            if (screen.z < 0f) return; // detras de la camara

            rectTransform.position = new Vector3(
                Mathf.Round(screen.x + pixelOffset.x),
                Mathf.Round(screen.y + pixelOffset.y),
                0f);
        }

        /// <summary>
        /// Punto sobre la cabeza usando el transform interpolado. Para capsula y caja se usa su tamano
        /// local (respeta escala y agacharse); para otros colliders se cae a bounds.
        /// </summary>
        private Vector3 HeadWorldPosition()
        {
            Transform t = target.transform;
            Vector2 localTop;

            if (target is CapsuleCollider2D capsule)
                localTop = capsule.offset + Vector2.up * (capsule.size.y * 0.5f);
            else if (target is BoxCollider2D box)
                localTop = box.offset + Vector2.up * (box.size.y * 0.5f);
            else
            {
                Bounds b = target.bounds;
                return new Vector3(b.center.x, b.max.y + worldMarginAbove, 0f);
            }

            Vector3 top = t.TransformPoint(localTop);
            return new Vector3(top.x, top.y + worldMarginAbove, 0f);
        }
    }
}
