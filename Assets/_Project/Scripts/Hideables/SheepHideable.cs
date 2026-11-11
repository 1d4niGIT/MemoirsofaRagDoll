using UnityEngine;

namespace RagDoll.Hideables
{
    /// <summary>
    /// Oveja de lana: se pasea de un lado a otro y lleva consigo a quien se esconde en ella,
    /// porque HideAbility pone al jugador como hijo del HidePoint.
    /// Es polimorfismo aplicado a gameplay: toda la logica de esconderse viene de HideableBase,
    /// esta clase solo agrega el movimiento.
    ///
    /// Consejo: deja la escala de la oveja y de su HidePoint en (1, 1, 1) para que el jugador
    /// no se deforme al ser hijo.
    /// </summary>
    public class SheepHideable : HideableBase
    {
        [SerializeField] private float patrolDistance = 3f; // distancia maxima desde el punto inicial
        [SerializeField] private float speed = 1f;

        private float startX;
        private int direction = 1;

        private void Start()
        {
            startX = transform.position.x;
        }

        private void Update()
        {
            Vector3 position = transform.position;
            position.x += direction * speed * Time.deltaTime;

            float offset = position.x - startX;
            if (Mathf.Abs(offset) >= patrolDistance)
            {
                position.x = startX + Mathf.Sign(offset) * patrolDistance;
                direction = -direction;
            }

            transform.position = position;
        }
    }
}
