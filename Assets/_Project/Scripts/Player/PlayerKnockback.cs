using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Hace que Mino pueda ser empujado. Es un componente aparte (composicion): ni la vida ni
    /// el controlador saben de pencas. Pone la velocidad del golpe y le pide al PlayerController
    /// que no la pise durante un instante.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(PlayerController))]
    public class PlayerKnockback : MonoBehaviour, IKnockbackable
    {
        private Rigidbody2D body;
        private PlayerController controller;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            controller = GetComponent<PlayerController>();
        }

        public void ApplyKnockback(Vector2 velocity, float duration)
        {
            body.linearVelocity = velocity;
            controller.LockMovement(duration);
        }
    }
}
