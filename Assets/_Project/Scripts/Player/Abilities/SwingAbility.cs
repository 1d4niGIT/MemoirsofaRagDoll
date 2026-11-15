using UnityEngine;
using RagDoll.Core;

namespace RagDoll.Player
{
    /// <summary>
    /// Balancearse con hilo. Flujo:
    ///   1) Clic y mantener: apuntas. El arrastre se mide desde el centro de Mino hasta el puntero;
    ///      el alfiler saldra al lado contrario, con mas velocidad cuanto mas lejos (hasta la cupula).
    ///      Una guia dibuja la parabola que seguira.
    ///   2) Soltar el clic: si el arrastre fue suficiente, gasta hilo segun la fuerza (ver ThrowCostFor) y sale el alfiler, que cae por la
    ///      gravedad. Soltar con el puntero casi sobre Mino no lanza nada ni gasta hilo.
    ///   3) Si el alfiler se clava en una superficie blanda, Mino cuelga de el con un DistanceJoint2D
    ///      (solo distancia maxima: el hilo se afloja pero no se estira).
    ///   4) Colgado: A/D balancea, W recoge el hilo y S lo suelta. En el aire gasta hilo cada segundo.
    ///   5) Se puede seguir lanzando alfileres COLGADO: el hilo actual se mantiene mientras apuntas y
    ///      mientras el nuevo alfiler vuela. Si el nuevo se clava, Mino cambia de anclaje (el anterior
    ///      se suelta); si falla, sigue colgado del anterior.
    ///   6) Espacio suelta el hilo. Si el hilo llega a 0 en pleno balanceo, Mino muere.
    ///
    /// Depende de interfaces (IAimInput, IKillable), no de clases concretas (D de SOLID).
    /// </summary>
    [RequireComponent(typeof(PlayerController), typeof(ThreadSupply))]
    public class SwingAbility : AbilityBase
    {
        [Header("Hilo")]
        [SerializeField] private PinProjectile pinPrefab;
        [SerializeField] private float maxThrowCost = 10f;      // hilo que cuesta lanzar con la barra llena
        [SerializeField] private float minThrowCost = 2.5f;     // costo minimo de cualquier lanzamiento
        [SerializeField] private float drainPerSecond = 5f;     // desgaste mientras cuelga en el aire

        [Header("Apuntado tipo honda")]
        [SerializeField] private float maxDragDistance = 1.5f;  // radio de la cupula, en unidades del mundo
        [SerializeField, Range(0f, 0.5f)] private float minPowerToThrow = 0.1f; // menos que esto = no lanza
        [SerializeField] private float minLaunchSpeed = 3f;     // velocidad con la fuerza minima que lanza
        [SerializeField] private float maxLaunchSpeed = 11f;    // velocidad con la fuerza maxima

        [Header("Balanceo y cuerda")]
        [SerializeField] private float swingForce = 18f;        // fuerza lateral con A/D
        [SerializeField] private float ropeClimbSpeed = 3f;     // unidades por segundo al recoger (W) o soltar (S)
        [SerializeField] private float minRopeLength = 1f;      // el hilo no se acorta mas que esto

        [Header("Visual (opcional)")]
        [SerializeField] private LineRenderer threadLine;       // hilo hacia el alfiler
        [SerializeField] private LineRenderer aimLine;          // linea de arrastre: de Mino hacia el puntero
        [SerializeField] private Transform rangeIndicator;      // la cupula: objeto FUERA de Mino, se muestra al apuntar

        [Header("Guia de trayectoria (opcional)")]
        [SerializeField] private LineRenderer trajectoryLine;   // parabola prevista del alfiler
        [SerializeField, Range(8, 40)] private int trajectoryPoints = 24;
        [SerializeField] private LayerMask trajectoryBlockers;  // Ground y Soft: la guia se corta al chocar

        private PlayerController controller;
        private ThreadSupply thread;
        private IAimInput aimInput;
        private IKillable killable;
        private Collider2D bodyCollider;
        private Camera cam;
        private SlingshotAim slingshot;

