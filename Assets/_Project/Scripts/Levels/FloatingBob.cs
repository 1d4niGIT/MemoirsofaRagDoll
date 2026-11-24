using UnityEngine;

namespace RagDoll.Levels
{
    /// <summary>
    /// Hace que un objeto luzca flotando ligeramente: sube y baja suave (y se inclina un poco).
    /// Ponlo en el HIJO que tiene el SpriteRenderer, NO en el objeto con el collider:
    /// asi el collider queda quieto y Mino no tiembla al pararse encima del mueble.
    /// Cada mueble arranca en un momento distinto (fase aleatoria) para que no se muevan al unisono.
    /// </summary>
    public class FloatingBob : MonoBehaviour
    {
        [Header("Subida y bajada")]
        [SerializeField] private float amplitude = 0.08f;   // unidades de Unity que sube/baja
        [SerializeField] private float speed = 0.8f;        // ciclos por segundo (mas bajo = mas lento)

        [Header("Inclinacion (opcional)")]
        [SerializeField] private float tiltDegrees = 1.5f;  // 0 = sin inclinar

        [Header("Variacion")]
        [SerializeField] private bool randomPhase = true;

        private Vector3 startLocalPosition;
        private Quaternion startLocalRotation;
        private float phase;

        private void Awake()
        {
            startLocalPosition = transform.localPosition;
            startLocalRotation = transform.localRotation;
            phase = randomPhase ? Random.value * Mathf.PI * 2f : 0f;
        }

        private void Update()
        {
            float t = Time.time * speed * Mathf.PI * 2f + phase;

            float offset = Mathf.Sin(t) * amplitude;
            transform.localPosition = startLocalPosition + Vector3.up * offset;

            if (tiltDegrees != 0f)
            {
                float tilt = Mathf.Sin(t * 0.7f + 1f) * tiltDegrees;
                transform.localRotation = startLocalRotation * Quaternion.Euler(0f, 0f, tilt);
            }
        }
    }
}
