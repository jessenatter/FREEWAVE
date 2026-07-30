using UnityEngine;

public class CampBase : Interactable
{
    Animator animator;
    bool open = false;
    
    [SerializeField] Interactable insideInteractable; //if there is something inside of the camp

    void Awake()
    {
        animator = GetComponent<Animator>();
        if(insideInteractable != null)
            insideInteractable.canInteract = false;
    }
    public override void Interact()
    {
        base.Interact();

        if(open)
        {
            open = false;
            animator.SetBool("open", false);
        }
        else
        {
            open = true;
            animator.SetBool("open",true);
            SoundManager.PlaySound("base",0.5f,0);
            if(insideInteractable != null)
                insideInteractable.canInteract = true;
            
            //maybe will want to change this
            canInteract = false;
        }
    }
}
