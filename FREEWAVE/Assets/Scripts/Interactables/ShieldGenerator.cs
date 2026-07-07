using UnityEngine;

public class ShieldGenerator : Interactable
{
    GameObject shield,keyVisual;
    void Start()
    {
        shield = transform.GetChild(1).gameObject;
        keyVisual = transform.GetChild(2).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Interact()
    {
        base.Interact();

        Player player = Manager.Instance.player;
        if(player.heldObject.gameObject.tag == "Key")
        {
            keyVisual.SetActive(true);
            GameObject usedKey = player.heldObject.gameObject;
            player.RemoveHeldObject();
            Destroy(usedKey);
        }
    }
}
