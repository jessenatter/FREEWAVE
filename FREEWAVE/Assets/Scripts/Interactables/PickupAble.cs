using UnityEngine;

public class PickupAble : Interactable
{
    public bool held = false;

    public override void Interact()
    {
        base.Interact();
        print("v");
    }
}
