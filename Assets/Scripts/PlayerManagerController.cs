using System;
using UnityEngine;


public class PlayerManagerController : MonoBehaviour
{
    [Header("Movement")]
    public float Speed = 8f;
    public float jumpForce = 5f;

    [Header("Ground Check")]
    public bool isGrounded;
    public float moveInput;

    [Header("Combat")]
    public float AttackRadius;
    public int damage = 24;

    private Rigidbody2D rb;

    public static event Action OnHit;
    public static event Action<bool> OnGround;
    //OnHit?.Invoke();

    private void OnEnable()
    {
        InputManagerController.OnMove += Move;
        InputManagerController.OnJump += Jump;
        InputManagerController.OnAttack += Attack;
    }

    private void OnDisable()
    {
        InputManagerController.OnMove -= Move;
        InputManagerController.OnJump -= Jump;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * Speed, rb.linearVelocity.y);
    }

    private void Move (Vector2 input)
    {
        moveInput = input.x;
    }

    private void Jump()
    {
        if (!isGrounded) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void Attack()
    {
        Collider2D[] Coll = Physics2D.OverlapCircleAll(transform.position, AttackRadius);

        foreach (Collider2D coll in Coll)
        {
            if(coll.gameObject.GetComponent<IDamageable>() != null)
            {
                coll.gameObject.GetComponent<IDamageable>().TakeDamage(damage);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<IDamageable>() != null)
        {
            //->a
            //->b
            //->c
            OnHit?.Invoke();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            OnGround?.Invoke(true);
        }
    }


    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag ("Ground"))
        {
            isGrounded = false;
            OnGround?.Invoke(false);
        }
    }

    /*[SerializeField] private float Speed = 5;
    void Start()
    {
        print(PlayerInputController.MoveInput);
    }
    private void OnEnable()
    {
   
    }
    void Update()
    {
        MovementeMechanic();
    }

    public void MovementeMechanic()
    {
        Vector2 moveInput = PlayerInputController.MoveInput;

        Vector3 dir = new Vector3(moveInput.x, moveInput.y, 0);

        transform.position += dir * speed * Time.deltaTime;
    }

    public float speed => Speed;*/
}