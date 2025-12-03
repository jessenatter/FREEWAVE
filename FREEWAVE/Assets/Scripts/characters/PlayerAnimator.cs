using UnityEngine;

public class PlayerAnimator : CharacterAnimator
{
    public override void CharacterAnimatorStart()
    {
        base.CharacterAnimatorStart();

        upperBodyAttack.spine1rotation = new Vector2(10,-20);
        upperBodyAttack.spine2rotation = new Vector2(10,-10);

        upperBodyRun.spine1rotation = new Vector2(-5,-10);
        upperBodyRun.spine2rotation = new Vector2(-5,-10);
        upperBodyRun.headRotation = new Vector2(5,-5);

        upperBodyRun.rotationDuration = 20f;
        upperBodyRun.duration = 50f;
    }
}
