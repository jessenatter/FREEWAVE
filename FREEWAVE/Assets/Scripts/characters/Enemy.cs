using UnityEngine;

public class Enemy : Character
{
    Player player;
    public GameObject target;
    float awarenessDistance = 5f; //how close i have to be to the player to follow
    float attackDistance = 1f; //how close to attack
    public PublicTimer attackChargeTimer = new PublicTimer(50f); //how long to charge the attack
    public bool hasTarget,chargingAttack;
    PublicTimer awarenessTimer = new PublicTimer(1000f);
    protected override void Start()
    {
        base.Start();

        player = Manager.Instance.player;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if(Manager.Instance.ship.currentShipState == Ship.ShipState.waitingForPlayer)
            target = player.gameObject;
        else
            target = Manager.Instance.ship.gameObject;

        Vector2 targetVector = target.transform.position - transform.position;
    
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
            float t = attackChargeTimer.Progress;

            xInput = 0;
            if(attackChargeTimer.TickLoop())
            {
                chargingAttack = false;
                currentCharacterState = characterState.movement;
                Attack();
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        damageToRecive = player.damage;
        if(collision.gameObject == Manager.Instance.ship.gameObject)
        {
            return;
        }
        base.OnTriggerEnter2D(collision);
    }

    protected virtual void startChargingAttack()
    {
        if(currentCharacterState == characterState.attacking || 
        currentCharacterState == characterState.hurting || chargingAttack) return;

        //stop moving, start charging attack
        currentCharacterState = characterState.idle;
        rb.linearVelocity = Vector2.zero;
        chargingAttack = true;
        attackChargeTimer.Reset();
    }

    protected override void Hurt(Vector2 hurtDir, float damage)
    {
        base.Hurt(hurtDir, damage);
        
        //cancel attack
        attackChargeTimer.Reset();
        chargingAttack = false;
        SoundManager.PlaySound(0.5f,0.2f,"stab1","stab2","stab3");
    }

    protected override void Die(Vector2 dir)
    {
        base.Die(dir);
        Destroy(gameObject);
    }
}
