using System.Collections.Generic;
using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Traduce el estado del jugador a parametros del Animator. Va en el objeto raiz de Mino
    /// (el Animator puede estar en el hijo "Visual"). No decide nada del gameplay: solo LEE
    /// el estado y lo muestra (S de SOLID). Por eso las animaciones se pueden cambiar o ampliar
    /// sin tocar fisicas, vida ni habilidades.
    ///
    /// Es tolerante: si un parametro todavia no existe en tu Animator Controller, lo ignora.
    /// Puedes ir creando parametros y estados poco a poco.
    ///
    /// Parametros que reconoce (nombre exacto, distingue mayusculas):
    ///   Speed (float)         velocidad horizontal, siempre positiva
    ///   VerticalSpeed (float) positiva al subir, negativa al caer
    ///   IsGrounded (bool)     tocando el suelo
    ///   IsCrouching (bool)    agachado
    ///   IsGliding (bool)      planeando
    ///   IsHidden (bool)       escondido
    ///   Hurt (trigger)        recibio daño
    ///   Died (trigger)        murio
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator; // si esta vacio, lo busca en los hijos

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
        private static readonly int IsGlidingHash = Animator.StringToHash("IsGliding");
        private static readonly int IsHiddenHash = Animator.StringToHash("IsHidden");
        private static readonly int HurtHash = Animator.StringToHash("Hurt");
        private static readonly int DiedHash = Animator.StringToHash("Died");

        private readonly HashSet<int> availableParameters = new HashSet<int>();

        private Rigidbody2D body;
        private GroundSensor ground;
        private CrouchAbility crouch; // pueden ser null si el jugador no las tiene
        private GlideAbility glide;
        private bool isDead;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            ground = GetComponent<GroundSensor>();
            crouch = GetComponent<CrouchAbility>();
            glide = GetComponent<GlideAbility>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();

            CacheParameters();
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerDamaged += HandlePlayerDamaged;
            GameEvents.OnPlayerDied += HandlePlayerDied;
            GameEvents.OnPlayerHidden += HandlePlayerHidden;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDamaged -= HandlePlayerDamaged;
            GameEvents.OnPlayerDied -= HandlePlayerDied;
            GameEvents.OnPlayerHidden -= HandlePlayerHidden;
        }

        private void Update()
        {
            if (animator == null || isDead) return;

            Vector2 velocity = body.linearVelocity;

            SetFloat(SpeedHash, Mathf.Abs(velocity.x));
            SetFloat(VerticalSpeedHash, velocity.y);
            SetBool(IsGroundedHash, ground != null && ground.IsGrounded);
            SetBool(IsCrouchingHash, crouch != null && crouch.IsActive);
            SetBool(IsGlidingHash, glide != null && glide.IsActive);
        }

        // ---------- Eventos ----------
        private void HandlePlayerDamaged(int amount)
        {
            if (isDead) return;
            SetTrigger(HurtHash);
        }

        private void HandlePlayerDied()
        {
            isDead = true;
            ResetTrigger(HurtHash); // el daño letal publica "daño" y luego "muerte": se queda solo la muerte
            SetTrigger(DiedHash);
        }

        private void HandlePlayerHidden(bool isHidden)
        {
            SetBool(IsHiddenHash, isHidden);
        }

        // ---------- Acceso seguro al Animator ----------
        private void CacheParameters()
        {
            availableParameters.Clear();

            if (animator == null || animator.runtimeAnimatorController == null) return;

            foreach (AnimatorControllerParameter parameter in animator.parameters)
                availableParameters.Add(parameter.nameHash);
        }

        private void SetFloat(int hash, float value)
        {
            if (availableParameters.Contains(hash)) animator.SetFloat(hash, value);
        }

        private void SetBool(int hash, bool value)
        {
            if (availableParameters.Contains(hash)) animator.SetBool(hash, value);
        }

        private void SetTrigger(int hash)
        {
            if (availableParameters.Contains(hash)) animator.SetTrigger(hash);
        }

        private void ResetTrigger(int hash)
        {
            if (availableParameters.Contains(hash)) animator.ResetTrigger(hash);
        }
    }
}
