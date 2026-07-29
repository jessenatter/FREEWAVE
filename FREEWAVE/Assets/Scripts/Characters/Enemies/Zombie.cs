using UnityEngine;

public class Zombie : Enemy
{
    ZombieAnimator zombieAnimator;
    [SerializeField] GameObject bloodParticles, deathParticles,physicsLimb;
    protected override void Start()
    {    
        zombieAnimator = GetComponent<ZombieAnimator>();

        moveSpeed = 1.5f;
        jumpForce = 1.5f;

        attackTimer.SetDuration(30f);
        attackCD.SetDuration(15f);
        hurtTimer.SetDuration(3f);
        attackChargeTimer.SetDuration(20f);

        base.Start();

        if(attacks == null || attacks.Length == 0)
        {
            attacks = new Attack[1];
            float defaultAttackRange = 1.2f;
            float defaultAttackKnockback = 25f;
            attacks[0] = new Attack(defaultAttackRange, damage, defaultAttackKnockback, Vector2.zero);
        }

        Attack primaryAttack = attacks[0];
        primaryAttack.proximityToPlayer = Mathf.Max(primaryAttack.proximityToPlayer, 1.2f);
        primaryAttack.damage = Mathf.Max(primaryAttack.damage, damage);
        primaryAttack.chargeDurationOverride = attackChargeTimer.Duration;

        primaryAttack.chargeUpperBodyState = zombieAnimator.chargeAttackUpper;
        primaryAttack.chargeLowerBodyState = zombieAnimator.chargeAttackLower;
        primaryAttack.attackUpperBodyState = zombieAnimator.upperBodyAttack;
        primaryAttack.attackLowerBodyState = zombieAnimator.lowerBodyAttack;

        Manager.Instance.enemies.Add(this);
    }

    protected override void Hurt(int hurtDir, float damage,float knockback)
    {
        base.Hurt(hurtDir, damage,knockback);

        GameObject _blood = Instantiate(bloodParticles);
        _blood.transform.position = transform.position;
        _blood.transform.position += new Vector3(0,0.4f,0);
        _blood.transform.SetParent(transform);
    }

    protected override void Die(int dir)
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
            _physicsLimb.GetComponent<Rigidbody2D>().AddForce(dieForce * new Vector2(dir,0.5f),ForceMode2D.Impulse);
        }

        GameObject _deathParticles = Instantiate(deathParticles);
        _deathParticles.transform.position = transform.position;
        _deathParticles.transform.GetChild(0).GetComponent<SpriteRenderer>();

        base.Die(dir);
    }
}


