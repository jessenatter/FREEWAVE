using UnityEngine;

public class LayerSwitcher : MonoBehaviour
{
    [SerializeField] int startLayer,endLayer;
    [SerializeField] PublicTimer changeLayerTimer = new PublicTimer(100f);
    [SerializeField] PublicTimer destroyTimer = new PublicTimer(1000f);
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
                if(changeLayerTimer.Tick())
                {
                    gameObject.layer = endLayer;
                }
            }
            else if(destroyOnSecondTimer)
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
