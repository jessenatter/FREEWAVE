using System;
using UnityEngine;

public class sack : MonoBehaviour
{
    public bool stabbed = false;

    int playerColliderLayer = 11;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == playerColliderLayer)
        {
            stabbed = true;
            print("stabbed");
        }
    }
}
