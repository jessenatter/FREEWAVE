using UnityEngine;

public class BreakableWall : Breakable
{
    override public void Break()
    {
        base.Break();
        brokenObject.GetComponent<Wall>().wallColor = GetComponent<Wall>().wallColor;
    }
}
