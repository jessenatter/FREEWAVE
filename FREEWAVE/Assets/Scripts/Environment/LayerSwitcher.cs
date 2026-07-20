using UnityEngine;

public class LayerSwitcher : MonoBehaviour
{
    [SerializeField] int endLayer;
    int startLayer;
    [SerializeField] PublicTimer changeLayerTimer = new PublicTimer(100f);
    [SerializeField] PublicTimer destroyTimer = new PublicTimer(1000f);
    [SerializeField] bool destroyOnSecondTimer,waitForCollisionBeforeSwitching;
    bool hasCollision;
    [SerializeField] int collisionLayer;

    void Start()
    {
        startLayer = gameObject.layer;
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if(!waitForCollisionBeforeSwitching || waitForCollisionBeforeSwitching && hasCollision)
        {
            if(gameObject.layer == startLayer)
            {
                if(changeLayerTimer.Tick())
                {
                    gameObject.layer = endLayer;
                }
            }
            else if(destroyOnSecondTimer) //else meaning ive changed layers now waiting to be destroyed
            {
                if(destroyTimer.Tick())
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
