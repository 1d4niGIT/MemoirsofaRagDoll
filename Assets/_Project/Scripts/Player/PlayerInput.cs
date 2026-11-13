using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Implementacion de IInputProvider e IAimInput con el asset de acciones (Input Actions).
    /// Las teclas NO estan escritas aqui: se configuran en el asset InputSystem_Actions, y este
    /// script solo traduce las acciones del mapa "Player" a las interfaces del juego.
    /// Por eso PlayerController, las habilidades y SwingAbility no cambian si cambias teclas o mando
    /// (D de SOLID).
    ///
    /// Acciones que usa (mapa "Player"): Move, Jump, Crouch, Interact, Attack y Point.
    /// </summary>
    public class PlayerInput : MonoBehaviour, IInputProvider, IAimInput
    {
        private InputSystem_Actions actions; // clase generada desde el asset de acciones

        private void Awake()
        {
            actions = new InputSystem_Actions();
        }

        private void OnEnable() => actions.Player.Enable();
        private void OnDisable() => actions.Player.Disable();
        private void OnDestroy() => actions.Dispose();

        // ---------- IInputProvider ----------
        public float Horizontal => actions.Player.Move.ReadValue<Vector2>().x;

        public float Vertical => actions.Player.Move.ReadValue<Vector2>().y; // W = +1, S = -1

        public bool JumpPressed => actions.Player.Jump.WasPressedThisFrame();

        public bool JumpHeld => actions.Player.Jump.IsPressed();

        public bool CrouchHeld => actions.Player.Crouch.IsPressed();

        public bool InteractPressed => actions.Player.Interact.WasPressedThisFrame();

        // ---------- IAimInput ----------
        public bool AimPressed => actions.Player.Attack.WasPressedThisFrame();

        public bool AimHeld => actions.Player.Attack.IsPressed();

        public bool AimReleased => actions.Player.Attack.WasReleasedThisFrame();

        public Vector2 PointerScreenPosition => actions.Player.Point.ReadValue<Vector2>();
    }
}