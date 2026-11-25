using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Hazards
{
    /// <summary>
    /// Superficie elastica (cama, trampolin). Cuando algo que pueda rebotar (IBounceable) cae SOBRE ella,
    /// lo lanza hacia arriba con una fraccion de la velocidad con la que llego ("restitution").
    /// Como cada rebote devuelve menos que el anterior, la altura baja sola hasta que el rebote
    /// seria menor que "minBounceSpeed": ahi ya no rebota y Mino se queda de pie sobre la cama.
    /// Si salta otra vez, vuelve a tomar impulso y rebota de nuevo.
    ///
    /// Una caida en picada (stomp) llega mas rapido, asi que rebota mas alto (hasta maxBounceSpeed).
    ///
    /// Requiere un Collider2D SOLIDO (no trigger) en el mismo objeto.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BouncySurface : MonoBehaviour
    {
        [Header("Elasticidad")]
        [SerializeField, Range(0f, 1f)] private float restitution = 0.75f;  // 1 = infinito, 0.5 = se apaga rapido
        [SerializeField] private float minBounceSpeed = 4f;    // por debajo de esta velocidad ya no rebota
        [SerializeField] private float maxBounceSpeed = 14f;   // tope (ej. para la caida en picada)

        [Header("Deteccion")]
        [SerializeField] private float minInterval = 0.1f;     // evita rebotar dos veces en el mismo choque
        [SerializeField] private float topTolerance = 0.3f;    // margen para decidir que cayo "desde arriba"

        private Collider2D surface;
        private float nextBounceTime;

        private void Awake() => surface = GetComponent<Collider2D>();

        // Solo al CHOCAR: el rebote depende de la velocidad del impacto, no de seguir en contacto.
        private void OnCollisionEnter2D(Collision2D collision) => TryBounce(collision);

        private void TryBounce(Collision2D collision)
        {
            if (Time.time < nextBounceTime) return;

            // Solo rebota si el objeto esta encima: el pie por encima del borde superior de la cama.
            Bounds other = collision.collider.bounds;
            if (other.min.y < surface.bounds.max.y - topTolerance) return;

            IBounceable bounceable = collision.collider.GetComponentInParent<IBounceable>();
            if (bounceable == null) return;

            // Velocidad vertical con la que llego (antes de que la fisica la frene).
            float impactSpeed = Mathf.Abs(collision.relativeVelocity.y);
            float outSpeed = Mathf.Min(impactSpeed * restitution, maxBounceSpeed);

            // Ya casi no tiene energia: no rebota y se queda de pie.
            if (outSpeed < minBounceSpeed) return;

            nextBounceTime = Time.time + minInterval;
            bounceable.Bounce(outSpeed);
        }
    }
}