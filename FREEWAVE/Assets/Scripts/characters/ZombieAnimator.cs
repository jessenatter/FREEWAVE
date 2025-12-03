using UnityEngine;

public class ZombieAnimator : CharacterAnimator
{
    [SerializeField] GameObject chargeAttack;
    public float chargeAttackTime;
    protected override void Start()
    {
        base.Start();
        //charge attack
        LimbManager.limbState _hurt = new LimbManager.limbState(chargeAttack.transform.GetChild(0).gameObject,0.1f,false,frontLeg,chargeAttackTime);
        lowerBodyHurt = new lowerBodyState(_hurt,_hurt,this,1);

        Vector2 upperBodySpine2HurtRotation = new Vector2(30,30);
        LimbManager.limbState _upperHurt = new LimbManager.limbState(chargeAttack.transform.GetChild(1).gameObject,0.1f,false,frontArm,chargeAttackTime);
        upperBodyHurt = new upperBodyState(_upperHurt,_upperHurt,this,Vector2.zero,upperBodySpine2HurtRotation,Vector2.zero,1,false);
    }
}
