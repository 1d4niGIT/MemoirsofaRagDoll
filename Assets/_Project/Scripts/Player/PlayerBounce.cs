using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Hace que Mino pueda rebotar. Es un componente aparte (composicion), igual que PlayerKnockback:
    /// ni el controlador ni las habilidades saben de camas. Solo pone la velocidad vertical;
    /// el movimiento lateral lo sigue controlando el jugador, asi puede dirigir el rebote.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerBounce : MonoBehaviour, IBounceable
    {
        private Rigidbody2D body;

        private void Awake() => body = GetComponent<Rigidbody2D>();

        public void Bounce(float upwardSpeed)
        {
            Vector2 velocity = body.linearVelocity;
            velocity.y = upwardSpeed;
            body.linearVelocity = velocity;
        }
    }
}
