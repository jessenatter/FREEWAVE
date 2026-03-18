using UnityEngine;

public class PickupAble : MonoBehaviour
{
    Manager manager;
    public GameObject pickupPrompt;    
    public bool held = false;
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        manager.pickupAbles.Add(this);
        pickupPrompt = transform.GetChild(0).gameObject;
    }
}
