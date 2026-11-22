using UnityEngine;
using MoreMountains.Feedbacks;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Conecta los eventos de perdida de RELLENO con MMF_Players de Feel:
    ///  - un golpe le quita relleno (OnStuffingHit)
    ///  - va perdiendo relleno por no tener hilo (OnStuffingLeaked, cada cierto intervalo)
    /// Aqui no hay efectos: solo escucha y dispara. Los efectos (VFX, freeze frame, sacudida...)
    /// se arman en el Inspector del MMF_Player, sin tocar codigo.
    ///
    /// Va en Mino. Arrastra su MMF_Player (ej. el hijo "Feedback_StuffingHit") al campo.
    /// </summary>
    public class PlayerStuffingFeedback : MonoBehaviour
    {
        [SerializeField] private MMF_Player feedback;       // efecto del golpe
        [SerializeField] private MMF_Player leakFeedback;   // efecto de perdida gradual (opcional: si esta vacio usa el mismo del golpe)
        [SerializeField] private bool randomPointOnBody = true;   // el efecto aparece en un punto al azar del collider de Mino

        private Collider2D body;

        private void Awake() => body = GetComponent<Collider2D>();

        private void OnEnable()
        {
            GameEvents.OnStuffingHit += HandleStuffingHit;
            GameEvents.OnStuffingLeaked += HandleStuffingLeaked;
        }

        private void OnDisable()
        {
            GameEvents.OnStuffingHit -= HandleStuffingHit;
            GameEvents.OnStuffingLeaked -= HandleStuffingLeaked;
        }

        private void HandleStuffingHit(int amount) => Play(feedback);

        private void HandleStuffingLeaked() => Play(leakFeedback != null ? leakFeedback : feedback);

        private void Play(MMF_Player player)
        {
            if (player == null) return;

            // Los feedbacks con "Position Mode: Script" aparecen en este punto.
            player.PlayFeedbacks(SpawnPoint(), 1f);
        }

        /// <summary>Un punto al azar del collider de Mino (o su posicion si no se usa el azar).</summary>
        private Vector3 SpawnPoint()
        {
            if (!randomPointOnBody || body == null) return transform.position;

            Bounds b = body.bounds;
            Vector2 p = new Vector2(Random.Range(b.min.x, b.max.x), Random.Range(b.min.y, b.max.y));
            return body.ClosestPoint(p);   // si cae fuera de la forma, lo acerca al borde
        }
    }
}