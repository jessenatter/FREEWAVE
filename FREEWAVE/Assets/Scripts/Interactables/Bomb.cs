using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class Bomb : PickupAble
{
    Rigidbody2D rb;
    BoxCollider2D bc;
    PublicTimer bombTimer = new PublicTimer(3f);
    float throwForce = 3f;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        
    }
    public override void Pickup()
    {
        base.Pickup();
        print("a");
        bc.enabled = false;
        rb.simulated = false;
        print(bc.enabled);
    }

    public void Throw(int dir)
    {
        bc.enabled = true;
        rb.simulated = true;
        Vector2 throwDir = new Vector2(dir,1);
        rb.AddForce(throwDir * throwForce,ForceMode2D.Impulse);
    }
}
