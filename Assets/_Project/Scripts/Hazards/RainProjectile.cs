using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Hazards
{
  
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class RainProjectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private float gravityScale = 1f;

        [Header("Visual")]
        [SerializeField] private bool rotateWithVelocity = true;
        [SerializeField] private float spriteAngleOffset = -90f;

        private Rigidbody2D body;
        private Collider2D hitbox;
        private bool resolved;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            hitbox = GetComponent<Collider2D>();
            hitbox.isTrigger = false;

            body.gravityScale = gravityScale;
            body.linearDamping = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

      
        public void Launch(Vector2 velocity, Collider2D[] ignored)
        {
            if (ignored != null)
            {
                foreach (Collider2D other in ignored)
                    if (other != null) Physics2D.IgnoreCollision(hitbox, other);
            }

            body.linearVelocity = velocity;
            Destroy(gameObject, lifetime);
        }

        private void FixedUpdate()
        {
            if (!rotateWithVelocity || resolved) return;

            Vector2 v = body.linearVelocity;
            if (v.sqrMagnitude > 0.01f)
                body.rotation = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg + spriteAngleOffset;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (resolved) return;

            if (collision.collider.GetComponent<RainProjectile>() != null)
            {
                Physics2D.IgnoreCollision(hitbox, collision.collider);
                return;
            }

            resolved = true;

            IKillable killable = collision.collider.GetComponentInParent<IKillable>();
            if (killable != null) killable.Kill();

            Destroy(gameObject);
        }
    }
}