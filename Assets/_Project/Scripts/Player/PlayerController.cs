using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Orquestador del jugador. No sabe que habilidades existen: solo recorre una lista de IAbility.
    /// Tampoco sabe de teclados: pide el input a un IInputProvider.
    /// Mueve horizontalmente y expone propiedades para que las habilidades modifiquen el movimiento
    /// sin tocar esta clase:
    ///  - SpeedMultiplier: velocidad relativa (agacharse la baja).
    ///  - MovementLocked: Mino no se mueve (escondido).
    ///  - ExternalMovement: otra habilidad controla la velocidad (balanceo con hilo).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        /// <summary>1 = normal. Agacharse puede bajarlo (ej. 0.5).</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        /// <summary>True = el jugador no se mueve horizontalmente (ej. escondido).</summary>
        public bool MovementLocked { get; set; }

        /// <summary>True = otra habilidad controla la velocidad (ej. balanceo): Move() no la toca.</summary>
        public bool ExternalMovement { get; set; }

        /// <summary>True mientras un empujon (knockback) manda sobre el movimiento.</summary>
        public bool IsMovementLockedByKnockback => Time.time < knockbackUntil;

        private float knockbackUntil;
        private Rigidbody2D body;
        private IInputProvider input;
        private IAbility[] abilities;
        private bool isDead;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            input = GetComponent<IInputProvider>();      // GetComponent funciona con interfaces
            abilities = GetComponents<IAbility>();

            if (input == null)
                Debug.LogError("PlayerController necesita un componente que implemente IInputProvider (ej. PlayerInput).", this);
        }

        private void OnEnable() => GameEvents.OnPlayerDied += HandlePlayerDied;
        private void OnDisable() => GameEvents.OnPlayerDied -= HandlePlayerDied;

        private void Update()
        {
            if (isDead || input == null) return;

            foreach (IAbility ability in abilities)
            {
                // Una habilidad desactivada (ej. mientras Mino esta escondido) no recibe input.
                if (ability is Behaviour behaviour && !behaviour.enabled) continue;
                ability.HandleInput(input);
            }
        }

        private void FixedUpdate()
        {
            if (isDead || input == null) return;

            Move();

            foreach (IAbility ability in abilities)
            {
                if (ability is Behaviour behaviour && !behaviour.enabled) continue;
                ability.FixedTick();
            }
        }

        /// <summary>Bloquea el movimiento horizontal unos segundos (lo usa PlayerKnockback).</summary>
        public void LockMovement(float seconds)
        {
            knockbackUntil = Mathf.Max(knockbackUntil, Time.time + seconds);
        }

        private void Move()
        {
            // Durante el empujon no se pisa la velocidad que dejo el golpe.
            if (IsMovementLockedByKnockback) return;

            // Mientras otra habilidad manda (balanceo), no se toca la velocidad.
            if (ExternalMovement) return;

            float horizontal = MovementLocked ? 0f : input.Horizontal;

            Vector2 velocity = body.linearVelocity;
            velocity.x = horizontal * moveSpeed * SpeedMultiplier;
            body.linearVelocity = velocity;

            if (Mathf.Abs(horizontal) > 0.01f)
                Flip(horizontal);
        }

        private void Flip(float direction)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction);
            transform.localScale = scale;
        }

        private void HandlePlayerDied()
        {
            isDead = true;

            foreach (IAbility ability in abilities)
                ability.Cancel();

            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
        }
    }
}