using System.Collections.Generic;
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
    ///
    /// ATRAPADO (opcional, apagado por defecto: se activa solo en las pencas por las que se pasa agachado
    /// con "Hold When Inside"): si alguien queda DENTRO del collider solido (ej. Mino se levanta debajo de un cactus),
    /// no se le empuja ni se le expulsa: se queda quieto en ese lugar, recibiendo el dano cada
    /// "hitInterval", hasta que deje de estar superpuesto (ej. al agacharse otra vez).
    /// Se usa solo IKnockbackable (ApplyKnockback con velocidad cero), sin conocer al jugador.
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

        [Header("Atrapado dentro de la barrera solida")]
        [SerializeField] private bool holdWhenInside = false;      // SOLO para pencas por las que se pasa agachado: dejarlo quieto en vez de expulsarlo
        [SerializeField] private float insidePenetration = 0.1f;   // cuanto debe hundirse para contar como "dentro"

        private float nextHitTime;
        private Collider2D damageTrigger;
        private Collider2D solidCollider;

        // Quienes estan atrapados ahora: su IKnockbackable y sus colliders no-trigger.
        private readonly Dictionary<IKnockbackable, Collider2D[]> trapped = new Dictionary<IKnockbackable, Collider2D[]>();
        private readonly List<Collider2D> overlaps = new List<Collider2D>();
        private readonly List<IKnockbackable> toRelease = new List<IKnockbackable>();
        private ContactFilter2D noFilter;

        protected virtual void Awake()
        {
            foreach (Collider2D c in GetComponents<Collider2D>())
            {
                if (c.isTrigger) { if (damageTrigger == null) damageTrigger = c; }
                else if (solidCollider == null) solidCollider = c;
            }

            noFilter = ContactFilter2D.noFilter;   // propiedad estatica (NoFilter() quedo obsoleto en Unity 6)

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
            if (pushable == null) return;

            // Si esta atrapado dentro de la barrera, se queda quieto: solo recibe el dano,
            // pero se le pide una sacudida visual para que se note el golpe.
            if (trapped.ContainsKey(pushable))
            {
                IHitReactable reactable = other.attachedRigidbody != null
                    ? other.attachedRigidbody.GetComponentInChildren<IHitReactable>()
                    : other.GetComponentInChildren<IHitReactable>();
                reactable?.ReactToHit();
                return;
            }

            pushable.ApplyKnockback(PushDirection(other) * knockbackSpeed, knockbackDuration);
        }

        private void FixedUpdate()
        {
            if (!holdWhenInside || solidCollider == null) return;

            FindNewlyTrapped();
            HoldTrapped();
        }

        /// <summary>Busca quien se hundio dentro del collider solido (mas que insidePenetration).</summary>
        private void FindNewlyTrapped()
        {
            overlaps.Clear();
            solidCollider.Overlap(noFilter, overlaps);

            foreach (Collider2D c in overlaps)
            {
                if (c == null || c.isTrigger) continue;

                IKnockbackable pushable = c.GetComponentInParent<IKnockbackable>();
                if (pushable == null || trapped.ContainsKey(pushable)) continue;

                ColliderDistance2D d = c.Distance(solidCollider);
                if (!d.isOverlapped || d.distance > -insidePenetration) continue;

                Collider2D[] targets = c.attachedRigidbody != null
                    ? c.attachedRigidbody.GetComponentsInChildren<Collider2D>()
                    : new[] { c };

                // Se ignora la barrera para el: asi la fisica no lo expulsa de golpe.
                foreach (Collider2D t in targets)
                    if (t != null && !t.isTrigger) Physics2D.IgnoreCollision(t, solidCollider, true);

                trapped.Add(pushable, targets);
            }
        }

        /// <summary>Mientras siga hundido, lo mantiene quieto; cuando sale, la barrera vuelve.</summary>
        private void HoldTrapped()
        {
            if (trapped.Count == 0) return;

            toRelease.Clear();

            foreach (KeyValuePair<IKnockbackable, Collider2D[]> pair in trapped)
            {
                bool stillInside = false;
                foreach (Collider2D t in pair.Value)
                {
                    if (t == null || t.isTrigger) continue;
                    ColliderDistance2D d = t.Distance(solidCollider);
                    if (d.isOverlapped && d.distance <= -insidePenetration) { stillInside = true; break; }
                }

                if (stillInside)
                    pair.Key.ApplyKnockback(Vector2.zero, Time.fixedDeltaTime * 3f);   // quieto; se refresca cada paso
                else
                    toRelease.Add(pair.Key);
            }

            foreach (IKnockbackable key in toRelease)
            {
                foreach (Collider2D t in trapped[key])
                    if (t != null && !t.isTrigger) Physics2D.IgnoreCollision(t, solidCollider, false);
                trapped.Remove(key);
            }
        }

        private void OnDisable()
        {
            // Si la amenaza se apaga, devuelve la barrera a quienes estaban atrapados.
            foreach (KeyValuePair<IKnockbackable, Collider2D[]> pair in trapped)
                foreach (Collider2D t in pair.Value)
                    if (t != null && solidCollider != null && !t.isTrigger)
                        Physics2D.IgnoreCollision(t, solidCollider, false);
            trapped.Clear();
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