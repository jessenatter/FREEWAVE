using UnityEngine;

public class Breakable : MonoBehaviour
{
    [SerializeField] protected GameObject brokenObject;

    void Start()
    {
        
    }

    void Update()
    {

    }
    
    public virtual void Break()
    {
        GameObject _brokenObject = Instantiate(brokenObject);
        _brokenObject.transform.position = transform.position;
        Destroy(gameObject);
    }
}
