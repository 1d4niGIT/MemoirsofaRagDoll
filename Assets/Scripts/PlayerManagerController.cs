using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerManagerController : MonoBehaviour
{
    [Header("Movement")]
    public float Speed = 8f;
    public float jumpForce = 5f;

    [Header("Ground Check")]
    public bool isGrounded;
    public float moveInput;
    public LayerMask groundLayer;        
    public float groundCheckDistance = 0.05f;

    [Header("Salto (Coyote / Buffer / Variable)")]
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;
    [Range(0.1f, 1f)] public float jumpCutMultiplier = 0.5f;

    [Header("Correccion de esquina")]
    public float cornerTolerance = 0.25f;
    public int cornerSteps = 5;

    [Header("Agacharse")]
    [Range(0.1f, 1f)] public float crouchSpeedMultiplier = 0.5f;   
    [Range(0.3f, 0.9f)] public float crouchHeightRatio = 0.5f;

    [Header("Esconderse")]
    public SpriteRenderer spriteRenderer;
    [Range(0f, 1f)] public float hiddenAlpha = 0.6f;

    [Header("Balanceo (swing)")]
    public float swingPump = 6f;                 
    public float swingDamping = 0.15f;
    public float maxAngularSpeed = 6f;           
    public float launchMultiplier = 1.3f;      

  
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private Vector2 standSize, standOffset;
    private float defaultGravity;

    private float coyoteCounter, bufferCounter;
    private bool crouchHeld, isCrouching;

    private HideSpot currentHideSpot;
    public bool IsHidden { get; private set; }
    private int originalSortingOrder;
    private string originalSortingLayer;
    private float originalAlpha;

    private SwingPoint nearbySwing, activeSwing;
    private float swingAngle, swingAngularVel;
    public bool IsSwinging => activeSwing != null;

    private void OnEnable()
    {
        InputManagerController.OnMove += Move;
        InputManagerController.OnJump += Jump;
        InputManagerController.OnJumpReleased += JumpReleased;
        InputManagerController.OnCrouch += Crouch;
        InputManagerController.OnInteract += Interact;
    }

    private void OnDisable()
    {
        InputManagerController.OnMove -= Move;
        InputManagerController.OnJump -= Jump;
        InputManagerController.OnJumpReleased -= JumpReleased;
        InputManagerController.OnCrouch -= Crouch;
        InputManagerController.OnInteract -= Interact;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        standSize = col.size;
        standOffset = col.offset;
        defaultGravity = rb.gravityScale;

        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
            originalSortingLayer = spriteRenderer.sortingLayerName;
            originalAlpha = spriteRenderer.color.a;
        }
    }

    
    private void Update()
    {
        if (IsSwinging || IsHidden) return;

        
        UpdateCrouch();

        
        coyoteCounter = isGrounded ? coyoteTime : coyoteCounter - Time.deltaTime;

        
        bufferCounter -= Time.deltaTime;

        if (bufferCounter > 0f && coyoteCounter > 0f && !isCrouching)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            bufferCounter = 0f;
            coyoteCounter = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (IsSwinging) { SwingStep(); return; }

        CheckGround();

        if (IsHidden)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float speed = Speed * (isCrouching ? crouchSpeedMultiplier : 1f);
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        CornerCorrection();
    }

    
    private void Move(Vector2 input)
    {
        moveInput = input.x;
    }

    private void Jump()
    {
        if (IsSwinging) { Launch(); return; }
        if (IsHidden) return;
        bufferCounter = jumpBufferTime;     
    }

    private void JumpReleased()
    {
        
        if (!IsSwinging && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
    }

    private void Crouch(bool pressed)
    {
        crouchHeld = pressed;
    }

    private void Interact()
    {
        if (IsSwinging) { Launch(); return; }
        if (IsHidden) { SetHidden(false); return; }

        if (nearbySwing != null) { StartSwing(nearbySwing); return; }
        if (currentHideSpot != null && isGrounded) SetHidden(true);
    }

   
    private void CheckGround()
    {
        Bounds b = col.bounds;
        RaycastHit2D hit = Physics2D.BoxCast(b.center, new Vector2(b.size.x * 0.9f, b.size.y),
            0f, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null && rb.linearVelocity.y <= 0.01f;
    }

    
    private void CornerCorrection()
    {
        if (rb.linearVelocity.y <= 0f) return;

        Bounds b = col.bounds;
        Vector2 headSize = new Vector2(b.size.x * 0.9f, 0.1f);
        Vector2 headCenter = new Vector2(b.center.x, b.max.y + 0.05f);

        if (!Physics2D.OverlapBox(headCenter, headSize, 0f, groundLayer)) return;

        float step = cornerTolerance / cornerSteps;
        for (int i = 1; i <= cornerSteps; i++)
        {
            foreach (int dir in new[] { -1, 1 })
            {
                Vector2 shifted = headCenter + Vector2.right * dir * step * i;
                if (!Physics2D.OverlapBox(shifted, headSize, 0f, groundLayer))
                {
                    rb.position += Vector2.right * dir * step * i;
                    return;
                }
            }
        }
    }

   
    private void UpdateCrouch()
    {
        bool wantsCrouch = crouchHeld && isGrounded;

        if (wantsCrouch && !isCrouching)
        {
            isCrouching = true;
            float newH = standSize.y * crouchHeightRatio;
            col.size = new Vector2(standSize.x, newH);
            col.offset = new Vector2(standOffset.x, standOffset.y - (standSize.y - newH) * 0.5f);
        }
        else if (!wantsCrouch && isCrouching && CanStand())
        {
            isCrouching = false;
            col.size = standSize;
            col.offset = standOffset;
        }
    }

    private bool CanStand()
    {
        Bounds b = col.bounds;
        float extra = standSize.y * (1f - crouchHeightRatio);
        Vector2 center = new Vector2(b.center.x, b.max.y + extra * 0.5f);
        Vector2 size = new Vector2(b.size.x * 0.9f, extra * 0.9f);
        return !Physics2D.OverlapBox(center, size, 0f, groundLayer);
    }

   
    private void SetHidden(bool hidden)
    {
        IsHidden = hidden;
        if (spriteRenderer == null) return;

        if (hidden)
        {
            // el sprite del jugador pasa DETRAS del objeto
            spriteRenderer.sortingLayerName = currentHideSpot.SortingLayerName;
            spriteRenderer.sortingOrder = currentHideSpot.SortingOrder - 1;
            Color c = spriteRenderer.color; c.a = hiddenAlpha; spriteRenderer.color = c;
            rb.linearVelocity = Vector2.zero;
            moveInput = 0f;
        }
        else
        {
            spriteRenderer.sortingLayerName = originalSortingLayer;
            spriteRenderer.sortingOrder = originalSortingOrder;
            Color c = spriteRenderer.color; c.a = originalAlpha; spriteRenderer.color = c;
        }
    }

    
    private void StartSwing(SwingPoint point)
    {
        activeSwing = point;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        Vector2 rel = (Vector2)transform.position - (Vector2)point.transform.position;
        swingAngle = Mathf.Atan2(rel.x, -rel.y);
        swingAngularVel = 0f;

        isCrouching = false;
        col.size = standSize;
        col.offset = standOffset;
    }

    private void SwingStep()
    {
        float dt = Time.fixedDeltaTime;
        float L = activeSwing.length;

        
        float accel = -(Mathf.Abs(Physics2D.gravity.y) / L) * Mathf.Sin(swingAngle);
        accel += moveInput * swingPump;
        accel -= swingAngularVel * swingDamping;

        swingAngularVel = Mathf.Clamp(swingAngularVel + accel * dt, -maxAngularSpeed, maxAngularSpeed);
        swingAngle += swingAngularVel * dt;

        Vector2 pivot = activeSwing.transform.position;
        rb.linearVelocity = Vector2.zero;
        rb.MovePosition(pivot + new Vector2(Mathf.Sin(swingAngle), -Mathf.Cos(swingAngle)) * L);
    }

    private void Launch()
    {
        
        float L = activeSwing.length;
        Vector2 tangent = new Vector2(Mathf.Cos(swingAngle), Mathf.Sin(swingAngle));

        rb.gravityScale = defaultGravity;
        rb.linearVelocity = tangent * (swingAngularVel * L) * launchMultiplier;
        activeSwing = null;
        coyoteCounter = 0f;
    }

   
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out HideSpot hs)) currentHideSpot = hs;
        if (other.TryGetComponent(out SwingPoint sp)) nearbySwing = sp;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out HideSpot hs) && hs == currentHideSpot)
        {
            if (IsHidden) SetHidden(false);
            currentHideSpot = null;
        }
        if (other.TryGetComponent(out SwingPoint sp) && sp == nearbySwing) nearbySwing = null;
    }
}
