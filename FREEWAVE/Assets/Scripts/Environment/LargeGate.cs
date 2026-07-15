using UnityEngine;

public class LargeGate : MonoBehaviour
{
    [SerializeField] KeyHoleLargeGate hole1,hole2;

    Animator animator;

    bool open = false;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!open && hole1.keyUsed && hole2.keyUsed)
        {
            open = true;
            animator.SetBool("open",true);
        }
    }
}
