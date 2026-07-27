using UnityEngine;

public class Zombie : Enemy
{
    PublicTimer lookingForMeatTimer = new PublicTimer(1000f);
    PublicTimer restTimer = new PublicTimer(500f);
    float hasPlayerMoveSpeed = 1.5f, lookingForMeatMoveSpeed = .7f;
    float eatingCorpseDistance = 0.75f;
    ZombieAnimator zombieAnimator;

    [SerializeField] GameObject bloodParticles, fleshParticles;
    enum zombieState
    {
        lookingForMeat,
        resting,
        eating,
        hasPlayer,
    }
    zombieState currentZombieState = zombieState.lookingForMeat;
    protected override void Start()
    {    
        zombieAnimator = GetComponent<ZombieAnimator>();

        lookingForMeatTimer.SetDuration(lookingForMeatTimer.Duration + Random.Range(-200,200));
        restTimer.SetDuration(restTimer.Duration + Random.Range(-200,200));
        damage = 1;

        moveSpeed = 1.5f;
        jumpForce = 1.5f;

        attackTimer.SetDuration(30f);
        attackCD.SetDuration(15f);
        knockbackForce = 3f;
        hurtTimer.SetDuration(30f);
        attackChargeTimer.SetDuration(20f);

        base.Start();
        Manager.Instance.enemies.Add(this);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if(currentZombieState == zombieState.lookingForMeat)
        {
            moveSpeed = lookingForMeatMoveSpeed;

            if(lookingForMeatTimer.TickLoop())
            {
                currentZombieState = zombieState.resting;
            }

            if(hasPlayer)
            {
                lookingForMeatTimer.Reset();
                currentZombieState = zombieState.hasPlayer;
            }
            
        }
        else if(currentZombieState == zombieState.resting)
        {
            xInput = 0;

            if(restTimer.TickLoop())
            {
                currentZombieState = zombieState.lookingForMeat;
            }

            if(hasPlayer)
            {
                lookingForMeatTimer.Reset();
                currentZombieState = zombieState.hasPlayer;
            }
        }
        else if(currentZombieState == zombieState.eating)
        {
            if(hasPlayer)
            {
                lookingForMeatTimer.Reset();
                currentZombieState = zombieState.hasPlayer;
            }
        }
        else if(currentZombieState == zombieState.hasPlayer)
        {
            moveSpeed = hasPlayerMoveSpeed;
        }
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void startChargingAttack()
    {
        base.startChargingAttack();

        zombieAnimator.currentUpperBodyState = zombieAnimator.chargeAttackUpper;
        zombieAnimator.currentLowerBodyState = zombieAnimator.chargeAttackLower;
    }

    protected override void Hurt(Vector2 hurtDir, float damage)
    {
        base.Hurt(hurtDir, damage);

        GameObject _blood = Instantiate(bloodParticles);
        _blood.transform.position = transform.position;
        _blood.transform.position += new Vector3(0,0.4f,0);
        _blood.transform.SetParent(transform);
    }

    protected override void Die()
    {
        base.Die();
        Manager.Instance.enemies.Remove(this);

        GameObject _blood = Instantiate(fleshParticles);
        _blood.transform.position = transform.position;
        _blood.transform.position += new Vector3(0,0.4f,0);
        //_blood.transform.SetParent(transform);
    }
}


