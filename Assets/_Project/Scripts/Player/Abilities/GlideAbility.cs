using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Planear. Mientras Mino esta en el aire, cayendo y manteniendo el boton de salto,
    /// su velocidad de caida se limita a un valor bajo (como si la tela lo frenara).
    /// Al soltar el boton o tocar el suelo, termina.
    ///
    /// Como es una habilidad mas (IAbility), PlayerController la usa sin saber que existe.
    /// </summary>
    public class GlideAbility : AbilityBase
    {
        [SerializeField] private float maxFallSpeed = 2f;       // caida maxima mientras planea
        [SerializeField] private float minFallToGlide = 0.1f;   // evita activarla durante la subida

        public override void HandleInput(IInputProvider input)
        {
            bool isFalling = Body.linearVelocity.y < -minFallToGlide;
            bool canGlide = !Ground.IsGrounded && isFalling;

            IsActive = input.JumpHeld && canGlide;
        }

        public override void FixedTick()
        {
            if (!IsActive) return;

            Vector2 velocity = Body.linearVelocity;
            if (velocity.y < -maxFallSpeed)
                velocity.y = -maxFallSpeed;

            Body.linearVelocity = velocity;
        }
    }
}
