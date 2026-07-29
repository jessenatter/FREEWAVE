using UnityEngine;

[System.Serializable]
public class Attack //for enemies
{
    public float proximityToPlayer; //proximity to player in order to use this attack
    public float damage;
    public Vector2 movementInput;
    public CharacterAnimator.upperBodyState chargeUpperBodyState,attackUpperBodyState;
    public CharacterAnimator.lowerBodyState chargeLowerBodyState,attackLowerBodyState;
    public float chargeDurationOverride;

    public float knockbackForce;
    public Attack(float proximityToPlayer, float damage, float knockbackForce, Vector2 movementInput)
    {
        this.proximityToPlayer = proximityToPlayer;
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        this.movementInput = movementInput;
        chargeDurationOverride = 0f;
    }

    public Attack(
        float proximityToPlayer,
        float damage,
        float knockbackForce,
        Vector2 movementInput,
        float chargeDurationOverride,
        CharacterAnimator.upperBodyState chargeUpperBodyState,
        CharacterAnimator.lowerBodyState chargeLowerBodyState,
        CharacterAnimator.upperBodyState attackUpperBodyState,
        CharacterAnimator.lowerBodyState attackLowerBodyState)
    {
        this.proximityToPlayer = proximityToPlayer;
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        this.movementInput = movementInput;
        this.chargeDurationOverride = chargeDurationOverride;
        this.chargeUpperBodyState = chargeUpperBodyState;
        this.chargeLowerBodyState = chargeLowerBodyState;
        this.attackUpperBodyState = attackUpperBodyState;
        this.attackLowerBodyState = attackLowerBodyState;
    }

    public bool CanUse(float distanceToTarget)
    {
        return distanceToTarget <= proximityToPlayer;
    }

    public float ResolveChargeDuration(float fallback)
    {
        if(chargeDurationOverride > 0f)
            return chargeDurationOverride;

        if(chargeUpperBodyState != null && chargeUpperBodyState.duration > 0f)
            return chargeUpperBodyState.duration;

        return fallback;
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