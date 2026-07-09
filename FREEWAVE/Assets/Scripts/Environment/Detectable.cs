using UnityEngine;

public class Detectable : MonoBehaviour
{
    void Start()
    {
        Manager.Instance.detectables.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
