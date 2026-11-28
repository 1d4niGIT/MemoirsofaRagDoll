using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Hazards
{
    /// <summary>
    /// Penca que CAE desde arriba. Hereda todo de KnockbackHazard: el dano (campo Damage), el hilo
    /// que se suelta (VFX), el relleno cuando no queda hilo y el empujon son los de siempre.
    /// Aqui solo se agrega lo propio de una penca que cae:
    /// - se prepara con Init(): dibujo, capa de dibujo (sorting order), tamano, si hace dano o es decorativa;
    /// - el collider se ajusta al dibujo elegido (hay varios dibujos en un solo prefab);
    /// - al golpear a alguien o tocar el suelo se desvanece y desaparece.
    ///
    /// Prefab: Rigidbody2D (Dynamic), SpriteRenderer, CapsuleCollider2D con Is Trigger, este script.
    /// Capa fisica: la misma de las demas pencas (ej. Hazard), y NO incluida en "Ceiling Layers".
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class FallingPencaHazard : KnockbackHazard
    {
        [Header("Caida")]
        [SerializeField] private float maxFallSpeed = 14f;      // tope de velocidad, evita que "atraviese" a Mino

        [Header("Collider")]
        [SerializeField, Range(0.2f, 1f)] private float colliderShrink = 0.6f;   // collider = dibujo x este factor

        [Header("Desaparecer")]
        [SerializeField] private float fadeTime = 0.3f;
        [SerializeField] private float maxLifetime = 6f;        // seguro por si algo falla

        private Rigidbody2D body;
        private SpriteRenderer art;
        private CapsuleCollider2D capsule;
        private LayerMask groundLayers;
        private bool canDamage = true;
        private bool vanishing;
        private float vanishTimer;
        private Color baseColor = Color.white;
        private Vector3 baseScale = Vector3.one;   // escala del prefab: sirve para agrandar todas a la vez

        /// <summary>Velocidad maxima de caida (la usa la zona para calcular cuanto tarda en llegar).</summary>
        public float MaxFallSpeed => maxFallSpeed;

        /// <summary>Aviso para quien la creo (la zona lleva la cuenta de cuantas hay en pantalla).</summary>
        public event System.Action<FallingPencaHazard> Despawned;

        protected override void Awake()
        {
            base.Awake();
            baseScale = transform.localScale;
            body = GetComponent<Rigidbody2D>();
            art = GetComponentInChildren<SpriteRenderer>();
            capsule = GetComponent<CapsuleCollider2D>();
        }

        /// <summary>Prepara esta penca justo despues de crearla.</summary>
        public void Init(Sprite sprite, string sortingLayer, int sortingOrder, bool damages,
                         float scale, Color tint, LayerMask ground, float gravityScale, float spin)
        {
            if (art != null)
            {
                art.sprite = sprite;
                art.sortingLayerName = sortingLayer;
                art.sortingOrder = sortingOrder;
                art.color = baseColor = tint;
            }

            transform.localScale = baseScale * scale;   // escala del prefab x escala de la capa
            transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-20f, 20f));

            // Un solo prefab para varios dibujos: el collider se adapta al sprite elegido.
            if (capsule != null && sprite != null)
            {
                Bounds b = sprite.bounds;
                Vector2 size = b.size * colliderShrink;
                capsule.size = size;
                capsule.offset = b.center;
                capsule.direction = size.x > size.y ? CapsuleDirection2D.Horizontal : CapsuleDirection2D.Vertical;
            }

            canDamage = damages;
            if (!damages && capsule != null) capsule.enabled = false;   // decorativa: no toca a nadie

            groundLayers = ground;
            body.gravityScale = gravityScale;
            body.angularVelocity = spin;

            Destroy(gameObject, maxLifetime);
        }

        protected override bool CanHit() => canDamage && !vanishing && base.CanHit();

        /// <summary>Gancho de KnockbackHazard: se ejecuta cuando golpea a alguien.</summary>
        protected override void OnHitExtra(Collider2D other) => Vanish();

        private void Update()
        {
            if (vanishing)
            {
                vanishTimer += Time.deltaTime;
                if (art != null)
                {
                    Color c = baseColor;
                    c.a = baseColor.a * (1f - Mathf.Clamp01(vanishTimer / fadeTime));
                    art.color = c;
                }
                if (vanishTimer >= fadeTime) Destroy(gameObject);
                return;
            }

            Vector2 v = body.linearVelocity;
            if (v.y < -maxFallSpeed)
            {
                v.y = -maxFallSpeed;
                body.linearVelocity = v;
            }

            CheckGround();
        }

        private void CheckGround()
        {
            if (groundLayers.value == 0 || art == null) return;

            Bounds b = art.bounds;
            Vector2 feet = new Vector2(b.center.x, b.min.y);
            if (Physics2D.OverlapCircle(feet, 0.05f, groundLayers) == null) return;

            // Toco el suelo: se queda quieta y se desvanece.
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            Vanish();
        }

        private void Vanish()
        {
            if (vanishing) return;
            vanishing = true;
            if (capsule != null) capsule.enabled = false;   // ya no hace mas dano
        }

        private void OnDestroy() => Despawned?.Invoke(this);
    }
}