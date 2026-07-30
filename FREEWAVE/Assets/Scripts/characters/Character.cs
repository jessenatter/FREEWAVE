using System;
using Unity.Mathematics;
using UnityEngine;

public class Character : MonoBehaviour
{
    //defines combat / movement behavior for characters
    protected float moveSpeed = 3.5f, jumpForce = 27.5f,dashAttackSpeed = 10f;
    protected float xInput,yInput,dashXinput;
    bool grounded,isJumping,canJump,recentlyIdle,canAttack = true;
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

    #endregion
    [HideInInspector] public bool characterIsActive,getAttackInput,groundedHit,nearInteractable;
    GameObject attackCollider,downAttackCollider;
    [SerializeField] int hurtLayer; //layer of the collider that can hurt this character
    CharacterAnimator characterAnimator;
    CharacterAnimator.lowerBodyState previousLowerBodyState;
    CharacterAnimator.upperBodyState previousUpperBodyState;
    [HideInInspector] public float health = 10,damage = 1,damageToRecive,knockbackForceToRecive;
    [SerializeField] protected GameObject backHand,frontHand; //for putting stuff in back and front hands
    [HideInInspector] public PickupAble heldPickupable;
    protected Interactable lastClosestInteractable;
    [SerializeField] bool isPlayer;
    PublicTimer recentlyIdleTimer = new PublicTimer(15f);

    protected float initGravityScale = 2.7f,downAttackGravityScale;
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
    }
    protected virtual void Update()
    {
        if(currentCharacterState == characterState.frozen) return;

        RaycastHit2D hit = Physics2D.BoxCast(transform.position, bc.size * 0.9f, 0, Vector2.down, 0.3f, groundLayer);

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
        CheckForInteractables();
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

        rb.linearVelocityX = xInput * moveSpeed;
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
        transform.position += Vector3.one * 0.0001f;
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
        if(currentCharacterState == characterState.hurting) return;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(hurtDir,.5f) * knockbackForce,ForceMode2D.Impulse);
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
        }
    }
    void DownAttackUpdate()
    {
        if(groundedHit)
        {
            currentCharacterState = characterState.movement;
            downAttackCollider.SetActive(false);
            rb.gravityScale = initGravityScale;
        }
    }
    void DashAttackUpdate()
    {
        if(dashAttackTimer.TickLoop())
        {
            currentCharacterState = characterState.movement;
            attackCollider.SetActive(false);
        }
        
        rb.linearVelocityX = dashXinput * dashAttackSpeed;
    }
    void HurtUpdate()
    {
        hurtTimer.Tick();
        if(hurtTimer.IsComplete)
        {
            if(groundedHit)
            {
                hurtTimer.Reset();
                currentCharacterState = characterState.movement;
            }
        }
    }
    void AttackCDupdate()
    {
        if(!canAttack && currentCharacterState != characterState.attacking)
        {
            if(attackCD.TickLoop())
                canAttack = true;
        }
    }
    void CheckForInteractables()
    {
        float minInteractDistance = 1.5f;
        float lastPickupDistance = Mathf.Infinity;
        int highestPriority = int.MinValue;
        Interactable closestInteractable = null;

        foreach(Interactable interactable in Manager.Instance.interactables)
        {
            if(interactable.canInteract == false) continue;
            
            if(interactable is PickupAble pickupAble)
            {
                if(heldPickupable != null) continue;
                if(pickupAble.held) continue;
            }

            Vector2 distance = transform.position - interactable.transform.position;
            float distanceMagnitude = distance.magnitude;

            if(distanceMagnitude < minInteractDistance)
            {
                nearInteractable = true;
                if(closestInteractable == null
                    || interactable.InteractPriority > highestPriority
                    || (interactable.InteractPriority == highestPriority && distanceMagnitude < lastPickupDistance))
                {
                    closestInteractable = interactable;
                    highestPriority = interactable.InteractPriority;
                    lastPickupDistance = distanceMagnitude;

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
            if(heldPickupable != null)
                return;
            
            OnPickup(pickupAble);
        }
        else
            OnInteract();
    }

    protected virtual void OnInteract()
    {
        lastClosestInteractable.Interact();
    }

    protected virtual void OnPickup(PickupAble pickupAble)
    {
        pickupAble.Pickup();
        pickupAble.interactPrompt.SetActive(false);
        pickupAble.transform.position = backHand.transform.position;
        pickupAble.transform.rotation = backHand.transform.rotation;
        pickupAble.transform.SetParent(backHand.transform);
        pickupAble.held = true;
        heldPickupable = pickupAble;
        lastClosestInteractable = null;
    }

    public void RemoveHeldPickupable()
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
            Hurt((int)_x,damageToRecive,knockbackForceToRecive);
        }
    }

    protected virtual void Land()
    {
        
    }
}
