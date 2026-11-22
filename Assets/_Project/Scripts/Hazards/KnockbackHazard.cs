using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Hazards
{
    /// <summary>
    /// Amenaza solida que, al tocarla, hace dano cada "hitInterval" segundos y empuja a quien la toca
    /// en direccion contraria al contacto. Base compartida por la penca, la aguja y lo que venga
    /// (cactus, pinzas...): el empujon se escribe una sola vez.
    ///
    /// Las hijas agregan efectos extra sobrescribiendo OnHitExtra.
    ///
    /// Collider: uno SOLIDO (barrera) y uno TRIGGER un poco mas grande (deteccion del dano),
    /// los dos en el mismo objeto.
    /// </summary>
    public abstract class KnockbackHazard : HazardBase
    {
        [Header("Golpe repetido")]
        [SerializeField] private float hitInterval = 1f;

        [Header("Hilo que se suelta (VFX)")]
        [SerializeField] private int snagLines = 2;   // lineas de hilo que salen de Mino al golpearlo

        [Header("Empujon")]
        [SerializeField] private float knockbackSpeed = 7f;
        [SerializeField] private float knockbackDuration = 0.2f;
        [SerializeField, Range(0f, 1f)] private float upwardBias = 0.4f;

        private float nextHitTime;
        private Collider2D damageTrigger;

        protected virtual void Awake()
        {
            foreach (Collider2D c in GetComponents<Collider2D>())
            {
                if (c.isTrigger) { damageTrigger = c; break; }
            }

            if (damageTrigger == null)
                Debug.LogWarning(name + " necesita un Collider2D con Is Trigger (dano) ademas del solido.", this);
        }

        protected override bool CanHit() => Time.time >= nextHitTime;

        protected override void OnHit(IDamageable target, Collider2D other)
        {
            nextHitTime = Time.time + hitInterval;
            base.OnHit(target, other);      // dano normal
            GameEvents.RaiseSnagged(other, ContactPoint(other), snagLines);   // aviso para el VFX
            OnHitExtra(other);              // efectos propios de la hija

            IKnockbackable pushable = other.GetComponentInParent<IKnockbackable>();
            if (pushable != null)
                pushable.ApplyKnockback(PushDirection(other) * knockbackSpeed, knockbackDuration);
        }

        /// <summary>Gancho: efecto extra al golpear (ej. gastar hilo). Por defecto nada.</summary>
        protected virtual void OnHitExtra(Collider2D other) { }

        /// <summary>Punto de esta amenaza mas cercano al objetivo: donde "engancho".</summary>
        private Vector2 ContactPoint(Collider2D other)
        {
            Collider2D reference = damageTrigger != null ? damageTrigger : GetComponent<Collider2D>();
            return reference.ClosestPoint(other.bounds.center);
        }

        /// <summary>Desde el punto de la amenaza mas cercano al objetivo, hacia el objetivo.</summary>
        private Vector2 PushDirection(Collider2D other)
        {
            Vector2 center = other.bounds.center;
            Collider2D reference = damageTrigger != null ? damageTrigger : GetComponent<Collider2D>();
            Vector2 dir = center - reference.ClosestPoint(center);

            if (dir.sqrMagnitude < 0.0001f) dir = center - (Vector2)transform.position;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;

            dir.y += upwardBias;
            return dir.normalized;
        }
    }
}