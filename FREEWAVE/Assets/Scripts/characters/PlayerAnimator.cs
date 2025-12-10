using UnityEngine;

public class PlayerAnimator : CharacterAnimator
{
    public override void CharacterAnimatorStart()
    {
        base.CharacterAnimatorStart();

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

        //set rotation durations
        upperBodyIdle.rotationDuration = 100f;
        upperBodyRun.rotationDuration = 20f;
    }
}
