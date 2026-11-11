using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Orquestador del jugador. No sabe que habilidades existen: solo recorre una lista de IAbility.
    /// Tampoco sabe de teclados: pide el input a un IInputProvider.
    /// Mueve horizontalmente y expone SpeedMultiplier / MovementLocked para que las habilidades
    /// (agacharse, esconderse) puedan modificar el movimiento sin tocar esta clase.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        /// <summary>1 = normal. Agacharse puede bajarlo (ej. 0.5).</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        /// <summary>True = el jugador no se mueve horizontalmente (ej. escondido).</summary>
        public bool MovementLocked { get; set; }

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

        private void OnEnable()  => GameEvents.OnPlayerDied += HandlePlayerDied;
        private void OnDisable() => GameEvents.OnPlayerDied -= HandlePlayerDied;

        private void Update()
        {
            if (isDead || input == null) return;

            foreach (IAbility ability in abilities)
            {
                if (ability is Behaviour b && !b.enabled) continue;
                ability.HandleInput(input);
            }
        }

        private void FixedUpdate()
        {
            if (isDead || input == null) return;

            Move();

            foreach (IAbility ability in abilities)
            {
                if (ability is Behaviour b && !b.enabled) continue;
                ability.FixedTick();
            }
        }

        private void Move()
        {
            float horizontal = MovementLocked ? 0f : input.Horizontal;

            // Unity 6: Rigidbody2D.velocity pasa a llamarse linearVelocity.
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
