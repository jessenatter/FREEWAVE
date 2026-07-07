using UnityEngine;

public class Zombie : Enemy
{
    float lookingForMeatTimer = 1000,restTimer = 500;
    float lookingForMeatTimerCurrent,restTimerCurrent;
    float hasPlayerMoveSpeed = 1.5f, lookingForMeatMoveSpeed = .7f;
    float eatingCorpseDistance = 0.75f;
    ZombieAnimator zombieAnimator;
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

        lookingForMeatTimer += Random.Range(-200,200);
        restTimer += Random.Range(-200,200);
        damage = 1;

        moveSpeed = 1.5f;
        jumpForce = 1.5f;

        attackTimer = 30;
        attackCD = 15;
        knockbackForce = 3f;
        hurtTimer = 30f;
        attackChargeTimer = 20f;

        base.Start();
        manager.enemies.Add(this);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if(currentZombieState == zombieState.lookingForMeat)
        {
            moveSpeed = lookingForMeatMoveSpeed;

            lookingForMeatTimerCurrent++;
            if(lookingForMeatTimerCurrent == lookingForMeatTimer)
            {
                lookingForMeatTimerCurrent = 0;
                currentZombieState = zombieState.resting;
            }

            if(hasPlayer)
            {
                lookingForMeatTimerCurrent = 0;
                currentZombieState = zombieState.hasPlayer;
            }
            
        }
        else if(currentZombieState == zombieState.resting)
        {
            xInput = 0;

            restTimerCurrent++;
            if(restTimerCurrent == restTimer)
            {
                restTimerCurrent = 0;
                currentZombieState = zombieState.lookingForMeat;
            }

            if(hasPlayer)
            {
                lookingForMeatTimerCurrent = 0;
                currentZombieState = zombieState.hasPlayer;
            }
        }
        else if(currentZombieState == zombieState.eating)
        {
            if(hasPlayer)
            {
                lookingForMeatTimerCurrent = 0;
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

    protected override void Die()
    {
        base.Die();
        manager.enemies.Remove(this);
    }
}


