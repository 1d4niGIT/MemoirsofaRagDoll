using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Agacharse. Mientras se mantiene el boton y Mino esta en el suelo:
    /// - su collider se hace mas bajo (manteniendo los pies en su sitio),
    /// - se mueve mas lento.
    /// Al soltar, solo se levanta si no hay un techo encima; si lo hay, sigue agachado.
    /// Funciona con BoxCollider2D o CapsuleCollider2D.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class CrouchAbility : AbilityBase
    {
        [SerializeField, Range(0.3f, 0.9f)] private float heightMultiplier = 0.5f;
        [SerializeField, Range(0.1f, 1f)] private float speedMultiplier = 0.5f;
        [SerializeField] private LayerMask ceilingLayers; // normalmente solo "Ground"

        private PlayerController controller;
        private BoxCollider2D box;
        private CapsuleCollider2D capsule;
        private Vector2 standingSize;
        private Vector2 standingOffset;

        protected override void Awake()
        {
            base.Awake();
            controller = GetComponent<PlayerController>();
            box = GetComponent<BoxCollider2D>();
            capsule = GetComponent<CapsuleCollider2D>();

            if (box != null)
            {
                standingSize = box.size;
                standingOffset = box.offset;
            }
            else if (capsule != null)
            {
                standingSize = capsule.size;
                standingOffset = capsule.offset;
            }
            else
            {
                Debug.LogError("CrouchAbility necesita un BoxCollider2D o CapsuleCollider2D en el jugador.", this);
            }
        }

        public override void HandleInput(IInputProvider input)
        {
            bool wantsCrouch = input.CrouchHeld && Ground.IsGrounded;

            if (!IsActive)
            {
                if (wantsCrouch) Begin();
                return;
            }

            // Ya agachado: se levanta solo si ya no quiere agacharse y hay espacio arriba.
            if (!wantsCrouch && CanStandUp())
                End();
        }

        public override void Cancel()
        {
            if (IsActive) End();
            base.Cancel();
        }

        private void Begin()
        {
            IsActive = true;
            SetCrouched(true);
            controller.SpeedMultiplier = speedMultiplier;
        }

        private void End()
        {
            IsActive = false;
            SetCrouched(false);
            controller.SpeedMultiplier = 1f;
        }

        /// <summary>Cambia la altura del collider sin mover el borde inferior (los pies).</summary>
        private void SetCrouched(bool crouched)
        {
            float height = crouched ? standingSize.y * heightMultiplier : standingSize.y;
            float bottom = standingOffset.y - standingSize.y * 0.5f;
            Vector2 newSize = new Vector2(standingSize.x, height);
            Vector2 newOffset = new Vector2(standingOffset.x, bottom + height * 0.5f);

            if (box != null)
            {
                box.size = newSize;
                box.offset = newOffset;
            }
            else if (capsule != null)
            {
                capsule.size = newSize;
                capsule.offset = newOffset;
            }
        }

        /// <summary>Comprueba si hay algo encima que impediria ponerse de pie.</summary>
        private bool CanStandUp()
        {
            Collider2D current = box != null ? (Collider2D)box : capsule;
            if (current == null) return true;

            Bounds bounds = current.bounds;
            float extraHeight = standingSize.y * (1f - heightMultiplier) * Mathf.Abs(transform.lossyScale.y);

            Vector2 origin = new Vector2(bounds.center.x, bounds.max.y);
            Vector2 size = new Vector2(bounds.size.x * 0.9f, 0.02f);

            return Physics2D.BoxCast(origin, size, 0f, Vector2.up, extraHeight, ceilingLayers).collider == null;
        }
    }
}
