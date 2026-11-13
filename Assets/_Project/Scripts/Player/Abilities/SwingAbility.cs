using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Balancearse con hilo. Flujo:
    ///   1) Clic y mantener (Aiming): el arrastre se mide desde el centro de Mino hasta el puntero.
    ///      El alfiler saldra al lado contrario, con mas velocidad cuanto mas lejos (hasta la cupula).
    ///   2) Soltar (PinFlying): si el arrastre fue suficiente, gasta hilo y sale el PinProjectile.
    ///      Soltar con el puntero casi sobre Mino no lanza nada y no gasta hilo.
    ///   3) Si el alfiler se clava en una superficie blanda (Attached), se crea un DistanceJoint2D
    ///      (solo distancia maxima: el hilo se afloja pero no se estira) y Mino cuelga.
    ///   4) Colgado: A/D balancea, W recoge el hilo (sube) y S lo suelta (baja, hasta el largo original).
    ///      En el aire gasta hilo cada segundo.
    ///   5) Soltar el hilo: clic o saltar. Si el hilo llega a 0 en pleno balanceo, Mino muere.
    ///
    /// Depende de interfaces (IAimInput, IKillable), no de clases concretas (D de SOLID).
    /// </summary>
    [RequireComponent(typeof(PlayerController), typeof(ThreadSupply))]
    public class SwingAbility : AbilityBase
    {
        private enum State { Idle, Aiming, PinFlying, Attached }

        [Header("Hilo")]
        [SerializeField] private PinProjectile pinPrefab;
        [SerializeField] private float throwThreadCost = 10f;   // hilo que cuesta lanzar un alfiler
        [SerializeField] private float drainPerSecond = 5f;     // desgaste mientras cuelga en el aire

        [Header("Apuntado tipo honda")]
        [SerializeField] private float maxDragDistance = 1.5f;  // radio de la cupula, en unidades del mundo
        [SerializeField, Range(0f, 0.5f)] private float minPowerToThrow = 0.1f; // menos que esto = no lanza
        [SerializeField] private float minLaunchSpeed = 6f;
        [SerializeField] private float maxLaunchSpeed = 22f;

        [Header("Balanceo y cuerda")]
        [SerializeField] private float swingForce = 18f;        // fuerza lateral con A/D
        [SerializeField] private float ropeClimbSpeed = 3f;     // unidades por segundo al recoger (W) o soltar (S)
        [SerializeField] private float minRopeLength = 1f;      // el hilo no se acorta mas que esto

        [Header("Visual (opcional)")]
        [SerializeField] private LineRenderer threadLine;       // hilo hacia el alfiler
        [SerializeField] private LineRenderer aimLine;          // linea de arrastre: de Mino hacia el puntero
        [SerializeField] private Transform rangeIndicator;      // la cupula: objeto FUERA de Mino, se muestra al apuntar

        private PlayerController controller;
        private ThreadSupply thread;
        private IAimInput aimInput;
        private IKillable killable;
        private Collider2D bodyCollider;
        private Camera cam;
        private SlingshotAim slingshot;

        private State state = State.Idle;
        private PinProjectile pin;
        private DistanceJoint2D joint;
        private float horizontalInput;
        private float verticalInput;
        private float attachLength; // largo del hilo al clavarse: es lo maximo que se puede soltar

        /// <summary>Colgado en el aire (util para el PlayerAnimator).</summary>
        public bool IsSwinging => state == State.Attached && !Ground.IsGrounded;

        /// <summary>Apuntando con el clic apretado (util para el PlayerAnimator).</summary>
        public bool IsAiming => state == State.Aiming;

        private Vector2 BodyCenter =>
            bodyCollider != null ? (Vector2)bodyCollider.bounds.center : (Vector2)transform.position;

        protected override void Awake()
        {
            base.Awake();
            controller = GetComponent<PlayerController>();
            thread = GetComponent<ThreadSupply>();
            aimInput = GetComponent<IAimInput>();
            killable = GetComponent<IKillable>();
            bodyCollider = GetComponent<Collider2D>();
            cam = Camera.main;
            slingshot = new SlingshotAim(maxDragDistance);

            if (aimInput == null)
                Debug.LogError("SwingAbility necesita un componente que implemente IAimInput (ej. PlayerInput).", this);

            PrepareLine(threadLine);
            PrepareLine(aimLine);

            if (rangeIndicator != null)
                rangeIndicator.gameObject.SetActive(false);
        }

        public override void HandleInput(IInputProvider input)
        {
            horizontalInput = input.Horizontal;
            verticalInput = input.Vertical;
            if (aimInput == null) return;

            switch (state)
            {
                case State.Idle:
                    // Solo empieza a apuntar si le alcanza el hilo para lanzar.
                    if (aimInput.AimPressed && thread.Current >= throwThreadCost)
                        BeginAim();
                    break;

                case State.Aiming:
                    UpdateAim();
                    break;

                case State.Attached:
                    if (aimInput.AimPressed || input.JumpPressed)
                        Detach();
                    break;
            }
        }

        public override void FixedTick()
        {
            if (state != State.Attached) return;

            AdjustRopeLength();

            bool airborne = !Ground.IsGrounded;

            // En el aire la velocidad la manda el joint; en el suelo Mino camina normal con el hilo flojo.
            controller.ExternalMovement = airborne;
            if (!airborne) return;

            Body.AddForce(Vector2.right * horizontalInput * swingForce);
            thread.Drain(drainPerSecond * Time.fixedDeltaTime);

            if (thread.IsEmpty)
            {
                Detach();
                killable?.Kill();
            }
        }

        public override void Cancel()
        {
            if (state == State.Aiming) EndAim();
            Detach();
            base.Cancel();
        }

        private void LateUpdate()
        {
            UpdateThreadLine();
            UpdateAimVisuals();
        }

        // ---------- Apuntado ----------
        private void BeginAim()
        {
            slingshot.Begin();
            state = State.Aiming;
            IsActive = true;
        }

        private void UpdateAim()
        {
            slingshot.UpdateAim(BodyCenter, PointerWorldPosition());
            GameEvents.RaiseAimChanged(true, slingshot.Power01);

            if (aimInput.AimReleased) ReleaseAim();
            else if (!aimInput.AimHeld) EndAim(); // se perdio el boton (ej. se cambio de ventana)
        }

        private void ReleaseAim()
        {
            float power = slingshot.Power01;
            Vector2 direction = slingshot.LaunchDirection;
            EndAim();

            // Puntero casi sobre Mino: no hay arrastre, no se lanza nada y no se gasta hilo.
            if (power < minPowerToThrow || direction == Vector2.zero) return;
            if (!thread.TryConsume(throwThreadCost)) return;

            float speed = Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, power);
            Throw(direction, speed);
        }

        private void EndAim()
        {
            slingshot.End();
            GameEvents.RaiseAimChanged(false, 0f);

            if (state == State.Aiming)
            {
                state = State.Idle;
                IsActive = false;
            }
        }

        /// <summary>Posicion del puntero en el mundo 2D.</summary>
        private Vector2 PointerWorldPosition()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return BodyCenter; // sin camara no hay arrastre

            Vector2 screen = aimInput.PointerScreenPosition;
            Vector3 world = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Mathf.Abs(cam.transform.position.z)));
            return world;
        }

        // ---------- Alfiler y joint ----------
        private void Throw(Vector2 direction, float speed)
        {
            if (pinPrefab == null) return;

            pin = Instantiate(pinPrefab, BodyCenter, Quaternion.identity);
            pin.Embedded += HandlePinEmbedded;
            pin.Failed += HandlePinFailed;
            pin.Launch(direction, speed, bodyCollider);

            state = State.PinFlying;
            IsActive = true;
        }

        private void HandlePinEmbedded(PinProjectile embeddedPin)
        {
            if (state != State.PinFlying || embeddedPin != pin) return;

            float distance = Vector2.Distance(BodyCenter, pin.transform.position);
            attachLength = distance;

            joint = gameObject.AddComponent<DistanceJoint2D>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedBody = pin.Body;
            joint.connectedAnchor = Vector2.zero;                       // el origen del alfiler
            joint.anchor = transform.InverseTransformPoint(BodyCenter); // el pecho de Mino
            joint.autoConfigureDistance = false;
            joint.distance = distance;
            joint.maxDistanceOnly = true;                               // hilo: afloja, pero no se estira

            state = State.Attached;
            controller.ExternalMovement = !Ground.IsGrounded;
        }

        private void HandlePinFailed(PinProjectile failedPin)
        {
            if (failedPin != pin) return;
            Detach();
        }

        /// <summary>
        /// W recoge el hilo (Mino sube hacia el alfiler) y S lo suelta (hasta el largo original).
        /// Al recoger parte de la distancia REAL, asi si el hilo estaba flojo empieza a tirar de inmediato.
        /// </summary>
        private void AdjustRopeLength()
        {
            if (joint == null || pin == null) return;
            if (Mathf.Abs(verticalInput) < 0.1f) return;

            float current = joint.distance;
            float target;

            if (verticalInput > 0f)
            {
                float actual = Vector2.Distance(BodyCenter, pin.transform.position);
                target = Mathf.Min(current, actual) - ropeClimbSpeed * verticalInput * Time.fixedDeltaTime;
            }
            else
            {
                target = current - ropeClimbSpeed * verticalInput * Time.fixedDeltaTime; // vertical es negativo: suma
            }

            float min = Mathf.Min(minRopeLength, attachLength);
            joint.distance = Mathf.Clamp(target, min, attachLength);
        }

        /// <summary>Suelta el hilo: quita el joint y el alfiler. Se puede llamar varias veces sin problema.</summary>
        private void Detach()
        {
            if (state == State.Idle || state == State.Aiming) return;

            if (joint != null) Destroy(joint);

            if (pin != null)
            {
                pin.Embedded -= HandlePinEmbedded;
                pin.Failed -= HandlePinFailed;
                pin.Release();
            }

            joint = null;
            pin = null;
            state = State.Idle;
            IsActive = false;
            controller.ExternalMovement = false;
        }

        // ---------- Visual ----------
        private static void PrepareLine(LineRenderer line)
        {
            if (line == null) return;
            line.positionCount = 2;
            line.enabled = false;
        }

        private void UpdateThreadLine()
        {
            if (threadLine == null) return;

            bool show = (state == State.PinFlying || state == State.Attached) && pin != null;
            threadLine.enabled = show;

            if (show)
            {
                threadLine.SetPosition(0, BodyCenter);
                threadLine.SetPosition(1, pin.transform.position);
            }
        }

        private void UpdateAimVisuals()
        {
            bool aiming = state == State.Aiming;

            if (rangeIndicator != null)
            {
                rangeIndicator.gameObject.SetActive(aiming);

                if (aiming)
                {
                    rangeIndicator.position = BodyCenter;
                    float diameter = maxDragDistance * 2f;
                    rangeIndicator.localScale = new Vector3(diameter, diameter, 1f);
                }
            }

            if (aimLine != null)
            {
                bool showLine = aiming && slingshot.Power01 > 0f;
                aimLine.enabled = showLine;

                if (showLine)
                {
                    Vector2 start = BodyCenter;
                    aimLine.SetPosition(0, start);
                    aimLine.SetPosition(1, start + slingshot.DragOffset);
                }
            }
        }
    }
}