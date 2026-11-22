using System.Collections.Generic;
using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Efecto visual: al herir a Mino, de la zona del choque salen hilos (LineRenderers) hacia
    /// puntos al azar del collider de Mino, porque un muneco de trapo se engancha en espinas,
    /// puas o tijeras. Cada hilo dura entre 1 y 2 segundos y se desvanece.
    ///
    ///  - Un extremo queda fijo donde engancho la amenaza; el otro va pegado a Mino. Si Mino sale
    ///    empujado, el hilo se estira y se curva de verdad.
    ///  - La cantidad de hilos la decide cada amenaza (snagLines en KnockbackHazard).
    ///  - Solo se dibuja cuando el golpe le GASTA HILO. Sin hilo (el golpe baja relleno) no salen.
    ///    Tampoco durante la invulnerabilidad, porque ahi el golpe no hace nada.
    ///  - Los LineRenderers se reutilizan (pool): no se crean ni destruyen en pleno juego.
    ///
    /// Va en Mino. Solo escucha eventos: ni la vida ni las amenazas saben que existe.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ThreadSnagVFX : MonoBehaviour
    {
        [Header("Aspecto")]
        [SerializeField] private Material lineMaterial;          // el mismo de tu hilo (Sprite-Unlit-Default)
        [SerializeField] private Color color = Color.white;
        [SerializeField] private float width = 0.04f;
        [SerializeField] private LineTextureMode textureMode = LineTextureMode.Tile;   // Tile = la imagen se repite; Stretch = se estira
        [SerializeField] private Vector2 textureScale = Vector2.one;                  // X: cuanto se repite a lo largo (mayor = mas repeticiones)
        [SerializeField] private string sortingLayer = "";       // vacio = no se cambia
        [SerializeField] private int sortingOrder = 10;

        [Header("Duracion")]
        [SerializeField] private float minLifetime = 1f;
        [SerializeField] private float maxLifetime = 2f;
        [SerializeField, Range(0.1f, 1f)] private float fadeFraction = 0.4f;   // ultima parte de la vida en que se desvanece

        [Header("Forma")]
        [SerializeField] private float startSpread = 0.15f;      // dispersion del punto de salida alrededor del choque
        [SerializeField] private float maxBend = 0.25f;          // cuanto se curva el hilo
        [SerializeField, Range(2, 16)] private int segments = 8;

        private class SnagThread
        {
            public LineRenderer line;
            public Vector2 start;      // fijo en el mundo: donde engancho
            public Vector2 localEnd;   // pegado a Mino (espacio local, asi acompana su movimiento y su volteo)
            public float bend;         // curvatura con signo
            public float bornAt;
            public float life;
        }

        private readonly List<SnagThread> active = new List<SnagThread>();
        private readonly Stack<SnagThread> free = new Stack<SnagThread>();

        private Collider2D body;
        private Transform container;
        private int lastThreadHitFrame = -1;

        private void Awake()
        {
            body = GetComponent<Collider2D>();
            container = new GameObject("SnagThreads (pool)").transform;

            if (lineMaterial == null)
                Debug.LogWarning("ThreadSnagVFX necesita un Material para las lineas (usa el de tu hilo).", this);
        }

        private void OnEnable()
        {
            GameEvents.OnThreadHit += HandleThreadHit;
            GameEvents.OnSnagged += HandleSnagged;
        }

        private void OnDisable()
        {
            GameEvents.OnThreadHit -= HandleThreadHit;
            GameEvents.OnSnagged -= HandleSnagged;
        }

        private void OnDestroy()
        {
            if (container != null) Destroy(container.gameObject);
        }

        // PlayerHealth avisa que el golpe gasto hilo ANTES de que la amenaza publique el enganche,
        // en el mismo frame: asi sabemos que este golpe si perdio hilo.
        private void HandleThreadHit(int amount) => lastThreadHitFrame = Time.frameCount;

        private void HandleSnagged(Collider2D victim, Vector2 contactPoint, int lineCount)
        {
            if (lastThreadHitFrame != Time.frameCount) return;              // el golpe no gasto hilo: sin hilos
            if (victim == null || !victim.transform.IsChildOf(transform)) return;   // no era Mino

            for (int i = 0; i < lineCount; i++)
                Spawn(contactPoint);
        }

        private void Spawn(Vector2 contactPoint)
        {
            SnagThread t = free.Count > 0 ? free.Pop() : CreateThread();

            t.start = contactPoint + Random.insideUnitCircle * startSpread;
            t.localEnd = transform.InverseTransformPoint(RandomPointOnBody());
            t.bend = Random.Range(-maxBend, maxBend);
            t.bornAt = Time.time;
            t.life = Random.Range(minLifetime, maxLifetime);

            t.line.enabled = true;
            active.Add(t);
        }

        /// <summary>Punto al azar del collider de Mino: uno cualquiera de su caja, llevado al collider.</summary>
        private Vector2 RandomPointOnBody()
        {
            Bounds b = body.bounds;
            Vector2 p = new Vector2(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y));
            return body.ClosestPoint(p);   // si cae fuera de la forma, lo acerca al borde
        }

        private SnagThread CreateThread()
        {
            GameObject go = new GameObject("SnagThread");
            go.transform.SetParent(container, false);

            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.positionCount = segments + 1;
            lr.numCapVertices = 2;
            lr.textureMode = textureMode;
            lr.textureScale = textureScale;
            lr.material = lineMaterial;
            lr.sortingOrder = sortingOrder;
            if (!string.IsNullOrEmpty(sortingLayer)) lr.sortingLayerName = sortingLayer;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.enabled = false;

            return new SnagThread { line = lr };
        }

        private void LateUpdate()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                SnagThread t = active[i];
                float age = Time.time - t.bornAt;

                if (age >= t.life)
                {
                    t.line.enabled = false;
                    active.RemoveAt(i);
                    free.Push(t);
                    continue;
                }

                // Se desvanece (y se afina) en la ultima parte de su vida.
                float t01 = age / t.life;
                float fadeStart = 1f - fadeFraction;
                float alpha = t01 > fadeStart ? 1f - (t01 - fadeStart) / fadeFraction : 1f;

                Color c = color;
                c.a *= alpha;
                t.line.startColor = c;
                t.line.endColor = c;
                t.line.widthMultiplier = width * alpha;

                Draw(t);
            }
        }

        /// <summary>Curva de Bezier cuadratica entre el enganche (fijo) y el punto de Mino (que se mueve).</summary>
        private void Draw(SnagThread t)
        {
            Vector2 end = transform.TransformPoint(t.localEnd);
            Vector2 dir = end - t.start;
            Vector2 perp = new Vector2(-dir.y, dir.x).normalized;
            Vector2 control = (t.start + end) * 0.5f + perp * t.bend;

            for (int i = 0; i <= segments; i++)
            {
                float u = i / (float)segments;
                float inv = 1f - u;
                Vector2 p = inv * inv * t.start + 2f * inv * u * control + u * u * end;
                t.line.SetPosition(i, p);
            }
        }
    }
}