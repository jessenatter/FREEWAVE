using UnityEngine;

public class CameraScript : MonoBehaviour
{
    Manager manager;

    GameObject target,player,ship;

    float lerpSpeedxy = 0.1f, lerpSpeedz = 0.05f;
    float initZ;
    
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        player = GameObject.FindGameObjectWithTag("Player");
        ship = GameObject.FindGameObjectWithTag("Ship");

        initZ = transform.position.z;
        
        target = player;
    }

    void Update()
    {
        GameObject targetObject = target;
        float targetZ = initZ;

        Vector3 targetPos = new Vector3(targetObject.transform.position.x, targetObject.transform.position.y, targetZ);
        transform.position = Vector2.Lerp(transform.position, targetPos, lerpSpeedxy);
        transform.position = new Vector3(transform.position.x,transform.position.y,Mathf.Lerp(transform.position.z, targetZ, lerpSpeedz));
    }

    void FixedUpdate()
    {

    }

    public void EnterShip()
    {
        target = ship;
    }
    
    public void ExitShip()
    {
        target = player;
    }
}
