using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class BuildingMaker : MonoBehaviour
{
    [SerializeField] GameObject ApartmentType;
    GameObject prevApartmentType;

    [SerializeField] int apartmentQuantity;
    int prevApartmentQuantity = 0;

    float heightMultiplier = 3.111f;
   
    float rotationInit;

    [SerializeField] bool gameStarted = false;
    [SerializeField] bool reset;

    [SerializeField] int targetSpriteLayer;
    int lastSpriteLayer;

    int prevApartmentIndex;

    // Start is called before the first frame update
    void Start()
    {
        gameStarted = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameStarted)
        {
            if (prevApartmentQuantity != apartmentQuantity)
            {
                Run();
                prevApartmentQuantity = apartmentQuantity;
            }
            else if(reset == true)
            {
                Run();
                reset = false;
            }
            else if(ApartmentType != prevApartmentType)
            {
                Run();
                prevApartmentType = ApartmentType;
            }
            else if(lastSpriteLayer != targetSpriteLayer)
            {
                Run();
                lastSpriteLayer = targetSpriteLayer;
            }
        }
    }


    void Run()
    {
        rotationInit = transform.rotation.eulerAngles.z;
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // Clear existing children
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < apartmentQuantity; i++)
        {
            float _y = heightMultiplier / 2 + (transform.position.y + (i * heightMultiplier));
            Vector3 spawnPosition = new Vector3(transform.position.x, _y, transform.position.z);

            GameObject building = Instantiate(ApartmentType);
            building.transform.position = spawnPosition;
            building.transform.rotation = transform.rotation;
            building.transform.SetParent(transform);
            building.GetComponent<SpriteRenderer>().sortingLayerName = SortingLayer.layers[targetSpriteLayer].name;
        }

        transform.rotation = Quaternion.Euler(0, 0, rotationInit);
    }
}
