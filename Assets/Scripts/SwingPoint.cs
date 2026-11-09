using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class SwingPoint : MonoBehaviour
{
    public float length = 3f;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, length);
    }
}
