using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Base comun de las habilidades. Implementa IAbility y da acceso al Rigidbody2D y al
    /// GroundSensor. Cada habilidad hija (Jump, Glide, Crouch, Swing, Hide) solo agrega lo suyo.
    /// Herencia usada solo para compartir lo comun; el jugador en si se arma por composicion
    /// (varios componentes en el mismo GameObject).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(GroundSensor))]
    public abstract class AbilityBase : MonoBehaviour, IAbility
    {
        protected Rigidbody2D Body { get; private set; }
        protected GroundSensor Ground { get; private set; }

        public bool IsActive { get; protected set; }

        protected virtual void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            Ground = GetComponent<GroundSensor>();
        }

        /// <summary>Update: leer input y decidir si se activa.</summary>
        public abstract void HandleInput(IInputProvider input);

        /// <summary>FixedUpdate: aplicar fisicas. Opcional, por eso es virtual vacio.</summary>
        public virtual void FixedTick() { }

        /// <summary>Corta la habilidad de inmediato.</summary>
        public virtual void Cancel()
        {
            IsActive = false;
        }
    }
}
