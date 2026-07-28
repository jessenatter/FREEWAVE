using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class Zombie : Enemy
{
    float hasTargetMoveSpeed = 1.5f, lookingForMeatMoveSpeed = .7f;
    float eatingCorpseDistance = 0.75f; //how close i have to be to a corpse to start eating it
    ZombieAnimator zombieAnimator;
    [SerializeField] GameObject bloodParticles, deathParticles,physicsLimb;
    enum zombieState
    {
        lookingForMeat, //patroling for corpses to eat or the player
        eating, //eating a corpse 
        hasTarget, //has player
    }
    zombieState currentZombieState = zombieState.lookingForMeat;
    protected override void Start()
    {    
        zombieAnimator = GetComponent<ZombieAnimator>();

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
            CheckForExitState();
        }
        else if(currentZombieState == zombieState.eating)
        {
            CheckForExitState();
        }
        else if(currentZombieState == zombieState.hasTarget)
        {
            moveSpeed = hasTargetMoveSpeed;
        }
    }

    void CheckForExitState()
    {
        if(hasTarget)
            currentZombieState = zombieState.hasTarget;
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void startChargingAttack()
    {
        base.startChargingAttack();

        //use custom zombie animator states for charging attack since base
        //class only has attack animation no attack charge
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

    protected override void Die(Vector2 dir)
    {
        Manager.Instance.enemies.Remove(this);

        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer sprite in sprites)
        {
            if(sprite.isVisible == false || Random.Range(0f,1f) > 0.5f) continue;

            GameObject _physicsLimb = Instantiate(physicsLimb,sprite.transform);
            _physicsLimb.GetComponent<SpriteRenderer>().sprite = sprite.sprite;
            _physicsLimb.transform.SetParent(null);

            float _scale = 1.25f;
            _physicsLimb.transform.localScale = new Vector2(_scale,_scale);

            float dieForce = 2f;
            _physicsLimb.GetComponent<Rigidbody2D>().AddForce(dieForce * dir,ForceMode2D.Impulse);
        }

        GameObject _deathParticles = Instantiate(deathParticles);
        _deathParticles.transform.position = transform.position;
        _deathParticles.transform.GetChild(0).GetComponent<SpriteRenderer>();

        base.Die(dir);
    }

    void SetDeadSprites()
    {
        
    }
}


