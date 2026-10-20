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
        Vector3 targetPos = new Vector3(
             target.transform.position.x + Offset.x, 
             target.transform.position.y + Offset.y, -10);
        Vector3 mousePos = Camera.main.ScreenToViewportPoint(Mouse.current.position.ReadValue()) - new Vector3(0.5f,0.5f,0.5f);
        //Vector2 mousePos = Camera.main.ScreenToViewportPoint(Mouse.current.position.ReadValue()).normalized;
        Vector3 DinamicPos = targetPos + mousePos * DinamicCamera;

        transform.position = Vector3.Lerp(
            transform.position, 
            DinamicPos,
            followSpeed * Time.deltaTime);

    }
}
