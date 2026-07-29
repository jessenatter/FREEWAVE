using UnityEngine;

public class Enemy : Character
{
    Player player;
    protected CharacterAnimator enemyAnimator;
    public GameObject target;
    float awarenessDistance = 5f; //how close i have to be to the player to follow
    [SerializeField] protected Attack[] attacks;
    public PublicTimer attackChargeTimer = new PublicTimer(50f); //how long to charge the attack
    public bool hasTarget,chargingAttack;
    public Attack currentAttack;
    PublicTimer awarenessTimer = new PublicTimer(1000f);
    protected override void Start()
    {
        base.Start();

        player = Manager.Instance.player;
        enemyAnimator = GetComponent<CharacterAnimator>();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

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

        if(target != null)
            xInput = Mathf.Sign(targetVector.x);
        else
            xInput = 0;

        if(chargingAttack)
        {
            xInput = 0;
            if(attackChargeTimer.TickLoop())
            {
                chargingAttack = false;
                currentCharacterState = characterState.movement;
                Attack();
            }
        }
        else if(hasTarget)
        {
            Attack nextAttack = SelectAttack(targetDistance);
            if(nextAttack != null)
                StartChargingAttack(nextAttack);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 10)
        {
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
        if(attack == null)
            return;

        if(currentCharacterState == characterState.attacking || 
        currentCharacterState == characterState.hurting || chargingAttack || currentCharacterState == characterState.dead) return;

        //stop moving, start charging attack
        currentAttack = attack;
        attackChargeTimer.SetDuration(currentAttack.ResolveChargeDuration(attackChargeTimer.Duration));
        currentCharacterState = characterState.idle;
        rb.linearVelocity = Vector2.zero;
        chargingAttack = true;
        attackChargeTimer.Reset();
        currentAttack.ApplyChargeState(enemyAnimator);
    }

    protected override void Attack()
    {
        if(currentAttack != null)
        {
            if(currentAttack.damage > 0f)
                damage = currentAttack.damage;

            currentAttack.ApplyAttackState(enemyAnimator);
        }

        base.Attack();
    }

    Attack SelectAttack(float targetDistance)
    {
        if(attacks == null || attacks.Length == 0)
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
        base.Hurt(hurtDir, damage,knockback);
        
        //cancel attack
        attackChargeTimer.Reset();
        chargingAttack = false;
        //currentAttack = null;
        SoundManager.PlaySound(0.5f,0.2f,"stab1","stab2","stab3");
        HapticsManager.PlayMedium(0.2f);
    }

    protected override void Die(int dir)
    {
        base.Die(dir);
        Destroy(gameObject);
    }
}
