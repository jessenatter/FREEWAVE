using UnityEngine;

public class Interactable : MonoBehaviour
{
    public GameObject interactPrompt;

    void Start()
    {
        Manager.Instance.interactables.Add(this);
        interactPrompt = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact()
    {
        
    }
}
