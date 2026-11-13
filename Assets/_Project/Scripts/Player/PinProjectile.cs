using System;
using UnityEngine;

namespace RagDoll.Player
{
    /// <summary>
    /// El alfiler de cabeza redonda que Mino lanza desde la espalda.
    /// Vuela en linea recta a la velocidad que le den; su alcance es velocidad x tiempo de vida.
    /// Si choca con una superficie blanda (capa "Soft") se clava y avisa con Embedded.
    /// Si choca con cualquier otra cosa, o se agota su tiempo, avisa con Failed y se destruye.
    ///
    /// No sabe nada de hilos ni de joints: solo vuela y avisa (S de SOLID).
    ///
    /// Prefab: Rigidbody2D (Dynamic), un Collider2D NO trigger y este script.
    /// El sprite debe mirar hacia la derecha (+X).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PinProjectile : MonoBehaviour
    {
        [SerializeField] private float maxLifetime = 1f;     // alcance = velocidad x esto
        [SerializeField] private LayerMask softLayers;       // capas donde se clava

        /// <summary>Se clavo en una superficie blanda.</summary>
        public event Action<PinProjectile> Embedded;

        /// <summary>No se clavo: choco con algo duro o se agoto su tiempo.</summary>
        public event Action<PinProjectile> Failed;

        public Rigidbody2D Body { get; private set; }

        private bool resolved;

        public void Launch(Vector2 direction, float speed, Collider2D ownerCollider)
        {
            Body = GetComponent<Rigidbody2D>();
            Body.gravityScale = 0f;
            Body.freezeRotation = true;  // el alfiler no gira por choques: conserva la direccion con que salio
            Body.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // evita atravesar colliders finos

            // El alfiler no debe chocar con quien lo lanza.
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), ownerCollider);

            Vector2 dir = direction.normalized;
            transform.right = dir;
            Body.linearVelocity = dir * speed;

            Invoke(nameof(Fail), maxLifetime);
        }

        /// <summary>Quita el alfiler (cuando Mino suelta el hilo).</summary>
        public void Release()
        {
            Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (resolved) return;

            if (IsSoft(collision.collider)) Embed(collision);
            else Fail();
        }

        private void Embed(Collision2D collision)
        {
            resolved = true;
            CancelInvoke();

            transform.position = collision.GetContact(0).point;
            Body.linearVelocity = Vector2.zero;
            Body.angularVelocity = 0f;   // sin esto, un cuerpo Kinematic seguiria girando para siempre
            Body.bodyType = RigidbodyType2D.Kinematic;
            transform.SetParent(collision.collider.transform, true); // acompana a la plataforma si se mueve

            Embedded?.Invoke(this);
        }

        private void Fail()
        {
            if (resolved) return;
            resolved = true;

            Failed?.Invoke(this);
            Destroy(gameObject);
        }

        private bool IsSoft(Collider2D other)
        {
            return (softLayers.value & (1 << other.gameObject.layer)) != 0;
        }
    }
}
