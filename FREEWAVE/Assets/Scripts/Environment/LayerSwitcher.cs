using UnityEngine;

public class LayerSwitcher : MonoBehaviour
{
    [SerializeField] int startLayer,endLayer;
    [SerializeField]float changeLayerTimer = 100;
    [SerializeField]float destroyTimer = 1000;
    [SerializeField] bool destroyOnSecondTimer,waitForCollision;
    bool hasCollision;
    [SerializeField] int collisionLayer;

    void Start()
    {
        gameObject.layer = startLayer;
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if(!waitForCollision || waitForCollision && hasCollision)
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.layer == collisionLayer)
            hasCollision = true; 
    }
}
