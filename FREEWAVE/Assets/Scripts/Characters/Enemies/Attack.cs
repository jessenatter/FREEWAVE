using UnityEngine;

[System.Serializable]
public class Attack //for enemies
{
    public float proximityToPlayer; //proximity to player in order to use this attack
    public float damage;
    public Vector2 movementInput;
    public CharacterAnimator.upperBodyState chargeUpperBodyState,attackUpperBodyState;
    public CharacterAnimator.lowerBodyState chargeLowerBodyState,attackLowerBodyState;
    public float knockbackForce;
    public float chargeDuration;
    public float attackDuration;
    
    public float attackChance = 0.65f;
    public Attack(
        float proximityToPlayer,
        float damage,
        float knockbackForce,
        Vector2 movementInput,
        CharacterAnimator.upperBodyState chargeUpperBodyState,
        CharacterAnimator.lowerBodyState chargeLowerBodyState,
        CharacterAnimator.upperBodyState attackUpperBodyState,
        CharacterAnimator.lowerBodyState attackLowerBodyState,
        float chargeDuration = 20f,
        float attackDuration = 30f,
        float attackChance = 0.65f)
    {
        this.proximityToPlayer = proximityToPlayer;
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        this.movementInput = movementInput;
        this.chargeUpperBodyState = chargeUpperBodyState;
        this.chargeLowerBodyState = chargeLowerBodyState;
        this.attackUpperBodyState = attackUpperBodyState;
        this.attackLowerBodyState = attackLowerBodyState;
        this.chargeDuration = chargeDuration;
        this.attackDuration = attackDuration;
        this.attackChance = attackChance;
    }

    public bool CanUse(float distanceToTarget)
    {
        return distanceToTarget <= proximityToPlayer;
    }

    public void ApplyChargeState(CharacterAnimator animator)
    {
        if(animator == null)
            return;

        if(chargeUpperBodyState != null)
            animator.currentUpperBodyState = chargeUpperBodyState;

        if(chargeLowerBodyState != null)
            animator.currentLowerBodyState = chargeLowerBodyState;
    }

    public void ApplyAttackState(CharacterAnimator animator)
    {
        if(animator == null)
            return;

        if(attackUpperBodyState != null)
            animator.currentUpperBodyState = attackUpperBodyState;

        if(attackLowerBodyState != null)
            animator.currentLowerBodyState = attackLowerBodyState;
    }
}