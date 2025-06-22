using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraClass : PrimaryClass
{
    Camera cam;
    CameraState currentCameraState;
    PlatformingState platformingState = new PlatformingState();
    CombatState combatState = new CombatState();

    public override void Start(Manager _manager)
    {
        base.Start(_manager);

        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        List<CameraState> _cameraStates = new List<CameraState>();
        _cameraStates.AddRange(new CameraState[] { platformingState, combatState });

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
        public float lerpSpeedXY, lerpSpeedZ, lerpSpeedFOV, lookAheadX, lookAheadY, targetFOV, targetZ;
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

            cam.gameObject.transform.position = new Vector3(_x + lookAheadX, _y + lookAheadY, _z);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, lerpSpeedFOV);
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

            if (cameraClass.manager.closestZombie != null)
                cameraClass.currentCameraState = cameraClass.combatState;
            
            base.UpdateCameraState();
        }
    }

    public class CombatState : CameraState
    {
        GameObject combatObject;
        float playerLookAheadX = .1f;
        float waitingToLeaveStateCD = 200f,waitingToLeaveStateTimer;

        public override void StartCameraState(CameraClass _cameraClass, Camera _cam)
        {
            base.StartCameraState(_cameraClass, _cam);

            combatObject = new GameObject("CameraCombatObject");
            target = combatObject;
            lerpSpeedXY = 0.05f;
            lerpSpeedZ = 0.03f;
            lerpSpeedFOV = 0.005f;
            targetZ = -5.5f;
            targetFOV = 35f;
        }

        public override void UpdateCameraState()
        {
            if (cameraClass.manager.closestZombie == null)
            {
                waitingToLeaveStateTimer++;
                if (waitingToLeaveStateTimer == waitingToLeaveStateCD)
                    cameraClass.currentCameraState = cameraClass.platformingState;

                lookAheadX = cameraClass.manager.player.xDir * playerLookAheadX;
                combatObject.transform.position = new Vector2(cameraClass.manager.player.gameObject.transform.position.x + lookAheadX,cameraClass.manager.player.gameObject.transform.position.y);
            }
            else
            {
                Vector2 combatObjectPos = (cameraClass.manager.player.gameObject.transform.position + cameraClass.manager.closestZombie.gameObject.transform.position)/2;
                combatObject.transform.position = combatObjectPos;
                lookAheadX = 0;
            }

            base.UpdateCameraState();
        }
    }
}
