using UnityEngine;

public class healthKit : Interactable
{
    [SerializeField] float healthToAdd;
    public override void Interact()
    {
        base.Interact();

        Manager.Instance.player.health += healthToAdd;
        Manager.Instance.player.health = Mathf.Clamp(Manager.Instance.player.health,0,10);

        Manager.Instance.interactables.Remove(this);
        Destroy(gameObject);
    }
}
