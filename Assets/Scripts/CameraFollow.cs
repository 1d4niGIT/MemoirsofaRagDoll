using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followspeed;
    public Vector2 offset;
    public float DinamicCamera;
    void Update()
    {
        Vector3 targetPos = new Vector3(target.transform.position.x + offset.x, target.position.y + offset.y, -10);


        Vector3 mousePos = Camera.main.ScreenToViewportPoint(Mouse.current.position.ReadValue()) - new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 DinamicPos = targetPos + mousePos * DinamicCamera;
                                  
        transform.position = Vector3.Lerp(transform.position,DinamicPos,followspeed * Time.deltaTime);
    }
}
