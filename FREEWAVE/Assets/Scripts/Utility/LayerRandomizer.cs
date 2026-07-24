using UnityEngine;

public class LayerRandomizer : MonoBehaviour
{
    void Start()
    {
        GetComponent<SpriteRenderer>().sortingOrder = 
        GetComponent<SpriteRenderer>().sortingOrder - Random.Range(0,5000);
    }
}