        // Tres cosas independientes que pueden ocurrir a la vez:
        private bool isAiming;              // clic apretado, apuntando
        private PinProjectile flyingPin;    // alfiler nuevo en vuelo
        private PinProjectile pin;          // alfiler en el que cuelga Mino
        private DistanceJoint2D joint;      // el hilo (uno solo; al cambiar de anclaje se reutiliza)

        private float horizontalInput;
        private float verticalInput;
        private float attachLength; // largo del hilo al clavarse: es lo maximo que se puede soltar

        // Guia de trayectoria: un solo arreglo reutilizado (sin crear basura) y cache del ultimo calculo.
        private Vector3[] trajectoryBuffer;
        private bool previewCached;
        private Vector2 lastPreviewOrigin;
        private Vector2 lastPreviewVelocity;

        private bool IsAttached => joint != null && pin != null;

        /// <summary>Colgado en el aire (util para el PlayerAnimator).</summary>
        public bool IsSwinging => IsAttached && !Ground.IsGrounded;

        /// <summary>Apuntando con el clic apretado (util para el PlayerAnimator).</summary>
        public bool IsAiming => isAiming;

        private Vector2 BodyCenter =>
            bodyCollider != null ? (Vector2)bodyCollider.bounds.center : (Vector2)transform.position;

        private Vector2 VisualCenter
        {
            get
            {
                if (bodyCollider == null) return transform.position;
                return transform.TransformPoint(bodyCollider.offset);
            }
        }

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
            trajectoryBuffer = new Vector3[trajectoryPoints];

            if (aimInput == null)
                Debug.LogError("SwingAbility necesita un componente que implemente IAimInput (ej. PlayerInput).", this);

            PrepareLine(threadLine);
            PrepareLine(aimLine);
            PrepareLine(trajectoryLine);

            if (rangeIndicator != null)
                rangeIndicator.gameObject.SetActive(false);
        }

        public override void HandleInput(IInputProvider input)
        {
            horizontalInput = input.Horizontal;
            verticalInput = input.Vertical;
            if (aimInput == null) return;

            if (isAiming)
            {
                UpdateAim();
            }
            else if (aimInput.AimPressed && flyingPin == null && thread.Current >= minThrowCost)
            {
                // Se puede apuntar tambien colgado. Solo un alfiler nuevo en vuelo a la vez.
                BeginAim();
            }

            // Espacio suelta el hilo en el que cuelga Mino.
            if (IsAttached && input.JumpPressed)
                Detach();
        }

        public override void FixedTick()
        {
            // Si la plataforma desaparecio y se llevo el alfiler, no queda de que colgar.
            if (joint != null && pin == null)
            {
                Detach();
                return;
            }

            if (!IsAttached) return;

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
            if (isAiming) EndAim();
            DiscardFlyingPin();
            Detach();
            HideAllVisuals();
            base.Cancel();
        }

        private void LateUpdate()
        {
            UpdateThreadLine();
            UpdateAimVisuals();
        }

        private void RefreshActive()
        {
            IsActive = isAiming || flyingPin != null || joint != null;
        }

        // ---------- Apuntado ----------
        private void BeginAim()
        {
            slingshot.Begin();
            isAiming = true;
            RefreshActive();
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
            float power = EffectivePower();
            Vector2 direction = slingshot.LaunchDirection;
            EndAim();

            // Puntero casi sobre Mino: no hay arrastre, no se lanza nada y no se gasta hilo.
            if (power < minPowerToThrow || direction == Vector2.zero) return;

            // Min con el hilo actual evita que un error de decimales haga fallar el cobro.
            float cost = Mathf.Min(ThrowCostFor(power), thread.Current);
            if (!thread.TryConsume(cost)) return;

            Throw(direction, LaunchSpeedFor(power));
        }

        /// <summary>
        /// Costo segun la fuerza: proporcional a la barra (llena = maxThrowCost, mitad = la mitad...)
        /// pero nunca menos que minThrowCost. Con 10 y 2.5: 100% = 10, 50% = 5, 25% = 2.5, minimo = 2.5.
        /// </summary>
        private float ThrowCostFor(float power)
        {
            return Mathf.Max(minThrowCost, maxThrowCost * power);
        }

