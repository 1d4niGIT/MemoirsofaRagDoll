using UnityEngine;

namespace RagDoll.Levels
{
    /// <summary>
    /// Plataforma que se hunde cuando alguien cae ENCIMA con suficiente fuerza (ej. la lampara).
    /// Solo dispara la animacion: el Animator cambia los frames y activa el collider de cada frame.
    ///
    /// Se activa unicamente en el instante del choque (OnCollisionEnter2D) y solo si:
    ///  - el objeto esta en una de las capas permitidas (Trigger Layers),
    ///  - el choque es VERTICAL y fuerte (Min Impact Speed, solo componente vertical),
    ///  - el contacto ocurre en la parte baja del objeto (cae ENCIMA, no de lado ni por debajo).
    ///
    /// Va en el objeto padre (el que tiene el Rigidbody2D); recibe los choques de los colliders hijos.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class SaggingPlatform : MonoBehaviour
    {
        [SerializeField] private LayerMask triggerLayers;            // capa de Mino ("Player")
        [SerializeField] private string sagTrigger = "Sag";          // trigger del Animator
        [SerializeField] private float minImpactSpeed = 16f;         // velocidad vertical minima del golpe
        [SerializeField] private bool logImpactSpeed;                // activalo para calibrar y depurar

        private Animator animator;
        private bool sagged;

        private void Awake() => animator = GetComponent<Animator>();

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (sagged) return;
            if ((triggerLayers.value & (1 << collision.gameObject.layer)) == 0) return;

            // Solo cuenta la velocidad vertical: un empujon lateral no activa la lampara.
            float impact = Mathf.Abs(collision.relativeVelocity.y);

            if (impact < minImpactSpeed)
            {
                Log("rechazado: impacto vertical " + impact.ToString("F1") + " < minimo " + minImpactSpeed);
                return;
            }

            // Cae ENCIMA si algun punto de contacto esta por debajo del centro del objeto.
            // (No se compara con los pies: a mucha velocidad el objeto se hunde antes de detectar el choque.)
            float centerY = collision.collider.bounds.center.y;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.point.y <= centerY)
                {
                    sagged = true;
                    Log("ACTIVADA: impacto vertical " + impact.ToString("F1"));
                    animator.SetTrigger(sagTrigger);
                    return;
                }
            }

            Log("rechazado: el contacto no esta en la parte baja del objeto (centro Y " + centerY.ToString("F2") + ")");
        }

        private void Log(string message)
        {
            if (logImpactSpeed)
                Debug.Log(name + ": " + message, this);
        }
    }
}