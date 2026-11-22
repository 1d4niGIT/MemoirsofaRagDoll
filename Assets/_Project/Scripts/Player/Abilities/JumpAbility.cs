using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Salto. Lee el input en Update (HandleInput) y aplica la fisica en FixedUpdate (FixedTick).
    /// Incluye "coyote time": un margen corto para saltar justo despues de dejar una plataforma,
    /// que hace el control mucho mas amable.
    ///
    /// Salto variable (como Hollow Knight): el salto sale con la fuerza completa, pero si sueltas
    /// el boton mientras Mino todavia sube, la subida se corta. Un toque rapido da un saltito y
    /// mantener el boton da la altura normal; soltar a mitad da una altura intermedia.
    /// </summary>
    public class JumpAbility : AbilityBase
    {
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float coyoteTime = 0.1f;

        [Header("Salto variable")]
        [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.35f; // 0 = corta en seco, 1 = sin corte
        [SerializeField] private float minJumpTime = 0.06f;                      // tiempo minimo antes de poder cortar

        private bool jumpRequested;
        private float lastGroundedTime = -999f;

        private bool jumpHeld;
        private bool jumpCutDone;
        private float jumpStartTime;
        private PlayerController controller;

        protected override void Awake()
        {
            base.Awake();
            controller = GetComponent<PlayerController>();
        }

        public override void HandleInput(IInputProvider input)
        {
            jumpHeld = input.JumpHeld;

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
            if (jumpRequested)
            {
                StartJump();
                return;
            }

            TryCutJump();
        }

        private void StartJump()
        {
            jumpRequested = false;
            lastGroundedTime = -999f; // evita un segundo salto aprovechando el coyote time

            // Unity 6: Rigidbody2D.velocity pasa a llamarse linearVelocity.
            Vector2 velocity = Body.linearVelocity;
            velocity.y = jumpForce;
            Body.linearVelocity = velocity;

            jumpStartTime = Time.time;
            jumpCutDone = false;
            IsActive = true;
        }

        /// <summary>
        /// Si el boton ya se solto y Mino sigue subiendo, frena la subida una sola vez.
        /// Cuanto mas tarde se suelte, mas alto habra llegado antes del corte.
        /// </summary>
        private void TryCutJump()
        {
            if (!IsActive || jumpCutDone || jumpHeld) return;
            if (Time.time - jumpStartTime < minJumpTime) return;

            jumpCutDone = true;

            // Si el balanceo con hilo tomo el control, no se toca su velocidad.
            if (controller != null && controller.ExternalMovement) return;

            Vector2 velocity = Body.linearVelocity;
            if (velocity.y > 0f)
            {
                velocity.y *= jumpCutMultiplier;
                Body.linearVelocity = velocity;
            }
        }

        public override void Cancel()
        {
            base.Cancel();
            jumpRequested = false;
        }
    }
}