using UnityEngine;

public class Zombie : Enemy
{
    ZombieAnimator zombieAnimator;
    [SerializeField] GameObject bloodParticles, deathParticles,physicsLimb;
    protected override void Start()
    {    
        zombieAnimator = GetComponent<ZombieAnimator>();

        moveSpeed = moveSpeed * Random.Range(1.00f,1.20f) * 0.8f;
        jumpForce = jumpForce * 1f;
        dashAttackSpeed = dashAttackSpeed * 0.6f;

        dashAttackTimer.SetDuration(20f);
        attackTimer.SetDuration(30f);
        attackCD.SetDuration(15f);
        hurtTimer.SetDuration(3f);
        attackChargeTimer.SetDuration(20f);
        groundedDistance = 0.05f;

        base.Start();

        if(attacks == null || attacks.Length == 0)
        {
            attacks = new Attack[2];
            float defaultAttackKnockback = 25f;
            attacks[0] = new Attack(1.2f, 4f, defaultAttackKnockback, Vector2.zero,zombieAnimator.chargeAttackUpper,
            zombieAnimator.chargeAttackLower,zombieAnimator.upperBodyAttack,zombieAnimator.lowerBodyAttack, 20f, 30f);

            attacks[1] = new Attack(2.2f, 1f, defaultAttackKnockback, new Vector2(1f,0f),zombieAnimator.chargeDashAttackUpper,
            zombieAnimator.chargeDashAttackLower,zombieAnimator.upperBodyAttack,zombieAnimator.lowerBodyAttack, 20f, 30f);
        }

        Manager.Instance.enemies.Add(this);
    }
    protected override void Update()
    {
        base.Update();

        print(groundedHit);
    }

    protected override void Hurt(int hurtDir, float damage,float knockback)
    {
        base.Hurt(hurtDir, damage,knockback);

        GameObject _blood = SpawnBlood(transform);
        _blood.transform.SetParent(transform);
    }

    GameObject SpawnBlood(Transform _transform)
    {
        GameObject _blood = Instantiate(bloodParticles);
        _blood.transform.position = _transform.position;
        _blood.transform.position += new Vector3(0,0.4f,0);
        _blood.transform.SetParent(null);

        return _blood;
    }

    protected override void Die(int dir)
    {
        Manager.Instance.enemies.Remove(this);

        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer sprite in sprites)
        {
            if(sprite.isVisible == false) continue;

            if (Random.Range(0f,1f) > 0.5f) //50 50 blood vs limb
            {
                SpawnBlood(sprite.transform);
            }
            else
            {
                GameObject _physicsLimb = Instantiate(physicsLimb,sprite.transform);
                _physicsLimb.GetComponent<SpriteRenderer>().sprite = sprite.sprite;
                _physicsLimb.transform.SetParent(null);

                float _scale = 1.25f;
                _physicsLimb.transform.localScale = new Vector2(_scale,_scale);

                float dieForce = Random.Range(1.5f,2f);
                _physicsLimb.GetComponent<Rigidbody2D>().AddForce(dieForce * new Vector2(dir,1f),ForceMode2D.Impulse);
            }
        }

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

    protected override void ExitHurtState()
    {
        base.ExitHurtState();
    }
}


