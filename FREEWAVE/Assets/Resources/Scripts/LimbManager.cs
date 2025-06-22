using System.Collections.Generic;
using UnityEngine;
using System;

public class Limb
{
    Character character;
    public float lengthA, lengthB, lengthCcurrent, lengthCmax, angleA, angleB, rotation, lerpSpeed = 1f;
    public GameObject partA, partB, partC;
    public SpriteRenderer srA, srB, srC;
    public LimbMode currentMode;
    public bool isBackLimb,isArm;
    Vector2 currentFollowPos;
    Rest rest = new Rest();
    FollowGameObject followGameObject;

    public void Start(bool _isBackLimb,bool _isArm, GameObject _gameObject,Character _character)
    {
        character = _character;
        isArm = _isArm;
        partA = _gameObject;
        partB = _gameObject.transform.GetChild(0).gameObject;
        partC = partB.transform.GetChild(0).gameObject;

        srA = partA.GetComponent<SpriteRenderer>();
        srB = partB.GetComponent<SpriteRenderer>();
        srC = partC.GetComponent<SpriteRenderer>();

        lengthA = Vector2.Distance(partA.transform.position, partB.transform.position);
        lengthB = Vector2.Distance(partB.transform.position, partC.transform.position);
        lengthCmax = lengthA + lengthB;

        followGameObject = new FollowGameObject();

        if (isArm)
            followGameObject.followObject = character.manager.player.handFollow;
        else
            followGameObject.followObject = character.manager.player.legFollow;

        currentMode = followGameObject;
    }

    public void Update()
    {
        Vector2 targetPos;
        
        if (currentMode.useLocalSpace)
            targetPos = partA.transform.TransformPoint(currentMode.GetTargetPosition(isBackLimb,isArm));
        else
            targetPos = currentMode.GetTargetPosition(isBackLimb,isArm);

        currentFollowPos = Vector2.Lerp(currentFollowPos, targetPos, lerpSpeed);

        ApplyRotations(currentFollowPos);
    }

    void ApplyRotations(Vector2 followPos)
    {
        lengthCcurrent = Vector2.Distance(partA.transform.position, followPos);
        lengthCcurrent = Mathf.Clamp(lengthCcurrent, 0.001f, lengthCmax);

        float angleAnumerator = Mathf.Pow(lengthA, 2) + Mathf.Pow(lengthCcurrent, 2) - Mathf.Pow(lengthB, 2);
        float angleAdenominator = 2 * lengthA * lengthCcurrent;

        float _angleA = Mathf.Clamp(angleAnumerator / angleAdenominator,-1,1);
        angleA = Mathf.Acos(_angleA);

        float angleBnumerator = Mathf.Pow(lengthA, 2) + Mathf.Pow(lengthB, 2) - Mathf.Pow(lengthCcurrent, 2);
        float angleBdenominator = 2 * lengthA * lengthB;

        float _angleB = Mathf.Clamp(angleBnumerator / angleBdenominator, -1, 1);
        angleB = Mathf.Acos(_angleB);

        float rotationNumerator = followPos.y - partA.transform.position.y;
        float rotationDenominator = followPos.x - partA.transform.position.x;

        rotation = Mathf.Atan2(rotationNumerator,rotationDenominator);

        Debug.DrawLine(partA.transform.position, followPos, Color.red);
        Debug.DrawRay(partA.transform.position, partA.transform.right * 0.5f, Color.green);

        float angleADeg = angleA * Mathf.Rad2Deg;
        float angleBDeg = angleB * Mathf.Rad2Deg;
        float rotationDeg = rotation * Mathf.Rad2Deg;

        if (isArm)
        {
            partA.transform.localRotation = Quaternion.Euler(0, 0, rotationDeg - angleADeg);
            partB.transform.localRotation = Quaternion.Euler(0, 0, 180 - angleBDeg);
        }
        else
        {
            partA.transform.localRotation = Quaternion.Euler(0, 0, rotationDeg + angleADeg + 90);
            partB.transform.localRotation = Quaternion.Euler(0, 0, angleBDeg - 180);
        }
    }

}

public abstract class LimbMode
{
    public bool useLocalSpace = true;
    public abstract Vector2 GetTargetPosition(bool isBackLimb,bool isArm);
}

public class FollowVector2 : LimbMode
{
    public Vector2 vector2;

    public override Vector2 GetTargetPosition(bool isBackLimb, bool isArm)
    {
        return vector2;
    }
}

public class FollowGameObject : LimbMode
{
    public GameObject followObject;

    public FollowGameObject() { useLocalSpace = false; }

    public override Vector2 GetTargetPosition(bool isBackLimb, bool isArm)
    {
        return followObject.transform.position;
    }
}

public class Rest : LimbMode
{
    public float maxReach;

    public override Vector2 GetTargetPosition(bool isBackLimb, bool isArm)
    {
        if(isArm)
            return new Vector2(-1, 0) * maxReach;
        else
            return new Vector2(0, -1) * maxReach;
    }
}

public class ThreePoints : LimbMode
{
    public Vector2 pointA, pointB, pointC;
    public float duration, initDuration;
    public bool loop;

    public override Vector2 GetTargetPosition(bool isBackLimb, bool isArm)
    {
        if (duration > 0)
            duration -= 1;
        else if (loop)
            duration = initDuration;

        float t = 1f - Mathf.Clamp01(duration / initDuration);

        if (isBackLimb)
            t = (t + 0.5f) % 1f;

        float segment = 1f / 3f;

        if (t < segment)
        {
            float segmentT = t / segment;
            return Vector2.Lerp(pointA, pointB, segmentT);
        }
        else if (t < 2f * segment)
        {
            float segmentT = (t - segment) / segment;
            return Vector2.Lerp(pointB, pointC, segmentT);
        }
        else
        {
            float segmentT = (t - 2f * segment) / segment;
            return Vector2.Lerp(pointC, pointA, segmentT);
        }
    }
}
