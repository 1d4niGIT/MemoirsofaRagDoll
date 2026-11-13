using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// El hilo de Mino: un recurso limitado que se gasta al lanzar alfileres y al balancearse,
    /// y se recupera con ovillos de lana o madejas de pabilo.
    /// Solo gestiona la cantidad y publica OnThreadChanged: no sabe quien lo gasta
    /// ni quien lo muestra (S de SOLID).
    /// </summary>
    public class ThreadSupply : MonoBehaviour
    {
        [SerializeField] private float maxThread = 100f;
        [SerializeField] private float startThread = 100f;

        public float Current { get; private set; }
        public float Max => maxThread;
        public bool IsEmpty => Current <= 0f;

        private void Awake()
        {
            Current = Mathf.Clamp(startThread, 0f, maxThread);
        }

        private void Start()
        {
            // Valor inicial para que la interfaz arranque bien.
            GameEvents.RaiseThreadChanged(Current, maxThread);
        }

        /// <summary>Gasta hilo SOLO si alcanza. Sirve para pagar el lanzamiento de un alfiler.</summary>
        public bool TryConsume(float amount)
        {
            if (amount <= 0f) return true;
            if (Current < amount) return false;

            SetCurrent(Current - amount);
            return true;
        }

        /// <summary>Gasta hilo aunque llegue a 0. Sirve para el desgaste continuo al balancearse.</summary>
        public void Drain(float amount)
        {
            if (amount <= 0f) return;
            SetCurrent(Mathf.Max(0f, Current - amount));
        }

        /// <summary>Recupera hilo. Devuelve false si ya estaba lleno (el ovillo no se gasta).</summary>
        public bool Add(float amount)
        {
            if (amount <= 0f || Current >= maxThread) return false;

            SetCurrent(Mathf.Min(maxThread, Current + amount));
            return true;
        }

        private void SetCurrent(float value)
        {
            if (Mathf.Approximately(value, Current)) return;

            Current = value;
            GameEvents.RaiseThreadChanged(Current, maxThread);
        }
    }
}
