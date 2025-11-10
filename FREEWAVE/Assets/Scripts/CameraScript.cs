using UnityEngine;

public class CameraScript : MonoBehaviour
{
    Manager manager;

    GameObject player;

    float lerpSpeedxy = 0.1f, lerpSpeedz = 0.05f;
    float initZ;
    
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        player = GameObject.FindGameObjectWithTag("Player");

        initZ = transform.position.z;
    }

    void Update()
    {
        GameObject targetObject = player;
        float targetZ = initZ;

        Vector3 target = new Vector3(targetObject.transform.position.x, targetObject.transform.position.y, targetZ);
        transform.position = Vector2.Lerp(transform.position, target, lerpSpeedxy);
        transform.position = new Vector3(transform.position.x,transform.position.y,Mathf.Lerp(transform.position.z, targetZ, lerpSpeedz));
    }

    void FixedUpdate()
    {
        
    }
}
