using UnityEngine;

public class Character : PrimaryClass
{
    protected string name;
    public GameObject gameObject;
    Limb frontArm, backArm, frontLeg, backLeg;

    override public void Start()
    {
        gameObject = new GameObject(name);
    }

    override public void Update()
    {

    }
}

public class Player : Character
{
    public override void Start()
    {
        name = "Player";

        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }
}

public class Limb
{
    public enum mode
    {
        rest,
        circle,
        follow,
        parabola,
    }
}

public class BodyState
{

}

public class UpperBodyState : BodyState
{

}
