using UnityEngine;

public class ZombieAnimator : CharacterAnimator
{
    GameObject chargeAttack;

    public upperBodyState chargeAttackUpper;
    public lowerBodyState chargeAttackLower;

    Zombie zombie;

    public override void CharacterAnimatorStart()
    {
        base.CharacterAnimatorStart();

        zombie = GetComponent<Zombie>();

        //set rotations
        upperBodyIdle.spine2rotation = new Vector2(3,-3);
        upperBodyIdle.headRotation = new Vector2(3,-3);

        upperBodyRun.spine1rotation = new Vector2(-10,-15);

        upperBodyAttack.spine2rotation = new Vector2(5,-10);
        upperBodyAttack.spine1rotation = new Vector2(5,-10);

        upperBodyDashAttack.spine2rotation = new Vector2(5,-10);
        upperBodyDashAttack.spine1rotation = new Vector2(5,-10);

        upperBodyHurt.spine2rotation = new Vector2(20,20);
        upperBodyHurt.spine1rotation = new Vector2(10,10);

        //set rotation durations (attack is preset)
        upperBodyIdle.rotationDuration = 100f;
        upperBodyRun.rotationDuration = 20f;
        upperBodyHurt.rotationDuration = 20f;

        chargeAttack = animationObjectHolder.transform.GetChild(7).gameObject;
        
        //charge attack
        LimbManager.limbState _chargeAttackLower = new LimbManager.limbState(chargeAttack.transform.GetChild(0).gameObject,zombie.attackChargeTimer,false,frontLeg,quickTransitionTime,false);
        chargeAttackLower = new lowerBodyState(_chargeAttackLower,this);

        Vector2 upperBodySpine2HurtRotation = new Vector2(-10,10);
        LimbManager.limbState _chargeAttackUpper = new LimbManager.limbState(chargeAttack.transform.GetChild(1).gameObject,zombie.attackChargeTimer,false,frontArm,quickTransitionTime,false);
        chargeAttackUpper = new upperBodyState( _chargeAttackUpper,this,Vector2.zero,upperBodySpine2HurtRotation,Vector2.zero,false,zombie.attackChargeTimer);
    }
}
