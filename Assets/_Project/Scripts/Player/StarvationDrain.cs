using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Sin hilo, Mino se queda sin relleno poco a poco (como el aire en Minecraft).
    /// Mientras el hilo este en 0, baja la vida a un ritmo constante; en cuanto recupera hilo, para.
    /// Es un componente aparte: ni PlayerHealth ni ThreadSupply saben de este desgaste.
    /// </summary>
    [RequireComponent(typeof(PlayerHealth), typeof(ThreadSupply))]
    public class StarvationDrain : MonoBehaviour
    {
        [SerializeField] private float healthLossPerSecond = 4f;   // 100 de relleno duran ~25 s
        [SerializeField] private float healthLossWhileSwinging = 8f; // sin hilo y colgado en el aire
        [SerializeField] private float leakEffectInterval = 0.8f;    // cada cuantos segundos sale el VFX mientras pierde relleno

        private PlayerHealth health;
        private ThreadSupply thread;
        private ISwingState swingState;   // opcional: si no hay balanceo, siempre se usa el ritmo normal
        private float leakTimer;
        private float accumulated;   // la vida es entera: se acumulan decimales hasta llegar a 1

        private void Awake()
        {
            health = GetComponent<PlayerHealth>();
            thread = GetComponent<ThreadSupply>();
            swingState = GetComponent<ISwingState>();
        }

        private void Update()
        {
            if (!health.IsAlive || !thread.IsEmpty)
            {
                accumulated = 0f;
                leakTimer = leakEffectInterval;   // al empezar a perder relleno, el primer efecto sale enseguida
                return;
            }

            // Aviso periodico para el VFX (no en cada punto: serian 4 u 8 efectos por segundo).
            leakTimer += Time.deltaTime;
            if (leakTimer >= leakEffectInterval)
            {
                leakTimer = 0f;
                GameEvents.RaiseStuffingLeaked();
            }

            bool swinging = swingState != null && swingState.IsSwinging;
            float rate = swinging ? healthLossWhileSwinging : healthLossPerSecond;

            accumulated += rate * Time.deltaTime;
            int whole = Mathf.FloorToInt(accumulated);
            if (whole <= 0) return;

            accumulated -= whole;
            health.ApplyStarvation(whole);
        }
    }
}