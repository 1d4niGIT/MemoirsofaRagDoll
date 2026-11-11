using UnityEngine;
using RagDoll.Core;
using Keyboard = UnityEngine.InputSystem.Keyboard;

namespace RagDoll.Player
{
    /// <summary>
    /// Implementacion de IInputProvider con el NUEVO Input System (teclado).
    /// Es el unico script que cambia respecto a la version con Input clasico:
    /// PlayerController y las habilidades siguen igual porque solo conocen IInputProvider (D de SOLID).
    ///
    /// Se usa un alias para Keyboard y asi no chocar con el componente PlayerInput
    /// que trae el paquete Input System.
    /// </summary>
    public class PlayerInput : MonoBehaviour, IInputProvider
    {
        private static Keyboard Kb => Keyboard.current; // puede ser null si no hay teclado

        public float Horizontal
        {
            get
            {
                Keyboard kb = Kb;
                if (kb == null) return 0f;

                float x = 0f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
                return x;
            }
        }

        public bool JumpPressed =>
            Kb != null && (Kb.spaceKey.wasPressedThisFrame || Kb.wKey.wasPressedThisFrame);

        public bool JumpHeld =>
            Kb != null && (Kb.spaceKey.isPressed || Kb.wKey.isPressed);

        public bool CrouchHeld =>
            Kb != null && (Kb.sKey.isPressed || Kb.leftCtrlKey.isPressed);

        public bool InteractPressed =>
            Kb != null && Kb.eKey.wasPressedThisFrame;
    }
}
