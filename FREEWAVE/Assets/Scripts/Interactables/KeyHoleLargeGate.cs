using UnityEngine;

public class KeyHoleLargeGate : Interactable
{
    public bool keyUsed =false;
    GameObject keyVisual;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        keyVisual = transform.parent.transform.GetChild(1).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
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
            player.RemoveheldPickupable();
            Destroy(usedKey);
            keyUsed = true;
            canInteract = false;
            interactPrompt.SetActive(false);
        }
    }
}