        /// <summary>
        /// Fuerza que realmente se usa: la del arrastre, pero limitada por el hilo disponible.
        /// Si te queda poco hilo, el tiro sale mas corto en vez de no salir.
        /// </summary>
        private float EffectivePower()
        {
            return Mathf.Min(slingshot.Power01, thread.Current / maxThrowCost);
        }

        /// <summary>
        /// Velocidad segun la fuerza. La fuerza se reescala desde la zona muerta: justo por encima de
        /// "minPowerToThrow" sale a la velocidad minima, asi un arrastre corto da un tiro corto.
        /// </summary>
        private float LaunchSpeedFor(float power)
        {
            float t = Mathf.InverseLerp(minPowerToThrow, 1f, power);
            return Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, t);
        }

        private void EndAim()
        {
            slingshot.End();
            GameEvents.RaiseAimChanged(false, 0f);

            isAiming = false;
            RefreshActive();
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

            flyingPin = Instantiate(pinPrefab, BodyCenter, Quaternion.identity);
            flyingPin.Embedded += HandlePinEmbedded;
            flyingPin.Failed += HandlePinFailed;
            flyingPin.Launch(direction, speed, bodyCollider);

            // El alfiler nuevo no debe chocar con el alfiler en el que ya cuelga Mino.
            if (pin != null)
                Physics2D.IgnoreCollision(flyingPin.GetComponent<Collider2D>(), pin.GetComponent<Collider2D>());

            RefreshActive();
        }

        private void HandlePinEmbedded(PinProjectile embeddedPin)
        {
            if (embeddedPin != flyingPin) return;

            UnsubscribeFlying(embeddedPin);
            flyingPin = null;

            // El nuevo alfiler se clavo: Mino cambia de anclaje y el anterior (si habia) se quita.
            PinProjectile previousPin = pin;
            pin = embeddedPin;
            attachLength = Vector2.Distance(BodyCenter, pin.transform.position);

            // Un solo joint, que se reutiliza al cambiar de anclaje.
            if (joint == null)
            {
                joint = gameObject.AddComponent<DistanceJoint2D>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = Vector2.zero;   // el origen del alfiler
                joint.autoConfigureDistance = false;
                joint.maxDistanceOnly = true;           // hilo: afloja, pero no se estira
            }

            joint.connectedBody = pin.Body;
            joint.anchor = transform.InverseTransformPoint(BodyCenter); // el pecho de Mino
            joint.distance = attachLength;

            if (previousPin != null) previousPin.Release();

            controller.ExternalMovement = !Ground.IsGrounded;
            RefreshActive();
        }

        private void HandlePinFailed(PinProjectile failedPin)
        {
            if (failedPin != flyingPin) return;

            // El alfiler nuevo no se clavo: si Mino colgaba de otro, sigue colgado.
            UnsubscribeFlying(failedPin);
            flyingPin = null;
            RefreshActive();
        }

        private void UnsubscribeFlying(PinProjectile p)
        {
            p.Embedded -= HandlePinEmbedded;
            p.Failed -= HandlePinFailed;
        }

        private void DiscardFlyingPin()
        {
            if (flyingPin == null) return;

            UnsubscribeFlying(flyingPin);
            flyingPin.Release();
            flyingPin = null;
            RefreshActive();
        }

        /// <summary>
        /// W recoge el hilo (Mino sube hacia el alfiler) y S lo suelta (hasta el largo original).
        /// Al recoger parte de la distancia REAL, asi si el hilo estaba flojo empieza a tirar de inmediato.
        /// </summary>
        private void AdjustRopeLength()
        {
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

        /// <summary>Suelta el hilo en el que cuelga: quita el joint y su alfiler. Seguro de llamar varias veces.</summary>
        private void Detach()
        {
            if (joint == null && pin == null) return;

            if (joint != null)
            {
                joint.enabled = false; // deja de actuar ya; Destroy se aplica al final del frame
                Destroy(joint);
            }

            if (pin != null) pin.Release();

            joint = null;
            pin = null;
            controller.ExternalMovement = false;
            RefreshActive();
        }

        /// <summary>Apaga todo lo visual de golpe. Se usa al cancelar, porque si la habilidad
        /// se deshabilita, LateUpdate ya no corre y nadie mas apagaria estos objetos.</summary>
        private void HideAllVisuals()
        {
            if (rangeIndicator != null) rangeIndicator.gameObject.SetActive(false);
            if (aimLine != null) aimLine.enabled = false;
            if (trajectoryLine != null) trajectoryLine.enabled = false;
            if (threadLine != null) threadLine.enabled = false;
            previewCached = false;
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

            // El hilo se dibuja hacia el alfiler en que cuelga; si no cuelga, hacia el que vuela.
            Transform lineTarget = pin != null ? pin.transform : (flyingPin != null ? flyingPin.transform : null);

            threadLine.enabled = lineTarget != null;

            if (lineTarget != null)
            {
                threadLine.SetPosition(0, VisualCenter);
                threadLine.SetPosition(1, lineTarget.position);
            }
        }

        private void UpdateAimVisuals()
        {
            if (rangeIndicator != null)
            {
                rangeIndicator.gameObject.SetActive(isAiming);

                if (isAiming)
                {
                    rangeIndicator.position = VisualCenter;
                    float diameter = maxDragDistance * 2f;
                    rangeIndicator.localScale = new Vector3(diameter, diameter, 1f);
                }
            }

            if (aimLine != null)
            {
                bool showLine = isAiming && slingshot.Power01 > 0f;
                aimLine.enabled = showLine;

                if (showLine)
                {
                    Vector2 start = VisualCenter;
                    aimLine.SetPosition(0, start);
                    aimLine.SetPosition(1, start + slingshot.DragOffset);
                }
            }

            UpdateTrajectoryPreview();
        }

        /// <summary>
        /// Dibuja la parabola que seguira el alfiler con la fuerza actual. Usa la misma gravedad,
        /// velocidad y formula que el vuelo real (ProjectileMath), y se corta donde chocaria.
        /// Optimizada: un solo arreglo reutilizado, nada se calcula si el origen y la velocidad
        /// no cambiaron desde el frame anterior, y solo existe mientras se apunta.
        /// </summary>
        private void UpdateTrajectoryPreview()
        {
            if (trajectoryLine == null || pinPrefab == null) return;

            float power = EffectivePower();
            bool show = isAiming && power >= minPowerToThrow;
            if (!show)
            {
                trajectoryLine.enabled = false;
                previewCached = false;
                return;
            }

            Vector2 origin = BodyCenter;
            Vector2 velocity = slingshot.LaunchDirection * LaunchSpeedFor(power);
            trajectoryLine.enabled = true;

            if (previewCached && origin == lastPreviewOrigin && velocity == lastPreviewVelocity)
                return; // nada cambio: la linea ya esta bien

            previewCached = true;
            lastPreviewOrigin = origin;
            lastPreviewVelocity = velocity;

            Vector2 gravity = Physics2D.gravity * pinPrefab.GravityScale;
            float step = pinPrefab.MaxLifetime / (trajectoryPoints - 1);

            int count = 0;
            trajectoryBuffer[count++] = origin;
            Vector2 previous = origin;

            for (int i = 1; i < trajectoryPoints; i++)
            {
                Vector2 point = ProjectileMath.PositionAt(origin, velocity, gravity, step * i);

                if (trajectoryBlockers.value != 0)
                {
                    RaycastHit2D hit = Physics2D.Linecast(previous, point, trajectoryBlockers);
                    if (hit.collider != null)
                    {
                        trajectoryBuffer[count++] = hit.point; // la guia termina donde chocaria
                        break;
                    }
                }

                trajectoryBuffer[count++] = point;
                previous = point;
            }

            trajectoryLine.positionCount = count;
            trajectoryLine.SetPositions(trajectoryBuffer);
        }
    }
}