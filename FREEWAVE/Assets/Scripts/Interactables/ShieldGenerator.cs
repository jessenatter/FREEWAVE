using UnityEngine;

public class ShieldGenerator : Interactable
{
    GameObject shield,keyVisual;
    //using awake not start to not cancel out start 
    void Awake()
    {
        shield = transform.GetChild(1).gameObject;
        keyVisual = transform.GetChild(2).gameObject;
    }

    public override void Interact()
    {
        base.Interact();

        Player player = Manager.Instance.player;
        if(player.heldPickupable == null) return;

        if(player.heldPickupable.gameObject.tag == "Key")
        {
            keyVisual.SetActive(true);
            GameObject usedKey = player.heldPickupable.gameObject;
            player.RemoveheldPickupable();
            Destroy(usedKey);
            shield.SetActive(false);
            canInteract = false;
            interactPrompt.SetActive(false);
        }
    }
}
