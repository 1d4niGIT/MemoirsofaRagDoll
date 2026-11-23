using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Hazards
{
    /// <summary>
    /// Aguja mortal: al tocarla, mata de golpe. Ignora el hilo, el relleno y la invulnerabilidad
    /// (usa IKillable.Kill, igual que RainProjectile). Es la version ESTATICA de la aguja que mata:
    /// un lecho de agujas, una aguja clavada en la pared, etc.
    ///
    /// No sabe quien es el objetivo: mata a cualquier cosa que implemente IKillable (D de SOLID).
    /// Hereda de HazardBase, asi que solo cambia QUE pasa al golpear (O de SOLID).
    ///
    /// Collider: un Collider2D con "Is Trigger" encendido que cubra las puntas peligrosas.
    /// </summary>
    public class DeadlyNeedleHazard : HazardBase
    {
        protected override void OnHit(IDamageable target, Collider2D other)
        {
            IKillable killable = other.GetComponentInParent<IKillable>();

            if (killable != null)
                killable.Kill();
            else
                base.OnHit(target, other);   // si no se puede matar, al menos hace dano
        }
    }
}
