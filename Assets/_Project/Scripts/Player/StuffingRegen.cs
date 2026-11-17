using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Recupera el relleno (la vida) de Mino con el tiempo:
    ///  - Con hilo mayor a 0: se regenera poco a poco.
    ///  - Escondido en una oveja: se regenera mas rapido, y SOLO el relleno (el hilo no se recupera).
    /// Se entera de que se esconde por el evento OnPlayerHidden, sin conocer a HideAbility.
    /// Es un componente aparte: PlayerHealth solo guarda el numero y esta clase decide cuando curar.
    /// </summary>
    [RequireComponent(typeof(PlayerHealth), typeof(ThreadSupply))]
    public class StuffingRegen : MonoBehaviour
    {
        [SerializeField] private float regenWithThread = 2f;   // relleno por segundo si hay hilo
        [SerializeField] private float regenHidden = 8f;       // relleno por segundo escondido en una oveja

        private PlayerHealth health;
        private ThreadSupply thread;
        private bool isHidden;
        private float accumulated;   // la vida es entera: se acumulan decimales hasta llegar a 1

        private void Awake()
        {
            health = GetComponent<PlayerHealth>();
            thread = GetComponent<ThreadSupply>();
        }

        private void OnEnable()  => GameEvents.OnPlayerHidden += HandleHidden;
        private void OnDisable() => GameEvents.OnPlayerHidden -= HandleHidden;

        private void HandleHidden(bool hidden) => isHidden = hidden;

        private void Update()
        {
            float rate = isHidden ? regenHidden : (thread.IsEmpty ? 0f : regenWithThread);

            if (!health.IsAlive || rate <= 0f || health.CurrentHealth >= health.MaxHealth)
            {
                accumulated = 0f;
                return;
            }

            accumulated += rate * Time.deltaTime;
            int whole = Mathf.FloorToInt(accumulated);
            if (whole <= 0) return;

            accumulated -= whole;
            health.Heal(whole);
        }
    }
}