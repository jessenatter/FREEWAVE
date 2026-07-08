using System;
using Unity.Mathematics;
using UnityEngine;

public class Character : MonoBehaviour
{
    protected float moveSpeed = 3.5f, jumpForce = 2f,dashAttackSpeed = 10f,knockbackForce = 5f;
    protected float xInput,yInput,dashXinput;
    bool grounded,isJumping,recentlyIdle;
    [SerializeField] protected LayerMask groundLayer;
    protected Rigidbody2D rb;
    protected BoxCollider2D bc;
    protected bool nearInteractable;
    [HideInInspector]public enum characterState
    {
        movement,
        attacking,
        attackingDown,
        dashAttacking,
        hurting,
        idle,
    }
    [HideInInspector]public characterState currentCharacterState = characterState.movement;
    float cayoteTimer = 10, cayoteTimerCurrent = 0;
    [HideInInspector]public float attackTimer = 15,attackTimerCurrent;
    [HideInInspector]public float attackCD = 10,attackCDcurrent; //use same cooldown for all attacks
    [HideInInspector]public float dashAttackTimer = 25,dashAttackTimerCurrent;
    protected float hurtTimer = 30,hurtTimerCurrent;
    [HideInInspector]public bool characterIsActive,getAttackInput,groundedHit;
    GameObject attackCollider,downAttackCollider;
    [SerializeField] int hurtLayer;
    CharacterAnimator characterAnimator;
    CharacterAnimator.lowerBodyState previousLowerBodyState;
    CharacterAnimator.upperBodyState previousUpperBodyState;
    [HideInInspector] public float health = 10,damage = 1,damageToRecive;
    bool canAttack = true;
    [SerializeField] GameObject hand;
    [HideInInspector] public PickupAble heldPickupable;
    protected Interactable lastClosestInteractable;
    [SerializeField] bool isPlayer;
    float recentlyIdleTimer = 15,idleTimerCurrent = 0;
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        attackCollider = transform.GetChild(0).gameObject;
        downAttackCollider = transform.GetChild(1).gameObject;

        characterAnimator = GetComponent<CharacterAnimator>();
        //set chr animator values
        characterAnimator.runStateDuration = 30f;
        characterAnimator.idleStateDuration = 150f;
        
        characterAnimator.CharacterAnimatorStart();
    }
    protected virtual void Update()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, bc.size * 0.9f, 0, Vector2.down, 0.1f, groundLayer);

        if(hit.collider != null)
            groundedHit = true;
        else
            groundedHit = false;

        if(xInput == 0)
        {
            recentlyIdle = true;
        }
        else if(recentlyIdle)
        {
            idleTimerCurrent++;
            if(idleTimerCurrent == recentlyIdleTimer)
            {
                recentlyIdle = false;
                idleTimerCurrent = 0;
            }
        }

        if(getAttackInput)
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

        characterAnimator.CharacterAnimatorUpdate();
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
        checkForInteractables();
        characterAnimator.CharacterAnimatorFixedUpdate(); //i guess we are calling it from here for order of opperations?
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
            transform.position += Vector3.one * 0.0001f;
        }
    }
    protected virtual void DashAttack()
    {
        if(currentCharacterState == characterState.movement && canAttack)
        {
            canAttack = false;
            currentCharacterState = characterState.dashAttacking;
            characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyDashAttack;
            characterAnimator.currentUpperBodyState = characterAnimator.upperBodyDashAttack;
            attackCollider.SetActive(true);
            dashXinput = xInput;
            if(xInput != 0)
                transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x) * Mathf.Sign(xInput),transform.localScale.y);
        }
    }
    protected virtual void DownAttack()
    {
        if(currentCharacterState == characterState.movement && canAttack)
        {
            currentCharacterState = characterState.attackingDown;
            characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyDropAttack;
            characterAnimator.currentUpperBodyState = characterAnimator.upperBodyDropAttack;
            downAttackCollider.SetActive(true);
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 5f;
            canAttack = false;
        }
    }
    protected virtual void Hurt(Vector2 hurtDir,float damage)
    {
        if(currentCharacterState != characterState.hurting)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(hurtDir * knockbackForce,ForceMode2D.Impulse);
            currentCharacterState = characterState.hurting;
            health -= damage;
            health = Mathf.Clamp(health,0,10);
            characterAnimator.currentUpperBodyState = characterAnimator.upperBodyHurt;
            characterAnimator.currentLowerBodyState = characterAnimator.lowerBodyHurt;
            if(health == 0)
                Die();
            
            attackCollider.SetActive(false);
            downAttackCollider.SetActive(false);
        }
    }
    protected virtual void Die()
    {
        
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
    void checkForInteractables()
    {
        float minInteractDistance = 1.5f;
        float lastPickupDistance = Mathf.Infinity;
        Interactable closestInteractable = null;

        foreach(Interactable interactable in Manager.Instance.interactables)
        {
            if(interactable.canInteract == false) return;
            
            if(interactable is PickupAble pickupAble)
                if(pickupAble.held) continue;

            Vector2 distance = transform.position - interactable.transform.position;

            if(distance.magnitude < minInteractDistance)
            {
                nearInteractable = true;
                if(closestInteractable == null || distance.magnitude < lastPickupDistance)
                {
                    closestInteractable = interactable;
                    lastPickupDistance = distance.magnitude;

                    if(lastClosestInteractable != null)
                    {
                        if(lastClosestInteractable != closestInteractable && isPlayer)
                            lastClosestInteractable.interactPrompt.SetActive(false);
                    }

                    lastClosestInteractable = closestInteractable;
                }
            }

            if(isPlayer && lastClosestInteractable != null)
                lastClosestInteractable.interactPrompt.SetActive(true);
        }

        if(closestInteractable == null && lastClosestInteractable != null)
        {
            if(isPlayer)
                lastClosestInteractable.interactPrompt.SetActive(false);
                
            lastClosestInteractable = null;
            nearInteractable = false;
        }
    }
    protected void InteractWithObject()
    {
        if(lastClosestInteractable == null) return;

        if(lastClosestInteractable is PickupAble pickupAble)
        {
            pickupAble.interactPrompt.SetActive(false);
            pickupAble.transform.position = hand.transform.position;
            pickupAble.transform.rotation = hand.transform.rotation;
            pickupAble.transform.SetParent(hand.transform);
            pickupAble.held = true;
            heldPickupable = pickupAble;
            lastClosestInteractable = null;
        }
        else
        {
            lastClosestInteractable.Interact();
        }
    }

    public void RemoveheldPickupable()
    {
        if(heldPickupable != null)
        {
            heldPickupable.transform.SetParent(null);
            heldPickupable = null;
        }
    }
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == hurtLayer && currentCharacterState != characterState.hurting)
        {
            float _x = Mathf.Sign(transform.position.x - collision.gameObject.transform.parent.transform.position.x);
            Vector2 hurtVec = new Vector2(_x,1);
            Hurt(hurtVec,damageToRecive);
        }
    }
}
