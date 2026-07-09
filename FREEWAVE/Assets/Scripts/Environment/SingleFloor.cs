using System.Collections.Generic;
using UnityEngine;

public class SingleFloor : MonoBehaviour
{
    bool playerInBuilding, shipInBuilding;
    float alpha = 1;
    SpriteRenderer sr;
    float lerpSpeed = 5f;
    GameObject spawnPos1,spawnPos2,lampSpawnPos;
    [SerializeField] List<GameObject> spawnObjects = new List<GameObject>();
    [SerializeField] List<GameObject> lamps = new List<GameObject>();
    void Start()
    {
        sr = transform.GetChild(0).GetComponent<SpriteRenderer>();

        spawnPos1 = transform.GetChild(5).gameObject;
        spawnPos2 = transform.GetChild(6).gameObject;
        lampSpawnPos = transform.GetChild(7).gameObject;

        int i = Random.Range(0,spawnObjects.Count);
        int j = Random.Range(0,spawnObjects.Count);
        int k = Random.Range(0,lamps.Count);

        GameObject _object1 = Instantiate(spawnObjects[i]);
        GameObject _object2 = Instantiate(spawnObjects[j]);
        GameObject _object3 = Instantiate(lamps[k]);

        _object1.transform.position = spawnPos1.transform.position;
        _object2.transform.position = spawnPos2.transform.position;
        _object3.transform.position = lampSpawnPos.transform.position;

        _object3.transform.SetParent(this.transform);
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
