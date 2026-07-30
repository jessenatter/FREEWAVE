using UnityEngine;

public class Zombie : Enemy
{
    ZombieAnimator zombieAnimator;
    [SerializeField] [Range(0f,1f)] float attackChanceAtCorrectDistance = 0.65f;
    [SerializeField] [Min(1f)] float attackChanceRollInterval = 8f;
    PublicTimer attackChanceRollTimer = new PublicTimer(8f);
    [SerializeField] GameObject bloodParticles, deathParticles,physicsLimb;
    protected override void Start()
    {    
        zombieAnimator = GetComponent<ZombieAnimator>();
        attackChanceRollTimer.SetDuration(attackChanceRollInterval);

        moveSpeed = moveSpeed * Random.Range(1.00f,1.20f);
        jumpForce = jumpForce * 1f;
        dashAttackSpeed = dashAttackSpeed * 1f;

        dashAttackTimer.SetDuration(20f);
        attackTimer.SetDuration(30f);
        attackCD.SetDuration(15f);
        hurtTimer.SetDuration(3f);
        attackChargeTimer.SetDuration(20f);

        base.Start();

        if(attacks == null || attacks.Length == 0)
        {
            attacks = new Attack[2];
            float defaultAttackKnockback = 25f;
            attacks[0] = new Attack(1.2f, damage, defaultAttackKnockback, Vector2.zero,zombieAnimator.chargeAttackUpper,
            zombieAnimator.chargeAttackLower,zombieAnimator.upperBodyAttack,zombieAnimator.lowerBodyAttack);

            attacks[1] = new Attack(2.2f, damage, defaultAttackKnockback, new Vector2(1f,0f),zombieAnimator.chargeAttackUpper,
            zombieAnimator.chargeAttackLower,zombieAnimator.upperBodyAttack,zombieAnimator.lowerBodyAttack);
        }

        Manager.Instance.enemies.Add(this);
    }

    protected override Attack SelectAttack(float targetDistance)
    {
        Attack selectedAttack = base.SelectAttack(targetDistance);
        if(selectedAttack == null)
            return null;

        // Roll at an interval so chance is meaningful and not evaluated every physics frame.
        if(!attackChanceRollTimer.TickLoop())
            return null;

        if(Random.Range(0f,1f) > attackChanceAtCorrectDistance)
            return null;

        return selectedAttack;
    }

    protected override void Update()
    {
        base.Update();

        print(currentCharacterState);
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


        //GameObject _deathParticles = Instantiate(deathParticles);
        //_deathParticles.transform.position = transform.position;
        //_deathParticles.transform.GetChild(0).GetComponent<SpriteRenderer>();

        base.Die(dir);
    }

    protected override void Attack()
    {
        base.Attack();

        //SoundManager.PlaySound(0.6f,0.25f,"zombieVoice1","zombieVoice2");
    }

    protected override void DashAttack()
    {
        base.DashAttack();

        //print("a");
        //SoundManager.PlaySound(0.6f,0.25f,"zombieVoice1","zombieVoice2");
    }
}


