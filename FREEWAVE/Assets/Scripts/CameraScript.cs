using System;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    Manager manager;

    GameObject target,player,ship,cursor;

    float lerpSpeedxy = 10f, lerpSpeedz = 10f;
    float initZ,initFOV;

    public Camera cameraComponent;
    
    Player playerScript;
    void Start()
    {
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        player = GameObject.FindGameObjectWithTag("Player");
        ship = GameObject.FindGameObjectWithTag("Ship");

        cameraComponent = GetComponent<Camera>();

        initZ = transform.position.z;
        initFOV = cameraComponent.fieldOfView;

        playerScript = player.GetComponent<Player>();

        target = player;
        cursor = manager.mouseObject;
    }

    void Update()
    {
        Vector2 targetVector = target.transform.position;

        if(playerScript.aiming)
            targetVector = (player.transform.position + cursor.transform.position)/2;

        float targetZ = initZ;
        float targetFOV = initFOV;

        float _x = Mathf.Lerp(transform.position.x, targetVector.x, lerpSpeedxy * Time.deltaTime);
        float _y = Mathf.Lerp(transform.position.y, targetVector.y, lerpSpeedxy * Time.deltaTime);
        float _z = Mathf.Lerp(transform.position.z, targetZ, lerpSpeedz * Time.deltaTime);
        float FOV = Mathf.Lerp(cameraComponent.fieldOfView,targetFOV,lerpSpeedz);

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
