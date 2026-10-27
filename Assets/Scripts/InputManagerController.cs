using System;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputManagerController 
{
    private static InputSystem_Actions inputs;

    public static Action<Vector2> OnMove;
    public static Action OnJump;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        inputs = new InputSystem_Actions();
        inputs.Enable();

        inputs.Player.Move.performed += OnMovementPerformed;
        inputs.Player.Move.canceled += OnMovementCanceled;
        inputs.Player.Jump.performed += OnJumpPerformed;
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
}
