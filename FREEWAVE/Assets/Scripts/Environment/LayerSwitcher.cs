using UnityEngine;

public class LayerSwitcher : MonoBehaviour
{
    [SerializeField] int startLayer,endLayer;
    [SerializeField] PublicTimer changeLayerTimer = new PublicTimer(100f);
    [SerializeField] PublicTimer destroyTimer = new PublicTimer(1000f);
    [SerializeField] bool destroyOnSecondTimer,waitForCollisionBeforeSwitching;
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
        if(!waitForCollisionBeforeSwitching || waitForCollisionBeforeSwitching && hasCollision)
        {
            print("aaa");
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
