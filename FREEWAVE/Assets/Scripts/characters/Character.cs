using System;
using Unity.Mathematics;
using UnityEditor.Build;
using UnityEngine;

public class Character : MonoBehaviour
{
    //defines combat / movement behavior for characters
    protected float moveSpeed = 3.5f, jumpForce = 27.5f,dashAttackSpeed = 10f;
    float knockbackCarryDecay = 8f;
    float carriedKnockbackXVelocity;
    protected float xInput,yInput,dashXinput;
    protected bool grounded,isJumping,canJump,recentlyIdle,canAttack = true,airMovement = false;
    protected LayerMask groundLayer;
    protected Rigidbody2D rb; protected BoxCollider2D bc;

    //please add no more character states during development, keep it to these
    [HideInInspector]public enum characterState
    {
        movement,
        attacking,
        attackingDown,
        dashAttacking,
        hurting,
        idle,
        frozen,
        dead,
    }
    [HideInInspector] public characterState currentCharacterState = characterState.movement;

    #region timers
    PublicTimer cayoteTimer = new PublicTimer(10f);
    [HideInInspector] public PublicTimer jumpCooldown = new PublicTimer(2f); //ammount of time grounded in order to jump
    [HideInInspector] public PublicTimer attackTimer = new PublicTimer(15f);
    [HideInInspector] public PublicTimer attackCD = new PublicTimer(10f); //use same cooldown for all attacks
    [HideInInspector] public PublicTimer dashAttackTimer = new PublicTimer(25f);
    protected PublicTimer hurtTimer = new PublicTimer(30f);
    protected PublicTimer postHitInvincibilityTimer = new PublicTimer(30f);

