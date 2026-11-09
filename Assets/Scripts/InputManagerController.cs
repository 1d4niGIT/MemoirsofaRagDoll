using System;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputManagerController
{
    private static InputSystem_Actions inputs;

    public static Action<Vector2> OnMove;
    public static Action OnJump;
    public static Action OnJumpReleased;      // salto variable
    public static Action<bool> OnCrouch;      // true = presionado, false = soltado
    public static Action OnInteract;          // esconderse / agarrarse a un punto de balanceo

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        inputs = new InputSystem_Actions();
        inputs.Enable();

        inputs.Player.Move.performed += OnMovementPerformed;
        inputs.Player.Move.canceled += OnMovementCanceled;
        inputs.Player.Jump.performed += OnJumpPerformed;
        inputs.Player.Jump.canceled += OnJumpCanceled;
        inputs.Player.Crouch.performed += ctx => OnCrouch?.Invoke(true);
        inputs.Player.Crouch.canceled += ctx => OnCrouch?.Invoke(false);
        // started = al presionar (funciona aunque la accion tenga la interaccion "Hold")
        inputs.Player.Interact.started += ctx => OnInteract?.Invoke();
    }

    private static void OnMovementPerformed(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        OnMove?.Invoke(moveInput);
    }

    private static void OnMovementCanceled(InputAction.CallbackContext context)
    {
        OnMove?.Invoke(Vector2.zero);
    }

    private static void OnJumpPerformed(InputAction.CallbackContext context)
    {
        OnJump?.Invoke();
    }

    private static void OnJumpCanceled(InputAction.CallbackContext context)
    {
        OnJumpReleased?.Invoke();
    }
}
