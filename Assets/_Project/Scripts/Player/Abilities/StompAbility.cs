using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Caida en picada. Estando en el aire, al PRESIONAR agacharse Mino baja de golpe a gran velocidad
    /// hasta tocar suelo. Esa velocidad es el "golpe" con el que se activan cosas como la lampara.
    ///
    /// Es una habilidad mas (IAbility, herencia de AbilityBase): PlayerController la usa sin saber que existe.
    /// Solo se activa con una pulsacion NUEVA (no si ya venias manteniendo agacharse al saltar)
    /// y no funciona mientras Mino cuelga del hilo.
    ///
    /// Ponla DESPUES de GlideAbility en el Inspector: asi, si estabas planeando, la picada gana.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class StompAbility : AbilityBase
    {
        [SerializeField] private float stompSpeed = 22f;   // velocidad de caida durante la picada

        private PlayerController controller;
        private bool previousCrouchHeld;

        protected override void Awake()
        {
            base.Awake();
            controller = GetComponent<PlayerController>();
        }

        public override void HandleInput(IInputProvider input)
        {
            bool pressedNow = input.CrouchHeld && !previousCrouchHeld;
            previousCrouchHeld = input.CrouchHeld;

            if (IsActive)
            {
                // Termina al tocar suelo o si el balanceo toma el control.
                if (Ground.IsGrounded || controller.ExternalMovement)
                    IsActive = false;
                return;
            }

            if (pressedNow && !Ground.IsGrounded && !controller.ExternalMovement)
                IsActive = true;
        }

        public override void FixedTick()
        {
            if (!IsActive) return;

            if (controller.ExternalMovement)
            {
                IsActive = false;
                return;
            }

            Vector2 velocity = Body.linearVelocity;
            if (velocity.y > -stompSpeed)
                velocity.y = -stompSpeed;   // corta la subida y acelera la bajada
            Body.linearVelocity = velocity;
        }
    }
}