    #endregion
    [HideInInspector] public bool characterIsActive,getAttackInput,groundedHit;
    GameObject attackCollider,downAttackCollider;
    [SerializeField] int hurtLayer; //layer of the collider that can hurt this character
    CharacterAnimator characterAnimator;
    CharacterAnimator.lowerBodyState previousLowerBodyState;
    CharacterAnimator.upperBodyState previousUpperBodyState;
    [HideInInspector] public float health = 10,damageToRecive,knockbackForceToRecive;
    [SerializeField] protected GameObject backHand,frontHand; //for putting stuff in back and front hands
    [HideInInspector] public PickupAble heldPickupable;
    protected float postHitInvincibilitySeconds = 1f;
    protected bool isPostHitInvincible;
    PublicTimer recentlyIdleTimer = new PublicTimer(15f);
    protected float initGravityScale = 2.7f,downAttackGravityScale;
    protected float groundedDistance = 0.3f;
    protected virtual void Start()
    {
        groundLayer = LayerMask.GetMask("Ground");
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        attackCollider = transform.GetChild(0).gameObject;
        downAttackCollider = transform.GetChild(1).gameObject;

        characterAnimator = GetComponent<CharacterAnimator>();
        //set chr animator values
        characterAnimator.runStateDuration = 30f;
        characterAnimator.idleStateDuration = 150f;
        
        characterAnimator.CharacterAnimatorStart();
        rb.gravityScale = initGravityScale;
        downAttackGravityScale = initGravityScale * 2f;
        postHitInvincibilityTimer.SetDuration(postHitInvincibilitySeconds * 60f);
        postHitInvincibilityTimer.Complete();
    }
    protected virtual void Update()
    {
        if(currentCharacterState == characterState.frozen) return;

        RaycastHit2D hit = Physics2D.BoxCast(transform.position, bc.size * 0.9f, 0, Vector2.down, groundedDistance, groundLayer);

        if(hit.collider != null)
            groundedHit = true;
        else
            groundedHit = false;

        if(xInput == 0)
            recentlyIdle = true;
        else if(recentlyIdle)
        {
            if(recentlyIdleTimer.TickLoop())
                recentlyIdle = false;
        }

        if(getAttackInput) //attack is determinded by x and y input
        {
            if(xInput == 0 && (Mathf.Sign(yInput) != -1 || grounded))
                Attack();
            else if(grounded && (recentlyIdle || xInput != 0))
                DashAttack();
            else if(Mathf.Sign(yInput) == -1)
                DownAttack();
            else if(!grounded)
                Attack();

            getAttackInput = false;
        }
    }
    protected virtual void FixedUpdate() //rb stuff
    {
        UpdatePostHitInvincibility();

        if(currentCharacterState == characterState.frozen)
        {
            AnimatorUpdate();
            rb.linearVelocity = new Vector2(0,rb.linearVelocityY);
            return;
        }

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

        AnimatorUpdate();
        AttackCDupdate();
        characterAnimator.CharacterAnimatorFixedUpdate(); //i guess we are calling it from here for order of opperations?
    }
    protected virtual void LateUpdate()
    {
        characterAnimator.CharacterAnimatorUpdate();
    }
    protected virtual void MovementUpdate()
    {
        if(xInput != 0)
            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x) * Mathf.Sign(xInput),transform.localScale.y);

        if (groundedHit)
        {
            grounded = true;
            cayoteTimer.Reset();

            if(isJumping)
            {
                isJumping = false;

                if(rb.linearVelocityY < 0)
                    Land();
            }

            if(jumpCooldown.Tick())
                canJump = true;
        }
        else if (grounded)
        {
            if(cayoteTimer.TickLoop())
                grounded = false;
        }

        if(!groundedHit)
            jumpCooldown.Reset();

        if(airMovement)
            ApplyHoriziontalMoveSpeed();
        else if(groundedHit || Mathf.Abs(carriedKnockbackXVelocity) > 0.01f)
            ApplyHoriziontalMoveSpeed();
    }

    void ApplyHoriziontalMoveSpeed()
    {
        float inputXVelocity = xInput * moveSpeed;
        if(groundedHit)
            carriedKnockbackXVelocity = 0f;
            
        if(Mathf.Abs(carriedKnockbackXVelocity) > 0.01f)
        {
            rb.linearVelocityX = inputXVelocity + carriedKnockbackXVelocity;
            carriedKnockbackXVelocity = Mathf.MoveTowards(carriedKnockbackXVelocity,0f,knockbackCarryDecay * Time.fixedDeltaTime);
        }
        else
        {
            carriedKnockbackXVelocity = 0f;
            rb.linearVelocityX = inputXVelocity;
        }
    }
    protected virtual void Jump()
    {
        if (grounded && !isJumping && canJump) DoJump();
    }
    protected virtual void DoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocityX,0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isJumping = true;
        canJump = false;
    }
    protected virtual void Attack()
    {
        if(currentCharacterState != characterState.movement || !canAttack) return;

        currentCharacterState = characterState.attacking;
        characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyAttack;
        characterAnimator.currentUpperBodyState = characterAnimator.upperBodyAttack;
        attackCollider.SetActive(true);
        canAttack = false;
        attackTimer.Reset();
        attackCD.Reset();
    }
    protected virtual void DashAttack()
    {
        if(currentCharacterState != characterState.movement || !canAttack) return;

        canAttack = false;
        currentCharacterState = characterState.dashAttacking;
        characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyDashAttack;
        characterAnimator.currentUpperBodyState = characterAnimator.upperBodyDashAttack;
        attackCollider.SetActive(true);
        dashXinput = xInput;
        dashAttackTimer.Reset();
        attackCD.Reset();
        if(xInput != 0)
            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x) * Mathf.Sign(xInput),transform.localScale.y);
    }
    protected virtual void DownAttack()
    {
        if(currentCharacterState != characterState.movement || !canAttack) return;
        
        currentCharacterState = characterState.attackingDown;
        characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyDropAttack;
        characterAnimator.currentUpperBodyState = characterAnimator.upperBodyDropAttack;
        downAttackCollider.SetActive(true);
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = downAttackGravityScale;
        canAttack = false;
        attackCD.Reset();
    }
    protected virtual void Hurt(int hurtDir,float damage,float knockbackForce)
    {
        if(currentCharacterState == characterState.hurting || currentCharacterState == characterState.dead || isPostHitInvincible) return;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(hurtDir,1f) * knockbackForce,ForceMode2D.Impulse);
        currentCharacterState = characterState.hurting;
        health -= damage;
        health = Mathf.Clamp(health,0,10);
        characterAnimator.currentUpperBodyState = characterAnimator.upperBodyHurt;
        characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyHurt;
        if(health == 0)
            Die(hurtDir);
        
        attackCollider.SetActive(false);
        downAttackCollider.SetActive(false);
        hurtTimer.Reset();
        isPostHitInvincible = true;
        postHitInvincibilityTimer.Reset();
    }
    protected virtual void Die(int dir)
    {
        currentCharacterState = characterState.dead;
    }
    void AnimatorUpdate()
    {
        if(groundedHit == false)
            isJumping = true;

        if(currentCharacterState == characterState.frozen)
        {
            characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyIdle;
            characterAnimator.currentUpperBodyState = characterAnimator.upperBodyIdle;
        }
        else if(xInput == 0)
        {
            if(!isJumping && currentCharacterState == characterState.movement)
            {
                characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyIdle;
                characterAnimator.currentUpperBodyState = characterAnimator.upperBodyIdle;
            }
        }
        else if(!isJumping && currentCharacterState == characterState.movement)
        {
            characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyRun;
            characterAnimator.currentUpperBodyState = characterAnimator.upperBodyRun;
        }
        
        if(isJumping && currentCharacterState == characterState.movement)
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
        if(attackTimer.TickLoop())
        {
            currentCharacterState = characterState.movement;
            attackCollider.SetActive(false);
            print("bb");
        }
    }
    void DownAttackUpdate()
    {
        if(!groundedHit) return;

        currentCharacterState = characterState.movement;
        downAttackCollider.SetActive(false);
        rb.gravityScale = initGravityScale;
    }
    void DashAttackUpdate()
    {
        if(dashAttackTimer.TickLoop())
        {
            currentCharacterState = characterState.movement;
            attackCollider.SetActive(false);
            print("aa");
        }
        
        rb.linearVelocityX = dashXinput * dashAttackSpeed;
    }
    void HurtUpdate()
    {
        hurtTimer.Tick();
        if(hurtTimer.IsComplete)
            ExitHurtState();
    }

    protected virtual void ExitHurtState()
    {
        hurtTimer.Reset();
        carriedKnockbackXVelocity = rb.linearVelocityX;
        currentCharacterState = characterState.movement;
    }
    void AttackCDupdate()
    {
        if(!canAttack && currentCharacterState != characterState.attacking)
        {
            if(attackCD.TickLoop())
                canAttack = true;
        }
    }
    void UpdatePostHitInvincibility()
    {
        if(!isPostHitInvincible) return;

        if(postHitInvincibilityTimer.Tick())
            isPostHitInvincible = false;
    }
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == hurtLayer)
        {
            float _x = Mathf.Sign(transform.position.x - collision.gameObject.transform.parent.transform.position.x);
            Hurt((int)_x,damageToRecive,knockbackForceToRecive);
        }
    }

    protected virtual void Land()
    {
        
    }
}
