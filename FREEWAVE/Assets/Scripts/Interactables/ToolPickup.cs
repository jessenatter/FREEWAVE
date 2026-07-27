using UnityEngine;

public class ToolPickup : Interactable
{
    [SerializeField] string weaponName;

    public override void Interact()
    {
        base.Interact();

        Player player = Manager.Instance.player;
        if(player.UnlockWeaponByName(weaponName))
        {
            canInteract = false;
            if(interactPrompt != null)
                interactPrompt.SetActive(false);

            Destroy(gameObject);
        }

    }
}
