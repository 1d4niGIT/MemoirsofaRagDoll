using UnityEngine;

namespace RagDoll.Levels
{
    /// <summary>
    /// Plataforma que flota suave de arriba abajo CON su collider, y lleva a Mino encima.
    /// Se mueve con un Rigidbody2D Kinematic en FixedUpdate (la fisica ve el movimiento y arrastra
    /// a lo que este parado encima). Mover el Transform directo NO sirve: Mino no se movería con ella.
    ///
    /// Ponlo en el objeto que tiene el Collider2D (el sprite puede ser hijo o el mismo objeto).
    /// No se inclina a proposito: una plataforma que gira hace resbalar a Mino.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class FloatingPlatform : MonoBehaviour
    {
        [SerializeField] private float amplitude = 0.08f;   // unidades que sube/baja
        [SerializeField] private float speed = 0.8f;        // ciclos por segundo
        [SerializeField] private bool randomPhase = true;

        private Rigidbody2D body;
        private Vector2 startPosition;
        private float phase;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate; // movimiento sin tirones
            startPosition = body.position;
            phase = randomPhase ? Random.value * Mathf.PI * 2f : 0f;
        }

        private void FixedUpdate()
        {
            float t = Time.time * speed * Mathf.PI * 2f + phase;
            body.MovePosition(startPosition + Vector2.up * (Mathf.Sin(t) * amplitude));
        }
    }
}
