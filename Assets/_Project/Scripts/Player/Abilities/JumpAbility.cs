using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Salto. Lee el input en Update (HandleInput) y aplica la fisica en FixedUpdate (FixedTick).
    ///
    /// Mejoras de sensacion:
    /// - Coyote time: margen corto para saltar justo despues de dejar una plataforma.
    /// - Jump buffer: si aprietas saltar un poquito ANTES de aterrizar, el salto sale al tocar suelo.
    /// - Salto variable suave: al soltar el boton mientras sube, en vez de frenar de golpe la
    ///   velocidad (se siente tosco) se aplica gravedad extra, asi la subida se curva y se frena natural.
    /// - Salto minimo: un toque rapido siempre da un saltito con altura util, no un brinco ridiculo.
    /// </summary>
    public class JumpAbility : AbilityBase
    {
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBufferTime = 0.12f;       // margen para apretar antes de aterrizar

        [Header("Salto variable")]
        [SerializeField] private float minJumpTime = 0.1f;           // tiempo que sube SI o SI antes de poder frenar
        [SerializeField] private float releaseGravityMultiplier = 2.5f; // gravedad extra al soltar mientras sube (1 = sin efecto)

        private bool jumpRequested;
        private float lastGroundedTime = -999f;
        private float lastJumpPressedTime = -999f;

        private bool jumpHeld;
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

            if (input.JumpPressed)
                lastJumpPressedTime = Time.time;

            if (Ground.IsGrounded)
            {
                lastGroundedTime = Time.time;

                // Ya aterrizamos: el salto termino.
                if (IsActive && Body.linearVelocity.y <= 0.01f)
                    IsActive = false;
            }

            bool canJump = Time.time - lastGroundedTime <= coyoteTime;
            bool wantsJump = Time.time - lastJumpPressedTime <= jumpBufferTime;

            if (wantsJump && canJump)
                jumpRequested = true;
        }

        public override void FixedTick()
        {
            if (jumpRequested)
            {
                StartJump();
                return;
            }

            ApplyVariableJump();
        }

        private void StartJump()
        {
            jumpRequested = false;
            lastGroundedTime = -999f;     // evita un segundo salto aprovechando el coyote time
            lastJumpPressedTime = -999f;  // consume el buffer

            // Unity 6: Rigidbody2D.velocity pasa a llamarse linearVelocity.
            Vector2 velocity = Body.linearVelocity;
            velocity.y = jumpForce;
            Body.linearVelocity = velocity;

            jumpStartTime = Time.time;
            IsActive = true;
        }

        /// <summary>
        /// Si soltaste el boton y Mino aun sube, suma gravedad extra: la subida se frena de forma
        /// gradual en vez de cortarse en seco. Cuanto mas tarde sueltas, mas alto llega.
        /// </summary>
        private void ApplyVariableJump()
        {
            if (!IsActive || jumpHeld) return;
            if (Time.time - jumpStartTime < minJumpTime) return;

            // Si el balanceo con hilo tomo el control, no se toca su velocidad.
            if (controller != null && controller.ExternalMovement) return;

            Vector2 velocity = Body.linearVelocity;
            if (velocity.y <= 0f) return;

            // Gravedad extra = (multiplicador - 1) veces la gravedad normal de Mino.
            float extra = Physics2D.gravity.y * Body.gravityScale * (releaseGravityMultiplier - 1f) * Time.fixedDeltaTime;
            velocity.y += extra; // extra es negativo
            Body.linearVelocity = velocity;
        }

        public override void Cancel()
        {
            base.Cancel();
            jumpRequested = false;
        }
    }
}