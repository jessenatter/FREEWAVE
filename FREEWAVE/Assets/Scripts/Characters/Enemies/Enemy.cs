using UnityEngine;

public class Enemy : Character
{
    Player player;
    protected CharacterAnimator enemyAnimator;
    [HideInInspector] public GameObject target;
    float awarenessDistance = 4f; //how close i have to be to the player to follow
    [HideInInspector] public Attack[] attacks;
    [HideInInspector] public bool hasTarget,chargingAttack;
    [HideInInspector] public Attack currentAttack;
    int queuedAttackDirection;
    bool hasQueuedAttackInput;
    [HideInInspector] public PublicTimer attackChargeTimer = new PublicTimer(50f); //how long to charge the attack
    PublicTimer awarenessTimer = new PublicTimer(500f); //how long to loose the player
    protected float attackChanceRollInterval = 8f; //min value is 1 
    protected PublicTimer attackChanceRollTimer = new PublicTimer(8f);

    [HideInInspector] public float attackChargeDuration = 50f,dashAttackChargeDuration = 70f;
    [HideInInspector] public float attackDuration = 15f,dashAttackDuration = 30f;
    protected override void Start()
    {
        postHitInvincibilitySeconds = 0.5f;
        base.Start();
        
        attackChanceRollTimer.SetDuration(attackChanceRollInterval);

        player = Manager.Instance.player;
        enemyAnimator = GetComponent<CharacterAnimator>();
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        //return;
        #region //follow target

        if(Manager.Instance.ship.currentShipState == Ship.ShipState.waitingForPlayer)
            target = player.gameObject;
        else
            target = Manager.Instance.ship.gameObject;

        Vector2 targetVector = target.transform.position - transform.position;
        float targetDistance = targetVector.magnitude;
    
        if(targetVector.magnitude < awarenessDistance)
        {
            hasTarget = true;
            awarenessTimer.Reset(); //reset awareness timer
        }
        else if(targetVector.magnitude > awarenessDistance * 2f)
        {
            if(awarenessTimer.TickLoop()) //if not in proximity of player for long enough, loose them
                hasTarget = false;
        }

        if(target != null && groundedHit)
            xInput = Mathf.Sign(targetVector.x); //move towards player/ship if grounded
        else
            xInput = 0;
        
        #endregion 

        #region //attack logic

        if(chargingAttack)
        {
            if(attackChargeTimer.TickLoop())
            {
                chargingAttack = false;
                currentCharacterState = characterState.movement;
                queuedAttackDirection = (int)currentAttack.movementInput.x;
                hasQueuedAttackInput = true;
            }

            if(groundedHit)
                rb.linearVelocityY = 0;
        }
        else if(hasTarget) //if i have a target, see if im close enough to attack
        {
            //fine some way  to make this fixx work
            if(currentCharacterState == characterState.idle)
                currentCharacterState = characterState.movement;
                
            Attack nextAttack = SelectAttack(targetDistance); //try to see if there is a vald attack for my position
            if(nextAttack != null)
                StartChargingAttack(nextAttack); //if there is then start charging it 
        }

        if(hasQueuedAttackInput && canAttack)
        {
            hasQueuedAttackInput = false;
            GiveAttackInput(queuedAttackDirection);
        }

        #endregion
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 10)
        {
            //set values for getting hurt
            damageToRecive = player.currentMelee.damage;
            knockbackForceToRecive = player.currentMelee.knockbackForce;
        }

        if(collision.gameObject == Manager.Instance.ship.gameObject)
        {
            return;
        }
        base.OnTriggerEnter2D(collision);
    }
    protected virtual void StartChargingAttack(Attack attack)
    {
        if(currentCharacterState != characterState.movement) return;

        //stop moving, start charging attack
        hasQueuedAttackInput = false;
        currentAttack = attack;
        currentCharacterState = characterState.idle;
        rb.linearVelocity = Vector2.zero;
        chargingAttack = true;
        attackChargeTimer.SetDuration(currentAttack.chargeDuration);
        attackChargeTimer.Reset();
        currentAttack.ApplyChargeState(enemyAnimator);
    }
    void GiveAttackInput(int _xInput)
    {
        currentAttack.ApplyAttackState(enemyAnimator);
        attackTimer.SetDuration(currentAttack.attackDuration);
        attackTimer.Reset();
        getAttackInput = true;
        xInput = Mathf.Abs(_xInput) * Mathf.Sign(transform.localScale.x);
    }
    protected virtual Attack SelectAttack(float targetDistance)
    {
        if(!attackChanceRollTimer.TickLoop())
            return null;

        Attack closestValidAttack = null;
        float closestRange = float.MaxValue;

        for(int i = 0; i < attacks.Length; i++)
        {
            Attack attack = attacks[i];
            if(attack == null)
                continue;

            if(!attack.CanUse(targetDistance))
                continue;

            if(Random.Range(0f,1f) > attack.attackChance)
                continue;

            if(attack.proximityToPlayer < closestRange)
            {
                closestRange = attack.proximityToPlayer;
                closestValidAttack = attack;
            }
        }

        return closestValidAttack;
    }
    protected override void Hurt(int hurtDir, float damage,float knockback)
    {
        //cancel attack charge
        hasQueuedAttackInput = false;
        attackChargeTimer.Reset();
        chargingAttack = false;
        //currentAttack = null;
        SoundManager.PlaySound(0.5f,0.2f,"stab1","stab2","stab3");
        HapticsManager.PlayMedium(0.2f);

        base.Hurt(hurtDir, damage,knockback);
    }
    protected override void Die(int dir)
    {
        base.Die(dir);
        Destroy(gameObject);
    }
}
