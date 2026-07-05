using UnityEngine;

public class InteractableBehavior
{
    protected float distanceToPickup = 0.3f;
    protected bool canInteract;

    public virtual void InteractableUpdate(GameObject gameObject)
    {
        if (Vector2.Distance(gameObject.transform.position, GameData.manager.player.gameObject.transform.position) < distanceToPickup)
            canInteract = true;
        else
            canInteract = false;

        if (canInteract && (GameData.manager.grabAction.IsPressed()))
            Interact();
    }

    public virtual void Interact()
    {

    }
}
