using UnityEngine;

public class Interactable : MonoBehaviour
{
    [HideInInspector] public GameObject interactPrompt;

    [HideInInspector]public bool canInteract = true;

    void Start()
    {
        Manager.Instance.interactables.Add(this);
        interactPrompt = transform.GetChild(0).gameObject;
    }
    
    public virtual void Interact()
    {
        
    }
}
