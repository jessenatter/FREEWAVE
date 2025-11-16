using System;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    Manager manager;

    GameObject target,player,ship,cursor;

    float lerpSpeedxy = 10f, lerpSpeedz = 10f;
    float initZ,initFOV;

    float shipFOV = 80,playerFOV = 65,aimFOV = 70;

    float shipZ = -10,playerZ = -8,aimZ = -9;
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

        float targetZ = initZ;
        float targetFOV = initFOV;
        Vector2 lookAhead = Vector2.zero;

        if(manager.GameState == Manager.gameState.playerControl)
        {
            targetZ = playerZ;
            targetFOV = playerFOV;
            lookAhead = new Vector2(0,1);

            if(playerScript.aiming)
            {
                targetZ = aimZ;
                targetFOV = aimFOV;
                targetVector = Vector2.Lerp(player.transform.position,cursor.transform.position,.7f);
            }
        }
        else
        {
            targetZ = shipZ;
            targetFOV = shipFOV;
        }

        float _x = Mathf.Lerp(transform.position.x, targetVector.x + lookAhead.x, lerpSpeedxy * Time.deltaTime);
        float _y = Mathf.Lerp(transform.position.y, targetVector.y + lookAhead.y, lerpSpeedxy * Time.deltaTime);
        float _z = Mathf.Lerp(transform.position.z, targetZ, lerpSpeedz * Time.deltaTime);
        float FOV = Mathf.Lerp(cameraComponent.fieldOfView,targetFOV,lerpSpeedz * Time.deltaTime);

        cameraComponent.fieldOfView = FOV;
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
