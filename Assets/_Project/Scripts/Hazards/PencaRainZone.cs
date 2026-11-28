using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Hazards
{
    /// <summary>
    /// Zona del nivel donde caen pencas mientras Mino la recorre AGACHADO.
    /// - Empieza cuando Mino esta dentro de la zona (y agachado, si "onlyWhileCrouching").
    /// - Se detiene cuando sale de la zona o se levanta; las que ya caen terminan su caida.
    /// - Cada penca elige al azar uno de los dibujos y una "capa de profundidad"
    ///   (sorting order, tamano, tinte y si hace dano o es solo decoracion) para dar sensacion de fondo.
    ///
    /// El dano, el hilo, el relleno y el empujon NO estan aqui: son los de FallingPencaHazard
    /// (KnockbackHazard). Esta clase solo decide cuando y donde caen.
    ///
    /// Va en un objeto vacio con un Collider2D en Is Trigger que cubra el recorrido
    /// (desde el inicio hasta el final de las pencas, de piso a un poco mas arriba del techo visible).
    /// Usa ICrouchState: no conoce al jugador.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PencaRainZone : MonoBehaviour
    {
        [System.Serializable]
        public class DepthLayer
        {
            public int sortingOrder = 0;
            public bool damages = true;                 // false = decoracion de fondo
            [Range(0.4f, 4f)] public float scale = 1f;   // se multiplica por la escala del prefab
            public Color tint = Color.white;
        }

        [Header("Piezas")]
        [SerializeField] private FallingPencaHazard pencaPrefab;
        [SerializeField] private Sprite[] sprites;                       // los 9 dibujos
        [SerializeField] private string sortingLayerName = "Default";
        [SerializeField] private DepthLayer[] depthLayers;
        [SerializeField] private LayerMask groundLayers;                 // donde se detienen

        [Header("Cuando caen")]
        [SerializeField] private bool onlyWhileCrouching = true;
        [SerializeField] private int maxActive = 14;                     // maximo en pantalla a la vez (suma de los dos tipos)

        [Header("Pencas dirigidas a Mino")]
        [SerializeField] private float minInterval = 0.3f;               // segundos entre una y otra
        [SerializeField] private float maxInterval = 0.8f;

        [Header("Pencas por toda la zona")]
        [SerializeField] private bool ambientEnabled = true;
        [SerializeField] private float ambientMinInterval = 0.2f;        // caen en un punto al azar de todo el ancho de la zona
        [SerializeField] private float ambientMaxInterval = 0.6f;

        [Header("Donde aparecen")]
        [SerializeField] private float spawnHeightOffset = 1f;           // por encima del borde superior de la zona
        [SerializeField] private float aimJitter = 1.5f;                 // dispersion alrededor de Mino
        [SerializeField] private bool leadTarget = true;                 // apunta a donde ESTARA Mino cuando la penca llegue
        [SerializeField, Range(0f, 1.5f)] private float leadAmount = 1f; // 1 = adelanto completo, 0 = apunta a donde esta ahora

        [Header("Caida")]
        [SerializeField] private Vector2 gravityRange = new Vector2(1.5f, 2.5f);
        [SerializeField] private float maxSpin = 90f;                    // grados por segundo

        [Header("Depuracion")]
        [SerializeField] private bool debugLog = false;                  // escribe en el Console cuando entra Mino y cuando aparece una penca

        private Collider2D zone;
        private ICrouchState target;
        private Transform targetTransform;
        private Rigidbody2D targetBody;
        private float nextAimedSpawn;
        private float nextAmbientSpawn;
        private int active;

        private void Awake()
        {
            zone = GetComponent<Collider2D>();
            if (!zone.isTrigger)
                Debug.LogWarning(name + ": el Collider2D de la zona debe estar en Is Trigger.", this);
        }

        // Stay (y no Enter): si Mino tiene varios colliders, se recupera solo aunque uno salga.
        private void OnTriggerStay2D(Collider2D other)
        {
            if (target != null) return;

            ICrouchState crouch = other.GetComponentInParent<ICrouchState>();
            if (crouch == null) return;

            target = crouch;
            targetBody = other.attachedRigidbody;
            targetTransform = targetBody != null ? targetBody.transform : other.transform;
            nextAimedSpawn = Time.time + minInterval;
            nextAmbientSpawn = Time.time + ambientMinInterval;

            if (debugLog) Debug.Log(name + ": Mino entro a la zona.", this);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (target == null) return;

            Transform t = other.attachedRigidbody != null ? other.attachedRigidbody.transform : other.transform;
            if (t == targetTransform) ClearTarget();
        }

        private void Update()
        {
            if (target == null) return;
            if (targetTransform == null) { ClearTarget(); return; }

            if (onlyWhileCrouching && !target.IsCrouching) return;

            // Dos "grifos" independientes: uno dirigido a Mino y otro repartido por toda la zona.
            if (active < maxActive && Time.time >= nextAimedSpawn)
            {
                nextAimedSpawn = Time.time + Random.Range(minInterval, maxInterval);
                Spawn(true);
            }

            if (ambientEnabled && active < maxActive && Time.time >= nextAmbientSpawn)
            {
                nextAmbientSpawn = Time.time + Random.Range(ambientMinInterval, ambientMaxInterval);
                Spawn(false);
            }
        }

        /// <param name="aimed">true = cae cerca de Mino (con adelanto); false = en cualquier punto de la zona.</param>
        private void Spawn(bool aimed)
        {
            if (pencaPrefab == null || sprites == null || sprites.Length == 0) return;

            Bounds b = zone.bounds;

            float gravityScale = Random.Range(gravityRange.x, gravityRange.y);
            float spawnY = b.max.y + spawnHeightOffset;

            float x;
            if (aimed)
            {
                x = targetTransform.position.x;

                // Adelanto: cuanto avanzara Mino mientras la penca cae.
                if (leadTarget && targetBody != null)
                {
                    float fallTime = FallTime(spawnY - targetTransform.position.y, gravityScale);
                    x += targetBody.linearVelocity.x * fallTime * leadAmount;
                }

                x += Random.Range(-aimJitter, aimJitter);
            }
            else
            {
                x = Random.Range(b.min.x, b.max.x);
            }

            x = Mathf.Clamp(x, b.min.x, b.max.x);

            Vector3 position = new Vector3(x, spawnY, 0f);

            Sprite sprite = sprites[Random.Range(0, sprites.Length)];

            int order = 0;
            bool damages = true;
            float scale = 1f;
            Color tint = Color.white;

            if (depthLayers != null && depthLayers.Length > 0)
            {
                DepthLayer layer = depthLayers[Random.Range(0, depthLayers.Length)];
                order = layer.sortingOrder;
                damages = layer.damages;
                scale = layer.scale;
                tint = layer.tint;

                // Elementos nuevos de la lista pueden quedar en cero: se corrigen para que no queden invisibles.
                if (scale <= 0.01f) scale = 1f;
                if (tint.a <= 0.01f) tint = Color.white;
            }

            FallingPencaHazard penca = Instantiate(pencaPrefab, position, Quaternion.identity);
            penca.Despawned += HandleDespawned;
            active++;

            if (debugLog)
                Debug.Log(name + ": penca " + sprite.name + " en " + position + " (orden " + order + ", dano " + damages + ").", penca);

            penca.Init(sprite, sortingLayerName, order, damages, scale, tint, groundLayers,
                       gravityScale,
                       Random.Range(-maxSpin, maxSpin));
        }

        /// <summary>Tiempo que tarda en caer una altura, con la gravedad y el tope de velocidad de la penca.</summary>
        private float FallTime(float height, float gravityScale)
        {
            float g = Mathf.Abs(Physics2D.gravity.y) * gravityScale;
            if (height <= 0f || g < 0.01f) return 0f;

            float vMax = pencaPrefab.MaxFallSpeed;
            float timeToMax = vMax / g;
            float distToMax = 0.5f * g * timeToMax * timeToMax;

            if (height <= distToMax) return Mathf.Sqrt(2f * height / g);
            return timeToMax + (height - distToMax) / vMax;
        }

        private void HandleDespawned(FallingPencaHazard penca)
        {
            active = Mathf.Max(0, active - 1);
        }

        private void ClearTarget()
        {
            target = null;
            targetTransform = null;
            targetBody = null;
        }

        private void OnDrawGizmosSelected()
        {
            Collider2D c = GetComponent<Collider2D>();
            if (c == null) return;

            Bounds b = c.bounds;
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(b.center, b.size);

            // Linea donde aparecen las pencas.
            float y = b.max.y + spawnHeightOffset;
            Gizmos.DrawLine(new Vector3(b.min.x, y), new Vector3(b.max.x, y));
        }
    }
}