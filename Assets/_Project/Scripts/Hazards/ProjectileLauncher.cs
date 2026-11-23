using UnityEngine;

namespace RagDoll.Hazards
{
    /// <summary>
    /// La caja de costuras del dibujo: lanza proyectiles hacia ARRIBA (la direccion "up" del
    /// punto de salida) para que parezca lluvia.
    ///
    /// Cada disparo saca una cantidad aleatoria de proyectiles (entre minProjectiles y
    /// maxProjectiles) AL MISMO TIEMPO, y cada uno aparece en un punto al azar dentro de la zona de
    /// salida (un rectangulo que se ve en el editor como un cuadro cian). El tiempo hasta el
    /// siguiente disparo tambien es aleatorio (entre minInterval y maxInterval).
    ///
    /// Solo se encarga de DISPARAR; lo que pasa despues (caer, chocar, matar) es de RainProjectile.
    ///
    /// Uso: ponlo en el objeto de la caja, asigna el prefab con RainProjectile y ajusta la zona
    /// de salida (spawnSize / spawnOffset) para que cubra la caja. Si quieres, usa un hijo vacio
    /// como punto de salida (muzzle); si no hay muzzle usa el propio transform.
    /// </summary>
    public class ProjectileLauncher : MonoBehaviour
    {
        [SerializeField] private RainProjectile projectilePrefab;
        [SerializeField] private Transform muzzle;

        [Header("Cadencia")]
        [SerializeField] private float minInterval = 0.8f;
        [SerializeField] private float maxInterval = 1.5f;
        [SerializeField] private float initialDelay = 0f;
        [SerializeField] private int minProjectiles = 1;
        [SerializeField] private int maxProjectiles = 3;

        [Header("Zona de salida")]
        [SerializeField] private Vector2 spawnSize = new Vector2(2f, 0.5f);
        [SerializeField] private Vector2 spawnOffset = Vector2.zero;

        [Header("Lanzamiento")]
        [SerializeField] private float launchSpeed = 8f;
        [SerializeField] private float speedVariance = 0f;
        [SerializeField] private float spreadAngle = 0f;

        private Collider2D[] ownColliders;
        private float nextFireTime;

        private Transform Origin => muzzle != null ? muzzle : transform;

        private void OnValidate()
        {
            minInterval = Mathf.Max(0f, minInterval);
            maxInterval = Mathf.Max(minInterval, maxInterval);
            minProjectiles = Mathf.Max(0, minProjectiles);
            maxProjectiles = Mathf.Max(minProjectiles, maxProjectiles);
            spawnSize = new Vector2(Mathf.Max(0f, spawnSize.x), Mathf.Max(0f, spawnSize.y));
        }

        private void Awake()
        {
            ownColliders = GetComponentsInChildren<Collider2D>();
        }

        private void Start()
        {
            nextFireTime = Time.time + initialDelay;
        }

        private void Update()
        {
            if (Time.time < nextFireTime) return;

            Fire();
            nextFireTime = Time.time + Random.Range(minInterval, maxInterval);
        }

        private void Fire()
        {
            if (projectilePrefab == null) return;

            Transform origin = Origin;
            int count = Random.Range(minProjectiles, maxProjectiles + 1);

            for (int i = 0; i < count; i++)
            {
                Vector2 local = spawnOffset + new Vector2(
                    Random.Range(-spawnSize.x * 0.5f, spawnSize.x * 0.5f),
                    Random.Range(-spawnSize.y * 0.5f, spawnSize.y * 0.5f));
                Vector3 position = origin.position + origin.rotation * (Vector3)local;

                float angle = Random.Range(-spreadAngle, spreadAngle);
                Vector2 direction = Quaternion.Euler(0f, 0f, angle) * origin.up;

                float speed = launchSpeed + Random.Range(-speedVariance, speedVariance);

                RainProjectile projectile = Instantiate(projectilePrefab, position, Quaternion.identity);
                projectile.Launch(direction.normalized * speed, ownColliders);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Transform origin = Origin;
            Gizmos.color = Color.cyan;

            Gizmos.matrix = Matrix4x4.TRS(origin.position, origin.rotation, Vector3.one);
            Gizmos.DrawWireCube(spawnOffset, spawnSize);
            Gizmos.DrawRay(spawnOffset, Vector3.up * 1.5f);
        }
    }
}