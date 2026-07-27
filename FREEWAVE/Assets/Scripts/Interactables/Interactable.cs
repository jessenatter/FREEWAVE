using UnityEngine;

public class Interactable : MonoBehaviour
{
    [HideInInspector] public GameObject interactPrompt;

    [HideInInspector]public bool canInteract = true;

    [SerializeField] int interactPriority = 0;
    public int InteractPriority => interactPriority;

    void Start()
    {
        Manager.Instance.interactables.Add(this);
        interactPrompt = transform.GetChild(0).gameObject;

        Vector2 initPos = interactPrompt.transform.position;

        interactPrompt.transform.SetParent(null);
        float size = 3f;
        interactPrompt.transform.localScale = new Vector3(size,size,size);
        interactPrompt.transform.SetParent(this.transform);
        interactPrompt.transform.position = initPos;
        interactPrompt.transform.SetAsFirstSibling();

        interactPrompt.SetActive(false);
    }
    
    public virtual void Interact()
    {
        
    }
}
