using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed;
    public Vector2 Offset;

    public float DinamicCamera;
    void Update()
    {
        Vector3 targePos = new Vector3(
            target.transform.position.x + Offset.x,
            target.transform.position.y + Offset.y,
            -10);
        

        Vector3 mousePos = Camera.main.ScreenToViewportPoint(Mouse.current.position.ReadValue()) - new Vector3(0.5f,0.5f,0.5f);

        Vector3 DinamicPos = targePos + mousePos * DinamicCamera;

        transform.position = Vector3.Lerp(
            transform.position,
            DinamicPos,
            followSpeed * Time.deltaTime
            );

    }

}
