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
        if(player.heldObject == null) return;

        if(player.heldObject.gameObject.tag == "Key")
        {
            keyVisual.SetActive(true);
            GameObject usedKey = player.heldObject.gameObject;
            player.RemoveHeldObject();
            Destroy(usedKey);
            shield.SetActive(false);
            canInteract = false;
            interactPrompt.SetActive(false);
        }
    }
}
