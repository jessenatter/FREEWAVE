using UnityEngine;

public class Character : MonoBehaviour
{
    protected float moveSpeed = 3.5f;
    protected float jumpForce = 2f;

    bool grounded,isJumping;

    float cayoteTimer = 10, cayoteTimerCurrent = 0;

    [SerializeField] protected LayerMask groundLayer;

    protected Rigidbody2D rb;

    protected BoxCollider2D bc;

    protected Manager manager;

    protected float xInput;

    public enum characterState
    {
        movement,
        attacking,
        attackingDown,
        dashAttacking,
    }

    public characterState currentCharacterState = characterState.movement;

    float attackTimer = 30,attackTimerCurrent;

    float dashAttackTimer = 50,dashAttackTimerCurrent;

    protected bool characterIsActive;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
    }

    protected virtual void Update()
    {
        
    }

    protected virtual void FixedUpdate() //rb stuff
    {
        if(characterIsActive)
        {
            if(currentCharacterState == characterState.movement)
                MovementUpdate();
            else if(currentCharacterState == characterState.attacking)
                AttackUpdate();
            else if(currentCharacterState == characterState.attackingDown)
                DownAttackUpdate();
            else if(currentCharacterState == characterState.dashAttacking)
                DashAttackUpdate();
        }
    }

    protected virtual void MovementUpdate()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, bc.size * 0.9f, 0, Vector2.down, 0.1f, groundLayer);

        if (hit.collider != null)
        {
            grounded = true;
            cayoteTimerCurrent = 0;
            isJumping = false;
        }
        else if (grounded)
        {
            cayoteTimerCurrent++;
            if (cayoteTimerCurrent == cayoteTimer)
            {
                grounded = false;
                cayoteTimerCurrent = 0;
            }
        }

        rb.linearVelocityX = xInput * moveSpeed;
    }
    protected virtual void Jump()
    {
        if (grounded && !isJumping)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJumping = true;
        }
    }
    void Attack()
    {
        
    }

    void DashAttack()
    {
        
    }

    void DownAttack()
    {
        
    }
    void AttackUpdate()
    {
        
    }

    void DownAttackUpdate()
    {
        
    }

    void DashAttackUpdate()
    {
        
    }
}
