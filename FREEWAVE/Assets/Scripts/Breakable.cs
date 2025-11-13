using UnityEngine;

public class Breakable : MonoBehaviour
{
    [SerializeField] GameObject brokenObject;

    void Start()
    {
        
    }

    void Update()
    {

    }
    
    public void Break()
    {
        GameObject _brokenObject = Instantiate(brokenObject);
        _brokenObject.transform.position = transform.position;
        Destroy(gameObject);
    }
}
