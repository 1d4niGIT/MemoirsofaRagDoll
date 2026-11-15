using System;
using UnityEngine;

namespace RagDoll.Player
{
    /// <summary>
    /// El alfiler de cabeza redonda que Mino lanza desde la espalda.
    /// Sale con una velocidad inicial y cae por la gravedad, describiendo una parabola.
    /// Mientras vuela, gira para apuntar siempre hacia donde se mueve.
    /// Si choca con una superficie blanda (capa "Soft") se clava y avisa con Embedded.
    /// Si choca con cualquier otra cosa, o se agota su tiempo, avisa con Failed y se destruye.
    ///
    /// No sabe nada de hilos ni de joints: solo vuela y avisa (S de SOLID).
    ///
    /// Prefab: Rigidbody2D (Dynamic), un Collider2D NO trigger y este script.
    /// El sprite debe mirar hacia la derecha (+X); lo ideal es que su pivote este en la punta.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PinProjectile : MonoBehaviour
    {
        [SerializeField] private float gravityScale = 1f;    // 1 = la gravedad de Physics2D
        [SerializeField] private float maxLifetime = 2.5f;   // segundos antes de rendirse (debe cubrir la caida)
        [SerializeField] private LayerMask softLayers;       // capas donde se clava

        /// <summary>Se clavo en una superficie blanda.</summary>
        public event Action<PinProjectile> Embedded;

        /// <summary>No se clavo: choco con algo duro o se agoto su tiempo.</summary>
        public event Action<PinProjectile> Failed;

        public Rigidbody2D Body { get; private set; }

        /// <summary>Se exponen para que la guia de trayectoria use los mismos valores que el vuelo real.</summary>
        public float GravityScale => gravityScale;
        public float MaxLifetime => maxLifetime;

        private bool resolved;
        private float lifeTimer;

        public void Launch(Vector2 direction, float speed, Collider2D ownerCollider)
        {
            Body = GetComponent<Rigidbody2D>();
            Body.gravityScale = gravityScale;
            Body.linearDamping = 0f;     // sin rozamiento del aire: asi la parabola es exacta
            Body.freezeRotation = true;  // los choques no lo hacen girar; la rotacion la dirige el script
            Body.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // evita atravesar colliders finos

            // El alfiler no debe chocar con quien lo lanza.
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), ownerCollider);

            Vector2 dir = direction.normalized;
            transform.right = dir;
            Body.linearVelocity = dir * speed;
        }

        /// <summary>Quita el alfiler (cuando Mino suelta el hilo).</summary>
        public void Release()
        {
            Destroy(gameObject);
        }

        private void FixedUpdate()
        {
            if (resolved || Body == null) return;

            // Tiempo de vida con un contador (mas barato y claro que Invoke por nombre).
            lifeTimer += Time.fixedDeltaTime;
            if (lifeTimer >= maxLifetime)
            {
                Fail();
                return;
            }

            // El alfiler apunta hacia donde se mueve (tangente de la parabola).
            Vector2 velocity = Body.linearVelocity;
            if (velocity.sqrMagnitude > 0.01f)
                Body.rotation = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
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