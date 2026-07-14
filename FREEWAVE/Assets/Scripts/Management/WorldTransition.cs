using UnityEngine;
using UnityEngine.InputSystem;

public class WorldTransition : MonoBehaviour
{
    //this script will be both the manager and camera for world transition scene
    [HideInInspector]public InputAction moveAction, jumpAction,dodgeAction,useDrugAction,switchRightAction;

    GameObject ship;
    float lerpSpeedxy = 10f, lerpSpeedz = 3f;
    float initZ,initFOV;

    public Camera cameraComponent;
    
    void Start()
    {
        ship = GameObject.FindGameObjectWithTag("Ship");

        cameraComponent = GetComponent<Camera>();

        initZ = transform.position.z;
        initFOV = cameraComponent.fieldOfView;

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        dodgeAction = InputSystem.actions.FindAction("Dodge");
        useDrugAction = InputSystem.actions.FindAction("UseDrug");
        switchRightAction = InputSystem.actions.FindAction("SwitchRight");
    }

    void Update()
    {
        Vector2 shipVector = ship.transform.position;
        float targetZ = initZ;
        float targetFOV = initFOV;

        float _x = Mathf.Lerp(transform.position.x, shipVector.x, lerpSpeedxy * Time.deltaTime);
        float _y = Mathf.Lerp(transform.position.y, shipVector.y, lerpSpeedxy * Time.deltaTime);
        float _z = Mathf.Lerp(transform.position.z, targetZ, lerpSpeedz * Time.deltaTime);
        float FOV = Mathf.Lerp(cameraComponent.fieldOfView,initFOV,lerpSpeedz * Time.deltaTime);

        cameraComponent.fieldOfView = FOV;
        transform.position = new Vector3(_x, _y, _z);
    }

}
