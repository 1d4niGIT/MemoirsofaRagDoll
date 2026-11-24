using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Sacudida visual al recibir dano estando quieto: el cuerpo se va un poco a la IZQUIERDA,
    /// luego a la derecha, y se calma (oscilacion amortiguada). Mueve solo el hijo "Visual",
    /// nunca el Rigidbody ni el collider, asi no afecta a la fisica.
    ///
    /// Va en el objeto "Visual" (el hijo con el SpriteRenderer) de Mino.
    /// </summary>
    public class HitShake : MonoBehaviour, IHitReactable
    {
        [SerializeField] private float amplitude = 0.12f;   // desplazamiento maximo (unidades de mundo)
        [SerializeField] private float duration = 0.25f;    // lo que dura la reaccion
        [SerializeField] private float swings = 1.5f;       // ciclos completos: izquierda -> derecha -> ...

        private Vector3 baseLocalPosition;
        private float startTime = -999f;
        private bool playing;

        private void Awake() => baseLocalPosition = transform.localPosition;

        public void ReactToHit()
        {
            startTime = Time.time;
            playing = true;
        }

        // LateUpdate: se aplica despues del Animator, asi el offset no se pisa.
        private void LateUpdate()
        {
            if (!playing) return;

            float t = (Time.time - startTime) / duration;
            if (t >= 1f)
            {
                playing = false;
                transform.localPosition = baseLocalPosition;
                return;
            }

            // -sin => empieza hacia la izquierda; (1 - t) => se va apagando.
            float offset = -Mathf.Sin(t * swings * 2f * Mathf.PI) * (1f - t) * amplitude;

            // Si Mino esta volteado (escala X negativa), se corrige para que "izquierda" sea la del mundo.
            float facing = Mathf.Sign(transform.parent != null ? transform.parent.lossyScale.x : 1f);
            transform.localPosition = baseLocalPosition + new Vector3(offset * facing, 0f, 0f);
        }
    }
}
