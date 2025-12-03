using UnityEngine;

public class ZombieAnimator : CharacterAnimator
{
    [SerializeField] GameObject chargeAttack;
    public float chargeAttackTime;

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

        //charge attack
        LimbManager.limbState _hurt = new LimbManager.limbState(chargeAttack.transform.GetChild(0).gameObject,0.1f,false,frontLeg,chargeAttackTime);
        lowerBodyHurt = new lowerBodyState(_hurt,_hurt,this,1);

        Vector2 upperBodySpine2HurtRotation = new Vector2(30,30);
        LimbManager.limbState _upperHurt = new LimbManager.limbState(chargeAttack.transform.GetChild(1).gameObject,0.1f,false,frontArm,chargeAttackTime);
        upperBodyHurt = new upperBodyState(_upperHurt,_upperHurt,this,Vector2.zero,upperBodySpine2HurtRotation,Vector2.zero,1,false,hurtStateDuration);
    }
}
