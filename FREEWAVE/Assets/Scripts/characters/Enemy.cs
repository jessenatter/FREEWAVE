using UnityEngine;

public class Enemy : Character
{
    public GameObject target;
    Player player;

    float awarenessDistance = 5f;

    float attackDistance = 1f;

    float attackChargeTimer = 50f,attackChargeTimerCurrent;

    public bool hasPlayer,chargingAttack;

    float awarenessTimer = 1000,awarenessTimerCurrent;

    protected override void Start()
    {
        base.Start();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        moveSpeed = 1.5f;
        jumpForce = 1.5f;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    protected override void Update()
    {
        base.Update();

        Vector2 playerEnemyVector = player.transform.position - transform.position;
    
        if(playerEnemyVector.magnitude < awarenessDistance)
        {
            hasPlayer = true;
            awarenessTimerCurrent = 0;
        }
        else if(playerEnemyVector.magnitude > awarenessDistance * 3f)
        {
            awarenessTimerCurrent++;
            if(awarenessTimerCurrent == awarenessTimer)
            {
                awarenessTimerCurrent = 0;
                hasPlayer = false;
            }
        }

        if(playerEnemyVector.magnitude < attackDistance)
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
            xInput = 0;
            attackChargeTimerCurrent++;
            if(attackChargeTimerCurrent == attackChargeTimer)
            {
                attackChargeTimerCurrent = 0;
                chargingAttack = false;
                Attack();
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        damageToRecive = player.damage;
        base.OnTriggerEnter2D(collision);
    }

    void startChargingAttack()
    {
        if(currentCharacterState != characterState.attacking && currentCharacterState != characterState.hurting)
        {
            chargingAttack = true;
        }
    }

    protected override void Die()
    {
        base.Die();
        Destroy(gameObject);
    }
}
