using System.Collections.Generic;
using UnityEngine;
using System;

public class Limb
{
    Character character;
    public float lengthA, lengthB, lengthCcurrent, lengthCmax, angleA, angleB, rotation, lerpSpeed = .075f;
    public GameObject partA, partB, partC,followObject;
    public SpriteRenderer srA, srB, srC;
    public LimbMode currentMode;
    public bool isBackLimb,isArm;
    Rest rest = new Rest();

    public void Start(bool _isBackLimb,bool _isArm, GameObject _gameObject,Character _character)
    {
        isBackLimb = _isBackLimb;
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

        followObject = new GameObject(partA.name + "followObject");
        followObject.transform.SetParent(partA.transform.parent);
        currentMode = rest;
        rest.maxReach = lengthCmax;
    }

    public void Update()
    {
        Vector2 targetPos;

        if (currentMode == null)
            currentMode = rest;

        targetPos = currentMode.GetTargetPosition(isBackLimb,isArm);

        Vector2 ajustedTarget = (Vector2)partA.transform.position + targetPos;

        followObject.transform.position = Vector2.Lerp(followObject.transform.position, ajustedTarget,Time.time * lerpSpeed);

        ApplyRotations(followObject.transform.position);
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

        float angleADeg = angleA * Mathf.Rad2Deg;
        float angleBDeg = angleB * Mathf.Rad2Deg;
        float rotationDeg = rotation * Mathf.Rad2Deg;

        float _x = Mathf.Sign(character.gameObject.transform.localScale.x * 180);

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

public class Rest : LimbMode
{
    public float maxReach;

    public override Vector2 GetTargetPosition(bool isBackLimb, bool isArm)
    {
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
