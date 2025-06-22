using System.Collections.Generic;
using UnityEngine;
using System;

public class Limb
{
    Character character;
    public float lengthA, lengthB, lengthCcurrent, lengthCmax, angleA, angleB, rotation, lerpSpeed = 0.01f;
    public GameObject partA, partB, partC;
    public SpriteRenderer srA, srB, srC;
    public LimbMode currentMode;
    public bool isBackLimb,isArm;
    Vector2 currentFollowPos;
    Rest rest = new Rest();

    public void Start(bool _isBackLimb,bool _isArm, GameObject _gameObject,Character character)
    {
        isArm = _isArm;
        partA = _gameObject;
        partB = _gameObject.transform.GetChild(0).gameObject;
        partC = partB.transform.GetChild(0).gameObject;

        srA = partA.GetComponent<SpriteRenderer>();
        srB = partB.GetComponent<SpriteRenderer>();
        srC = partC.GetComponent<SpriteRenderer>();

        lengthA = partA.transform.position.y - partB.transform.position.y;
        lengthB = partB.transform.position.y - partC.transform.position.y;
        lengthCmax = lengthA + lengthB;

        rest.maxReach = lengthCmax;
        currentMode = rest;
    }

    public void Update()
    {
        Vector2 targetPos = Vector2.zero;

        if (currentMode.useLocalSpace)
            targetPos = partA.transform.TransformPoint(currentMode.GetTargetPosition(isBackLimb));
        else
            targetPos = currentMode.GetTargetPosition(isBackLimb);

        currentFollowPos = Vector2.Lerp(currentFollowPos, targetPos, lerpSpeed);

        ApplyRotations(currentFollowPos);
    }

    void ApplyRotations(Vector3 followPos)
    {
        Vector2 localTarget = followPos;

        float dx = localTarget.x;
        float dy = localTarget.y;

        float distance = Mathf.Sqrt(dx * dx + dy * dy);
        lengthCcurrent = Mathf.Clamp(distance, 0.001f, lengthCmax - 0.001f);

        float angleToTarget = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

        if (lengthCcurrent >= lengthCmax - 0.001f)
        {
            angleA = rotation;
            angleB = rotation;
        }
        else
        {
            Vector3 triangleAngles = CalculateTriangleAngles(lengthA, lengthB, lengthCcurrent);
            angleA = rotation - triangleAngles.y;
            angleB = rotation + triangleAngles.x;
        }

        angleA = Mathf.Repeat(angleA, 360);
        angleB = Mathf.Repeat(angleB, 360);

        partA.transform.localRotation = Quaternion.Euler(0, 0, angleA);
        partB.transform.localRotation = Quaternion.Euler(0, 0, angleB);

    }

    void FindAngles()
    {
        Vector3 triangleAngles = CalculateTriangleAngles(lengthA, lengthB, lengthCcurrent);
        angleA = rotation - triangleAngles.y;
        angleB = rotation + triangleAngles.x;
    }

    Vector3 CalculateTriangleAngles(float a, float b, float c)
    {
        float angleA = CalculateAngle(a, b, c);
        float angleB = CalculateAngle(b, c, a);
        float angleC = 180f - angleA - angleB;

        return new Vector3(angleA, angleB, angleC);
    }

    float CalculateAngle(float a, float b, float c)
    {
        float cosA = (b * b + c * c - a * a) / (2 * b * c);
        cosA = Mathf.Clamp(cosA, -1f, 1f);
        float angleA = Mathf.Acos(cosA) * Mathf.Rad2Deg;
        return angleA;
    }
}

public abstract class LimbMode
{
    public bool useLocalSpace = true;
    public abstract Vector2 GetTargetPosition(bool isBackLimb);
}

public class FollowVector2 : LimbMode
{
    public Vector2 vector2;

    public override Vector2 GetTargetPosition(bool isBackLimb)
    {
        return vector2;
    }
}

public class FollowGameObject : LimbMode
{
    public GameObject followObject;

    FollowGameObject() { useLocalSpace = false; }

    public override Vector2 GetTargetPosition(bool isBackLimb)
    {
        return followObject.transform.position;
    }
}

public class Rest : LimbMode
{
    public float maxReach;

    public override Vector2 GetTargetPosition(bool isBackLimb)
    {
        return new Vector2(0, -1) * maxReach;
    }
}

public class ThreePoints : LimbMode
{
    public Vector2 pointA, pointB, pointC;
    public float duration, initDuration;
    public bool loop;

    public override Vector2 GetTargetPosition(bool isBackLimb)
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
