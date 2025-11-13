using UnityEngine;

public class CameraScript : MonoBehaviour
{
    Manager manager;

    GameObject target,player,ship;

    float lerpSpeedxy = 10f, lerpSpeedz = 10f;
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

        float _x = Mathf.Lerp(transform.position.x, targetObject.transform.position.x, lerpSpeedxy * Time.deltaTime);
        float _y = Mathf.Lerp(transform.position.y, targetObject.transform.position.y, lerpSpeedxy * Time.deltaTime);
        float _z = Mathf.Lerp(transform.position.z, targetZ, lerpSpeedz * Time.deltaTime);

        transform.position = new Vector3(_x, _y, _z);
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
