using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Esconderse. Con la tecla de interactuar busca el IHideable mas cercano; si lo hay,
    /// Mino entra. Con la misma tecla sale. Trabaja contra la interfaz IHideable, por eso
    /// cualquier escondite nuevo funciona sin tocar esta clase (O de SOLID).
    ///
    /// Mientras esta escondido: no se mueve, el resto de habilidades se desactiva, su fisica se apaga
    /// (las amenazas no lo golpean) y se publica OnPlayerHidden para que el boss pueda reaccionar.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class HideAbility : AbilityBase
    {
        [SerializeField] private float searchRadius = 1.2f;
        [SerializeField] private LayerMask hideableLayers;               // capa "Hideable"
        [SerializeField, Range(0f, 1f)] private float hiddenAlpha = 0.4f;

        private PlayerController controller;
        private SpriteRenderer[] sprites;
        private AbilityBase[] allAbilities;
        private IHideable current;
        private Transform originalParent;

        protected override void Awake()
        {
            base.Awake();
            controller = GetComponent<PlayerController>();
            sprites = GetComponentsInChildren<SpriteRenderer>();
            allAbilities = GetComponents<AbilityBase>();
        }

        public override void HandleInput(IInputProvider input)
        {
            if (!input.InteractPressed) return;

            if (IsActive) Exit();
            else TryEnter();
        }

        private void TryEnter()
        {
            IHideable target = FindClosestHideable();
            if (target == null) return;

            current = target;
            current.Enter(gameObject);
            IsActive = true;

            // Mientras esta escondido, el resto de habilidades se corta y se desactiva.
            foreach (AbilityBase ability in allAbilities)
            {
                if (ability == this) continue;
                ability.Cancel();
                ability.enabled = false;
            }

            controller.MovementLocked = true;
            Body.linearVelocity = Vector2.zero;
            Body.simulated = false; // sin fisica no hay colisiones ni dano de amenazas

            originalParent = transform.parent;
            transform.SetParent(target.HidePoint, true); // asi lo lleva la oveja
            Vector3 hidePosition = target.HidePoint.position;
            transform.position = new Vector3(hidePosition.x, hidePosition.y, transform.position.z);

            SetAlpha(hiddenAlpha);
            GameEvents.RaisePlayerHidden(true);
        }

        private void Exit()
        {
            if (current == null)
            {
                IsActive = false;
                return;
            }

            transform.SetParent(originalParent, true);
            Body.simulated = true;
            controller.MovementLocked = false;

            foreach (AbilityBase ability in allAbilities)
            {
                if (ability == this) continue;
                ability.enabled = true;
            }

            current.Exit(gameObject);
            current = null;
            IsActive = false;

            SetAlpha(1f);
            GameEvents.RaisePlayerHidden(false);
        }

        public override void Cancel()
        {
            if (IsActive) Exit();
            base.Cancel();
        }

        private IHideable FindClosestHideable()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(SearchCenter, searchRadius, hideableLayers);

            IHideable best = null;
            float bestDistance = float.MaxValue;

            foreach (Collider2D hit in hits)
            {
                IHideable candidate = hit.GetComponentInParent<IHideable>();
                if (candidate == null || !candidate.CanHide(gameObject)) continue;

                float distance = ((Vector2)hit.transform.position - SearchCenter).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            return best;
        }

        private void SetAlpha(float alpha)
        {
            foreach (SpriteRenderer sprite in sprites)
            {
                Color color = sprite.color;
                color.a = alpha;
                sprite.color = color;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(SearchCenter, searchRadius);
        }

        private Vector2 SearchCenter
        {
            get
            {
                Collider2D body = GetComponent<Collider2D>();
                return body != null ? (Vector2)body.bounds.center : (Vector2)transform.position;
            }
        }
    }
}
