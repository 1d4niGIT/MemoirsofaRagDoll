using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimplePlayer2D : MonoBehaviour
{
    public InputSystemActions inputs;
    public Vector2 MoveInput;
    public float MoveSpeed;

    private void Awake()
    {
        inputs = new();
    }

    private void OnEnable()
    {
        inputs.Enable();
        inputs.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputs.Player.Move.canceled += ctx => MoveInput = Vector2.zero;
    }

    void Update()
    {
        OnMove();
    }

    public void OnMove()
    {
        if (MoveInput != Vector2.zero)
            transform.position += (Vector3)MoveInput * MoveSpeed * Time.deltaTime;
    }
}
