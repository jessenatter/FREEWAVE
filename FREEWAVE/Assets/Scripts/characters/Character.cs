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

    protected bool nearPickupable;
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
    public float attackTimer = 15,attackTimerCurrent;

    public float attackCD = 15,attackCDcurrent;
    protected float dashAttackTimer = 25,dashAttackTimerCurrent;
    protected float hurtTimer = 30,hurtTimerCurrent;
    public bool characterIsActive,getAttackInput,groundedHit;
    GameObject attackCollider,downAttackCollider;
    [SerializeField] int hurtLayer;
    CharacterAnimator characterAnimator;
    CharacterAnimator.lowerBodyState previousLowerBodyState;
    CharacterAnimator.upperBodyState previousUpperBodyState;
    public float health = 10,damage = 1,damageToRecive;
    bool canAttack = true;
    [SerializeField] GameObject hand;
    protected PickupAble heldObject,nearbyObject;

    [SerializeField] bool isPlayer;
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        attackCollider = transform.GetChild(0).gameObject;
        downAttackCollider = transform.GetChild(1).gameObject;

        characterAnimator = GetComponent<CharacterAnimator>();
        characterAnimator.attackStateDuration = attackTimer;
        characterAnimator.dashAttackStateDuration = dashAttackTimer;
        characterAnimator.hurtStateDuration = hurtTimer;
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

            getAttackInput = false;
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

        AnimatorUpdate();
        AttackCDupdate();
        checkForPickupables();
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
        if(currentCharacterState == characterState.movement && canAttack)
        {
            currentCharacterState = characterState.attacking;
            characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyAttack;
            characterAnimator.currentUpperBodyState = characterAnimator.upperBodyAttack;
            attackCollider.SetActive(true);
            canAttack = false;
        }
    }
    protected virtual void DashAttack()
    {
        if(currentCharacterState == characterState.movement)
        {
            currentCharacterState = characterState.dashAttacking;
            characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyDashAttack;
            characterAnimator.currentUpperBodyState = characterAnimator.upperBodyDashAttack;
            attackCollider.SetActive(true);
            dashXinput = xInput;
        }
    }
    protected virtual void DownAttack()
    {
        if(currentCharacterState == characterState.movement)
        {
            currentCharacterState = characterState.attackingDown;
            characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyDropAttack;
            characterAnimator.currentUpperBodyState = characterAnimator.upperBodyDropAttack;
            downAttackCollider.SetActive(true);
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 5f;
        }
    }
    protected virtual void Hurt(Vector2 hurtDir,float damage)
    {
        if(currentCharacterState != characterState.hurting)
        {
            rb.AddForce(hurtDir * knockbackForce,ForceMode2D.Impulse);
            currentCharacterState = characterState.hurting;
            health -= damage;
            health = Mathf.Clamp(health,0,10);
            if(health == 0)
                Die();
        }
    }
    protected virtual void Die()
    {
        Destroy(gameObject);
    }
    void AnimatorUpdate()
    {
        if(groundedHit == false)
            isJumping = true;

        if(xInput == 0)
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

    void AttackCDupdate()
    {
        if(!canAttack && currentCharacterState != characterState.attacking)
        {
            attackCDcurrent++;
            if(attackCDcurrent == attackCD)
            {
                attackCDcurrent = 0;
                canAttack = true;
            }
        }
    }

    void checkForPickupables()
    {
        float minPickupDistance = 1.5f;
        float lastPickupDistance = 0;
        PickupAble closestPickupable = null;

        foreach(PickupAble pickupable in manager.pickupAbles)
        {
            if(pickupable.held) return;

            Vector2 distance = transform.position - pickupable.transform.position;

            if(distance.magnitude < minPickupDistance)
            {
                nearPickupable = true;
                if(closestPickupable == null || distance.magnitude < lastPickupDistance)
                {
                    closestPickupable = pickupable;
                    lastPickupDistance = distance.magnitude;

                    if(nearbyObject != null)
                    {
                        if(nearbyObject != closestPickupable && isPlayer)
                            nearbyObject.pickupPrompt.SetActive(false);
                    }

                    nearbyObject = closestPickupable;
                }
            }
            

            if(isPlayer && nearbyObject != null)
                nearbyObject.pickupPrompt.SetActive(true);
        }

        if(closestPickupable == null && nearbyObject != null)
        {
            if(isPlayer)
                nearbyObject.pickupPrompt.SetActive(false);
                
            nearbyObject = null;
            nearPickupable = false;
        }
    }

    protected void PickupObject()
    {
        if(nearbyObject == null) return;

        nearbyObject.pickupPrompt.SetActive(false);
        nearbyObject.transform.position = hand.transform.position;
        nearbyObject.transform.rotation = hand.transform.rotation;
        nearbyObject.transform.SetParent(hand.transform);
        nearbyObject.held = true;
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
