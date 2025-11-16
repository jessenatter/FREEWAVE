using UnityEngine;

public class LayerSwitcher : MonoBehaviour
{
    [SerializeField] int startLayer,endLayer;

    float changeLayerTimer = 100;

    float destroyTimer = 1000;

    [SerializeField] bool destroyOnSecondTimer;

    void Start()
    {
        gameObject.layer = startLayer;
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if(gameObject.layer == startLayer)
        {
            changeLayerTimer--;
            if(changeLayerTimer == 0)
            {
                gameObject.layer = endLayer;
            }
        }
        else if(destroyOnSecondTimer)
        {
            destroyTimer--;
            if(destroyTimer == 0)
                Destroy(gameObject);
        }
    }
}
