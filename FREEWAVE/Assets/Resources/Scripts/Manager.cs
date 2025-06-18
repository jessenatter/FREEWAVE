using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class Manager : MonoBehaviour
{
    public Player player = new Player();
    CameraClass cameraClass = new CameraClass();

    List<PrimaryClass> alwaysUpdate = new List<PrimaryClass>();

    [HideInInspector] public int groundLayer, characterLayer, buildingLayer, shipLayer, environmentLayer;
    [HideInInspector] public LayerMask groundMask, characterMask, buildingMask, shipMask, environmentMask;
    [HideInInspector] public InputAction moveAction, jumpAction, attackAction, interactAction, dodgeAction, aimAction, shootAction;

    private void Awake()
    {
        groundLayer = LayerMask.NameToLayer("Ground");
        characterLayer = LayerMask.NameToLayer("Character");
        buildingLayer = LayerMask.NameToLayer("Building");
        shipLayer = LayerMask.NameToLayer("Ship");

        groundMask = LayerMask.GetMask("Ground");
        characterMask = LayerMask.GetMask("Character");
        buildingMask = LayerMask.GetMask("Building");
        shipMask = LayerMask.GetMask("Ship");

        alwaysUpdate.Add(player);
        alwaysUpdate.Add(cameraClass);

        foreach (PrimaryClass primary in alwaysUpdate)
            primary.Start(this);

        StartInputs();
    }

    void StartInputs()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    void FixedUpdate()
    {
        foreach(PrimaryClass primary in alwaysUpdate)
            primary.Update();
    }
}

public class PrimaryClass
{
    public Manager manager;

    public virtual void Start(Manager _manager)
    {
        manager = _manager;
    }
    public virtual void Update()
    {

    }
}

public class CameraClass : PrimaryClass
{
    Camera cam;
    CameraState currentCameraState;
    PlatformingState platformingState = new PlatformingState();

    public override void Start(Manager _manager)
    {
        base.Start(_manager);

        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        List<CameraState> _cameraStates = new List<CameraState>();
        _cameraStates.AddRange(new CameraState[] { platformingState });

        foreach (CameraState cameraState in _cameraStates)
            cameraState.StartCameraState(this, cam);

        currentCameraState = platformingState;
    }

    public override void Update()
    {
        base.Update();

        currentCameraState.UpdateCameraState();
    }

    public class CameraState
    {
        public GameObject target;
        public float lerpSpeedXY, lerpSpeedZ,lerpSpeedFOV,lookAheadX,lookAheadY,targetFOV,targetZ;
        protected Camera cam;
        protected CameraClass cameraClass;

        public virtual void StartCameraState(CameraClass _cameraClass, Camera _cam)
        {
            cam = _cam;
            cameraClass = _cameraClass;
        }

        public virtual void UpdateCameraState()
        {
            float _y = Mathf.Lerp(cam.gameObject.transform.position.y, target.transform.position.y, lerpSpeedXY);
            float _x = Mathf.Lerp(cam.gameObject.transform.position.x, target.transform.position.x, lerpSpeedXY);
            float _z = Mathf.Lerp(cam.gameObject.transform.position.z, targetZ, lerpSpeedZ);

            cam.gameObject.transform.position = new Vector3(_x + lookAheadX,_y + lookAheadY,_z);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView,targetFOV,lerpSpeedFOV);
        }
    }

    public class PlatformingState : CameraState
    {
        float playerLookAheadX = .1f;

        public override void StartCameraState(CameraClass _cameraClass, Camera _cam)
        {
            base.StartCameraState(_cameraClass, _cam);

            target = cameraClass.manager.player.gameObject;
            lerpSpeedXY = 0.05f;
            lerpSpeedZ = 0.03f;
            lerpSpeedFOV = 0.01f;
            targetZ = -7f;
            targetFOV = 40f;
        }

        public override void UpdateCameraState()
        {
            lookAheadX = cameraClass.manager.player.xDir * playerLookAheadX;
            base.UpdateCameraState();
        }
    }
}