using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Hazards
{
    /// <summary>
    /// Base de todas las amenazas (tijeras, pinzas, cactus, pencas, guillotina).
    /// Detecta el contacto y le aplica dano a cualquier IDamageable, sin saber si es el jugador.
    ///
    /// Las hijas personalizan por "ganchos" (CanHit / OnHit) sin modificar esta clase:
    /// abierta a extension, cerrada a modificacion (O de SOLID). Toda hija debe seguir
    /// danando de verdad cuando CanHit() es true (L de SOLID).
    ///
    /// Requisitos en Unity: el Collider2D de la amenaza en "Is Trigger", y que el jugador
    /// tenga un Rigidbody2D (Mino ya lo tiene) para que se disparen los eventos de trigger.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class HazardBase : MonoBehaviour
    {
        [SerializeField] protected int damage = 1;

        /// <summary>Gancho: la amenaza puede estar "apagada" (ej. tijeras abiertas/cerradas).</summary>
        protected virtual bool CanHit() => true;

        /// <summary>Gancho: que pasa al golpear. Por defecto aplica el dano.</summary>
        protected virtual void OnHit(IDamageable target)
        {
            target.TakeDamage(damage);
        }

        // Se ejecuta al agregar el componente en el editor: deja el collider como trigger.
        private void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other) => TryHit(other);

        // Si el jugador se queda dentro, vuelve a recibir dano cuando termine su invulnerabilidad.
        private void OnTriggerStay2D(Collider2D other) => TryHit(other);

        private void TryHit(Collider2D other)
        {
            if (!CanHit()) return;

            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target == null || !target.IsAlive) return;

            OnHit(target);
        }
    }
}
