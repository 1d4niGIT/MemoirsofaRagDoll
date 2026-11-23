using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Hazards
{
    /// <summary>
    /// Superficie elastica (cama, trampolin). Cada vez que algo que pueda rebotar (IBounceable)
    /// cae SOBRE ella, lo lanza hacia arriba. Mientras siga en contacto vuelve a rebotar,
    /// y deja de hacerlo cuando sale de la superficie.
    ///
    /// Requiere un Collider2D SOLIDO (no trigger) en el mismo objeto.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BouncySurface : MonoBehaviour
    {
        [SerializeField] private float bounceSpeed = 12f;     // velocidad hacia arriba del rebote
        [SerializeField] private float minInterval = 0.1f;    // evita rebotar dos veces seguidas en el mismo choque
        [SerializeField] private float topTolerance = 0.15f;  // margen para decidir que cayo "desde arriba"

        private Collider2D surface;
        private float nextBounceTime;

        private void Awake() => surface = GetComponent<Collider2D>();

        private void OnCollisionEnter2D(Collision2D collision) => TryBounce(collision);
        private void OnCollisionStay2D(Collision2D collision) => TryBounce(collision);

        private void TryBounce(Collision2D collision)
        {
            if (Time.time < nextBounceTime) return;

            // Solo rebota si el objeto esta encima: el pie por encima del borde superior de la cama.
            Bounds other = collision.collider.bounds;
            if (other.min.y < surface.bounds.max.y - topTolerance) return;

            IBounceable bounceable = collision.collider.GetComponentInParent<IBounceable>();
            if (bounceable == null) return;

            nextBounceTime = Time.time + minInterval;
            bounceable.Bounce(bounceSpeed);
        }
    }
}
