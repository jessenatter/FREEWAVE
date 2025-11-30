using UnityEngine;

public class PlayerAnimator : CharacterAnimator
{
    protected override void Start()
    {
        base.Start();
        upperBodyAttack.spine1rotation = new Vector2(20,-30);
        upperBodyAttack.spine2rotation = new Vector2(20,-30);
    }
}
