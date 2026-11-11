using UnityEngine;

namespace RagDoll.Levels
{
    /// <summary>
    /// Camara 2D que sigue a un objetivo con suavizado y, opcionalmente, se detiene en los bordes
    /// del nivel. Solo conoce un Transform: no depende de PlayerController ni de ninguna otra clase
    /// del juego, asi sirve para cualquier escenario.
    ///
    /// Va en la Main Camera (que debe ser ortografica).
    /// Se mueve en LateUpdate para actuar DESPUES de que el jugador ya se movio en ese frame.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Objetivo")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 offset = new Vector2(0f, 1f);

        [Header("Suavizado")]
        [SerializeField] private float smoothTime = 0.15f; // mas alto = camara mas lenta
        [SerializeField] private bool followVertical = true; // false = solo scroll horizontal

        [Header("Limites del nivel (opcional)")]
        [SerializeField] private bool useBounds;
        [SerializeField] private Vector2 minBounds = new Vector2(-10f, -5f);
        [SerializeField] private Vector2 maxBounds = new Vector2(30f, 10f);

        private Camera cam;
        private Vector3 velocity;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = new Vector3(
                target.position.x + offset.x,
                followVertical ? target.position.y + offset.y : transform.position.y,
                transform.position.z);

            if (useBounds)
                desired = ClampToBounds(desired);

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }

        /// <summary>Evita que la camara muestre lo que esta fuera del nivel.</summary>
        private Vector3 ClampToBounds(Vector3 position)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            position.x = ClampAxis(position.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
            position.y = ClampAxis(position.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);
            return position;
        }

        private static float ClampAxis(float value, float min, float max)
        {
            // Si el nivel es mas pequeno que la camara, se centra en vez de temblar.
            if (min > max) return (min + max) * 0.5f;
            return Mathf.Clamp(value, min, max);
        }

        /// <summary>Permite asignar el objetivo por codigo (ej. al reaparecer el jugador).</summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void OnDrawGizmosSelected()
        {
            if (!useBounds) return;

            Gizmos.color = Color.yellow;
            Vector3 center = (minBounds + maxBounds) * 0.5f;
            Vector3 size = maxBounds - minBounds;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
