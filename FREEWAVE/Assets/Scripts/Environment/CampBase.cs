using UnityEngine;

public class CampBase : Interactable
{
    Animator animator;
    bool open = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
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
        }
    }
}
