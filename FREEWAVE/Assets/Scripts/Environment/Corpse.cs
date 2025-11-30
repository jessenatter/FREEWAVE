using UnityEngine;

public class Corpse : MonoBehaviour
{
    Manager manager;
    public float health = 100;
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        manager.corpses.Add(this.gameObject);
    }
}
