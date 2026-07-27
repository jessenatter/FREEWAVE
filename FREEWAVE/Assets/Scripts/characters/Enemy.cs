using UnityEngine;

public class Enemy : Character
{
    public GameObject target;
    Player player;

    float awarenessDistance = 5f;

    float attackDistance = 1f;

    public PublicTimer attackChargeTimer = new PublicTimer(50f);

    public bool hasPlayer,chargingAttack;

    PublicTimer awarenessTimer = new PublicTimer(1000f);

    protected override void Start()
    {
        base.Start();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        Vector2 playerEnemyVector = player.transform.position - transform.position;
    
        if(playerEnemyVector.magnitude < awarenessDistance)
        {
            hasPlayer = true;
            awarenessTimer.Reset();
        }
        else if(playerEnemyVector.magnitude > awarenessDistance * 3f)
        {
            if(awarenessTimer.TickLoop())
            {
                hasPlayer = false;
            }
        }

        if(playerEnemyVector.magnitude < attackDistance && currentCharacterState == characterState.movement)
            startChargingAttack();

        if(hasPlayer)
            target = player.gameObject;
        else
            target = null;

        Vector2 targetEnemyVector = Vector2.zero;

        if(target != null)
            targetEnemyVector = target.transform.position - transform.position;

        if(target != null)
            xInput = Mathf.Sign(targetEnemyVector.x);
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
            if(Manager.Instance.ship.rb.linearVelocity.magnitude < 5f) return;
            print("a");
            float shipForceModifier = 1f;
            rb.AddForce(Manager.Instance.ship.rb.linearVelocity * shipForceModifier,ForceMode2D.Impulse);
        }
        base.OnTriggerEnter2D(collision);
    }

    protected virtual void startChargingAttack()
    {
        if(currentCharacterState == characterState.attacking || currentCharacterState == characterState.hurting || chargingAttack) return;

        currentCharacterState = characterState.idle;
        rb.linearVelocity = Vector2.zero;
        chargingAttack = true;
        attackChargeTimer.Reset();
    }

    protected override void Hurt(Vector2 hurtDir, float damage)
    {
        base.Hurt(hurtDir, damage);
        attackChargeTimer.Reset();
        chargingAttack = false;
    }

    protected override void Die()
    {
        base.Die();
        Destroy(gameObject);
    }
}
