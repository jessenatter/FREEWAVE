using UnityEngine;

public class KeyHoleLargeGate : Interactable
{
    public bool keyUsed = false;
    GameObject keyVisual;

    void Awake()
    {
        keyVisual = transform.GetChild(1).gameObject;
    }

    public override void Interact()
    {
        base.Interact();

        Player player = Manager.Instance.player;
        if(player.heldPickupable == null) return;

        if(player.heldPickupable.gameObject.tag == "Key2")
        {
            keyVisual.SetActive(true);
            GameObject usedKey = player.heldPickupable.gameObject;
            player.RemoveHeldPickupable();
            Destroy(usedKey);
            keyUsed = true;
            canInteract = false;
            interactPrompt.SetActive(false);
        }
    }
}
