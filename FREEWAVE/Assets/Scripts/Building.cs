using UnityEngine;

public class Building : MonoBehaviour
{
    bool playerInBuilding,shipInBuilding;

    float alpha = 1;
    SpriteRenderer sr;

    float lerpSpeed = 3f;
    void Start()
    {
        sr = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float targetAlpha = 1;

        if(shipInBuilding || playerInBuilding)
            targetAlpha = 0;

        alpha = Mathf.Lerp(alpha,targetAlpha,Time.deltaTime * lerpSpeed);
        sr.color = new Color(1,1,1,alpha);
    }

    void FixedUpdate()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
            playerInBuilding = true;
        else if(collision.tag == "Ship")
            shipInBuilding = true;
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Player")
            playerInBuilding = false;
        else if(collision.tag == "Ship")
            shipInBuilding = false;
    }
}
