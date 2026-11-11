using UnityEngine;

namespace RagDoll.Player
{
    /// <summary>
    /// Responde una sola pregunta: "estoy tocando el suelo?".
    /// Es un componente aparte (composicion sobre herencia) para que cualquier habilidad
    /// lo reutilice: salto, planeo, agacharse...
    /// </summary>
    public class GroundSensor : MonoBehaviour
    {
        [SerializeField] private Transform checkPoint;      // hijo vacio colocado en los pies
        [SerializeField] private float radius = 0.15f;
        [SerializeField] private LayerMask groundLayers;    // capa "Ground"

        /// <summary>Se consulta bajo demanda, asi no depende del orden de ejecucion de scripts.</summary>
        public bool IsGrounded
        {
            get
            {
                Vector2 origin = checkPoint != null ? checkPoint.position : transform.position;
                return Physics2D.OverlapCircle(origin, radius, groundLayers) != null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = checkPoint != null ? checkPoint.position : transform.position;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(origin, radius);
        }
    }
}
