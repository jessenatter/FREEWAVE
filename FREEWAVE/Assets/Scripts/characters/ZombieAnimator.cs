using UnityEngine;

public class ZombieAnimator : CharacterAnimator
{
    GameObject chargeAttack;
    public float chargeAttackTime;

    public upperBodyState chargeAttackUpper;
    public lowerBodyState chargeAttackLower;

    public override void CharacterAnimatorStart()
    {
        base.CharacterAnimatorStart();

        upperBodyAttack.spine1rotation = new Vector2(10,-40);
        upperBodyAttack.spine2rotation = new Vector2(10,-30);

        upperBodyRun.spine1rotation = new Vector2(-5,-10);
        upperBodyRun.spine2rotation = new Vector2(-5,-10);
        upperBodyRun.headRotation = new Vector2(5,-5);

        upperBodyRun.rotationDuration = 20f;
        upperBodyRun.duration = 50f;

        chargeAttack = animationObjectHolder.transform.GetChild(7).gameObject;
        //charge attack
        LimbManager.limbState _chargeAttackLower = new LimbManager.limbState(chargeAttack.transform.GetChild(0).gameObject,chargeAttackTime,false,frontLeg,chargeAttackTime);
        chargeAttackLower = new lowerBodyState(_chargeAttackLower,_chargeAttackLower,this,chargeAttackTime);

        Vector2 upperBodySpine2HurtRotation = new Vector2(-10,10);
        LimbManager.limbState _chargeAttackUpper = new LimbManager.limbState(chargeAttack.transform.GetChild(1).gameObject,chargeAttackTime,false,frontArm,chargeAttackTime);
        chargeAttackUpper = new upperBodyState( _chargeAttackUpper, _chargeAttackUpper,this,Vector2.zero,upperBodySpine2HurtRotation,Vector2.zero,chargeAttackTime,false,chargeAttackTime);
    }
}
