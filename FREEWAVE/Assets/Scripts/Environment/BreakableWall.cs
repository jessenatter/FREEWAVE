using UnityEngine;

public class BreakableWall : Breakable
{
    override public void Break()
    {
        brokenObject.GetComponent<Wall>().wallColor = GetComponent<Wall>().wallColor;
        base.Break();
    }
}
