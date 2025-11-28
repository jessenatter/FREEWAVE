using System;
using Unity.Mathematics;
using UnityEngine;

public class Character : MonoBehaviour
{
    protected float moveSpeed = 3.5f, jumpForce = 2f,dashAttackSpeed = 10f,knockbackForce = 5f;
    protected float xInput,yInput,dashXinput;
    bool grounded,isJumping;
    [SerializeField] protected LayerMask groundLayer;
    protected Rigidbody2D rb;
    protected BoxCollider2D bc;
    protected Manager manager;
    public enum characterState
    {
        movement,
        attacking,
        attackingDown,
        dashAttacking,
        hurting,
    }
    public characterState currentCharacterState = characterState.movement;
    float cayoteTimer = 10, cayoteTimerCurrent = 0;
    public float attackTimer = 10,attackTimerCurrent;
    protected float dashAttackTimer = 25,dashAttackTimerCurrent;
    protected float hurtTimer = 30,hurtTimerCurrent;
    public bool characterIsActive,getAttackInput,groundedHit;
    GameObject attackCollider,downAttackCollider;
    [SerializeField] int hurtLayer;
    CharacterAnimator characterAnimator;
    CharacterAnimator.lowerBodyState previousLowerBodyState;
    CharacterAnimator.upperBodyState previousUpperBodyState;
    public float health = 10,damage = 1,damageToRecive;

    [SerializeField] bool useAnimatior = true;
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        attackCollider = transform.GetChild(0).gameObject;
        downAttackCollider = transform.GetChild(1).gameObject;

        if(useAnimatior)
            characterAnimator = GetComponent<CharacterAnimator>();
    }
    protected virtual void Update()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, bc.size * 0.9f, 0, Vector2.down, 0.1f, groundLayer);

        if(hit.collider != null)
            groundedHit = true;
        else
            groundedHit = false;

        if(getAttackInput)
        {
            if(xInput == 0 && (Mathf.Sign(yInput) != -1 || grounded))
                Attack();
            else if(grounded)
                DashAttack();
            else if(Mathf.Sign(yInput) == -1)
                DownAttack();
            else if(!grounded)
                Attack();
        }
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
            else if(currentCharacterState == characterState.hurting)
                HurtUpdate();
        }
    }
    protected virtual void MovementUpdate()
    {
        if(xInput != 0)
            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x) * Mathf.Sign(xInput),transform.localScale.y);

        if (groundedHit)
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

        if(useAnimatior)
            AnimatorUpdate();
    }
    protected virtual void Jump()
    {
        if (grounded && !isJumping)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJumping = true;
        }
    }
    protected virtual void Attack()
    {
        currentCharacterState = characterState.attacking;
        characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyAttack;
        characterAnimator.currentUpperBodyState = characterAnimator.upperBodyAttack;
        attackCollider.SetActive(true);
    }
    protected virtual void DashAttack()
    {
        currentCharacterState = characterState.dashAttacking;
        characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyDashAttack;
        characterAnimator.currentUpperBodyState = characterAnimator.upperBodyDashAttack;
        attackCollider.SetActive(true);
        dashXinput = xInput;
    }
    protected virtual void DownAttack()
    {
        currentCharacterState = characterState.attackingDown;
        characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyDropAttack;
        characterAnimator.currentUpperBodyState = characterAnimator.upperBodyDropAttack;
        downAttackCollider.SetActive(true);
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 5f;
    }
    protected virtual void Hurt(Vector2 hurtDir,float damage)
    {
        if(currentCharacterState == characterState.hurting) return;

        rb.AddForce(hurtDir * knockbackForce,ForceMode2D.Impulse);
        currentCharacterState = characterState.hurting;
        health -= damage;
        health = Mathf.Clamp(health,0,10);
        if(health == 0)
            Die();
    }
    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    void AnimatorUpdate()
    {
        if(xInput == 0)
        {
            if(!isJumping && currentCharacterState != characterState.attacking && currentCharacterState != characterState.dashAttacking && currentCharacterState != characterState.attackingDown && currentCharacterState != characterState.hurting)
            {
                characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyIdle;
                characterAnimator.currentUpperBodyState = characterAnimator.upperBodyIdle;
            }
        }
        else if(!isJumping)
        {
            characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyRun;
            characterAnimator.currentUpperBodyState = characterAnimator.upperBodyRun;
        }
        
        if(isJumping)
        {
            characterAnimator.currentUpperBodyState = characterAnimator.upperBodyJump;
            characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyJump;
        }

        if(characterAnimator.currentLowerBodyState != previousLowerBodyState)
            characterAnimator.currentLowerBodyState.EnterState();

        if(characterAnimator.currentUpperBodyState != previousUpperBodyState)
            characterAnimator.currentUpperBodyState.EnterState();

        previousLowerBodyState = characterAnimator.currentLowerBodyState;
        previousUpperBodyState = characterAnimator.currentUpperBodyState;
    }
    void AttackUpdate()
    {
        attackTimerCurrent++;
        if(attackTimerCurrent == attackTimer)
        {
            attackTimerCurrent = 0;
            currentCharacterState = characterState.movement;
            attackCollider.SetActive(false);
        }
    }
    void DownAttackUpdate()
    {
        if(groundedHit)
        {
            currentCharacterState = characterState.movement;
            downAttackCollider.SetActive(false);
            rb.gravityScale = 1;
        }
    }
    void DashAttackUpdate()
    {
        dashAttackTimerCurrent++;
        if(dashAttackTimerCurrent == dashAttackTimer)
        {
            dashAttackTimerCurrent = 0;
            currentCharacterState = characterState.movement;
            attackCollider.SetActive(false);
        }
        
        rb.linearVelocityX = dashXinput * dashAttackSpeed;
    }
    void HurtUpdate()
    {
        hurtTimerCurrent++;
        if(hurtTimerCurrent >= hurtTimer)
        {
            if(groundedHit)
            {
                hurtTimerCurrent = 0;
                currentCharacterState = characterState.movement;
            }
        }
    }
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == hurtLayer)
        {
            float _x = Mathf.Sign(transform.position.x - collision.gameObject.transform.position.x);
            Vector2 hurtVec = new Vector2(_x,1);
            Hurt(hurtVec,damageToRecive);
        }
    }
}
