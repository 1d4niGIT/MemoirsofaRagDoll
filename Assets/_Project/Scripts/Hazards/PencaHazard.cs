using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Hazards
{
    /// <summary>
    /// Penca: amenaza estatica. Mientras Mino la toca le baja vida cada "hitInterval" segundos
    /// y lo empuja en direccion contraria a donde la toco (desde la penca hacia Mino).
    /// El dano lo sigue poniendo HazardBase; esta clase solo agrega el ritmo y el empujon.
    ///
    /// La penca es SOLIDA: Mino no la traspasa. Para eso lleva DOS colliders en el mismo objeto:
    ///   1) uno solido (Is Trigger apagado): la barrera fisica.
    ///   2) uno trigger, un poco mas grande (~0.05 a 0.1 por lado): detecta el toque y hace el dano.
    /// El trigger debe ser mas grande porque Mino se detiene en el borde del solido y, si fueran
    /// iguales, nunca estaria "dentro" del trigger.
    /// </summary>
    public class PencaHazard : HazardBase
    {
        [Header("Golpe repetido")]
        [SerializeField] private float hitInterval = 1f;       // segundos entre golpes mientras se la toca

        [Header("Empujon")]
        [SerializeField] private float knockbackSpeed = 7f;
        [SerializeField] private float knockbackDuration = 0.2f;
        [SerializeField, Range(0f, 1f)] private float upwardBias = 0.4f; // algo de impulso hacia arriba

        private float nextHitTime;
        private Collider2D damageTrigger;

        private void Awake()
        {
            // El collider que detecta el dano es el trigger; el otro es la barrera solida.
            foreach (Collider2D c in GetComponents<Collider2D>())
            {
                if (c.isTrigger) { damageTrigger = c; break; }
            }

            if (damageTrigger == null)
                Debug.LogWarning("PencaHazard necesita un Collider2D con Is Trigger (dano) ademas del solido.", this);
        }

        protected override bool CanHit() => Time.time >= nextHitTime;

        protected override void OnHit(IDamageable target, Collider2D other)
        {
            nextHitTime = Time.time + hitInterval;
            base.OnHit(target, other);   // dano normal

            IKnockbackable pushable = other.GetComponentInParent<IKnockbackable>();
            if (pushable == null) return;

            pushable.ApplyKnockback(PushDirection(other) * knockbackSpeed, knockbackDuration);
        }

        /// <summary>Desde el punto de la penca mas cercano a Mino, hacia Mino.</summary>
        private Vector2 PushDirection(Collider2D other)
        {
            Vector2 playerCenter = other.bounds.center;
            Collider2D reference = damageTrigger != null ? damageTrigger : GetComponent<Collider2D>();
            Vector2 contact = reference.ClosestPoint(playerCenter);
            Vector2 dir = playerCenter - contact;

            // Si el centro de Mino esta dentro de la penca, ClosestPoint devuelve el mismo punto.
            if (dir.sqrMagnitude < 0.0001f)
                dir = playerCenter - (Vector2)transform.position;
            if (dir.sqrMagnitude < 0.0001f)
                dir = Vector2.up;

            dir.y += upwardBias;
            return dir.normalized;
        }
    }
}