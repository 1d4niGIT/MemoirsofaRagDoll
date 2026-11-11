using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Salto. Lee el input en Update (HandleInput) y aplica la fisica en FixedUpdate (FixedTick).
    /// Incluye "coyote time": un margen corto para saltar justo despues de dejar una plataforma,
    /// que hace el control mucho mas amable.
    /// </summary>
    public class JumpAbility : AbilityBase
    {
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float coyoteTime = 0.1f;

        private bool jumpRequested;
        private float lastGroundedTime = -999f;

        public override void HandleInput(IInputProvider input)
        {
            if (Ground.IsGrounded)
            {
                lastGroundedTime = Time.time;

                // Ya aterrizamos: el salto termino.
                if (IsActive && Body.linearVelocity.y <= 0.01f)
                    IsActive = false;
            }

            bool canJump = Time.time - lastGroundedTime <= coyoteTime;

            if (input.JumpPressed && canJump)
                jumpRequested = true;
        }

        public override void FixedTick()
        {
            if (!jumpRequested) return;

            jumpRequested = false;
            lastGroundedTime = -999f; // evita un segundo salto aprovechando el coyote time

            // Unity 6: Rigidbody2D.velocity pasa a llamarse linearVelocity.
            Vector2 velocity = Body.linearVelocity;
            velocity.y = jumpForce;
            Body.linearVelocity = velocity;

            IsActive = true;
        }

        public override void Cancel()
        {
            base.Cancel();
            jumpRequested = false;
        }
    }
}
