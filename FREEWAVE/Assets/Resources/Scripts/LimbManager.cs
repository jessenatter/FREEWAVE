using System.Collections.Generic;
using UnityEngine;
using System;

public class Limb
{
    float lengthA, lengthB, lengthCcurrent, lengthCmax, angleA, angleB, rotation,lerpSpeed = 0.01f;
    public GameObject partA, partB, partC;
    public SpriteRenderer srA, srB, srC;
    int inverted, flipped = 1;
    Vector2 followPos, followTarget;
    bool isBackLimb;
    public LimbMode currentLimbMode;
    Rest rest = new Rest();

    public void Start(bool _isBackLimb,bool _isInverted,GameObject _gameObject)
    {
        if (_isBackLimb)
            isBackLimb = true;

        if (_isInverted)
            inverted = -1;

        partA = _gameObject;
        partB = _gameObject.transform.GetChild(0).gameObject;
        partC = partB.transform.GetChild(0).gameObject;

        srA = partA.GetComponent<SpriteRenderer>();
        srB = partB.GetComponent<SpriteRenderer>();
        srC = partC.GetComponent<SpriteRenderer>();

        rest.restPos = new Vector2(0,partC.transform.position.y-partA.transform.position.y);
        followPos = partC.transform.position;
    }

    public void Update()
    {
        if (currentLimbMode == null)
            followTarget = rest.Update(isBackLimb);
        else
            followTarget = currentLimbMode.Update(isBackLimb);

        float _x = Mathf.Lerp(partA.transform.position.x,followTarget.x + partA.transform.position.x, lerpSpeed * Time.fixedDeltaTime);
        float _y = Mathf.Lerp(partA.transform.position.y,followTarget.y + partA.transform.position.y, lerpSpeed * Time.fixedDeltaTime);

        followPos = new Vector2(_x,_y);
        ApplyRotations();
    }

    void ApplyRotations()
    {
        Vector3 _followPos = new Vector3(followPos.x, followPos.y, 0);

        Vector2 direction = (_followPos - partA.transform.position).normalized;
        float _angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rotation = _angle + 90;

        Vector2 _lengthC = _followPos - partA.transform.position;
        lengthCcurrent = Mathf.Clamp(_lengthC.magnitude, 0, lengthCmax);

        if (lengthCcurrent == lengthCmax)
        {
            angleA = rotation;
            angleB = rotation;
        }
        else if (lengthCcurrent < 0.05)
        {
            angleA = rotation - 180;
            angleB = rotation;
        }
        else
            FindAngles();

        angleA = Mathf.Repeat(angleA, 360);
        angleB = Mathf.Repeat(angleB, 360);

        partA.transform.rotation = Quaternion.Euler(0, 0, angleA);
        partB.transform.rotation = Quaternion.Euler(0, 0, angleB);
    }

    void FindAngles()
    {
        Vector3 triangleAngles = CalculateTriangleAngles(lengthA, lengthB, lengthCcurrent);
        angleA = rotation - triangleAngles.y * inverted * flipped;
        angleB = rotation + triangleAngles.x * inverted * flipped;
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
        float angleA = Mathf.Acos(cosA) * Mathf.Rad2Deg;
        return angleA;
    }
}

public class LimbMode
{
    public virtual Vector2 Update(bool isBackLimb)
    {
        return Vector2.zero;
    }
}

public class FollowVector2 : LimbMode
{
    public Vector2 vector2;

    public override Vector2 Update(bool isBackLimb)
    {
        base.Update(isBackLimb);

        return vector2;
    }
}

public class FollowGameObject : LimbMode
{
    public GameObject followObject;

    public override Vector2 Update(bool isBackLimb)
    {
        base.Update(isBackLimb);

        Vector2 followObjectRelativePos = followObject.transform.position;
        return followObjectRelativePos;
    }
}

public class Rest : LimbMode
{
    public Vector2 restPos;

    public override Vector2 Update(bool isBackLimb)
    {
        base.Update(isBackLimb);

        return restPos;
    }
}
public class ThreePoints : LimbMode
{
    public Vector2 pointA, pointB, pointC;
    public float duration, initDuration;
    public bool loop;

    public override Vector2 Update(bool isBackLimb)
    {
        base.Update(isBackLimb);

        if (duration > 0)
            duration -= 1;
        else if (loop)
        {
            Vector2 _a = pointA;
            Vector2 _c = pointC;
            pointA = _c;
            pointC = _a;

            duration = initDuration;
        }

        float t = 1f - Mathf.Clamp01(duration / initDuration);

        if (isBackLimb)
            t = 1f - t;

        Vector2 ab = Vector2.Lerp(pointA, pointB, t);
        Vector2 bc = Vector2.Lerp(pointB, pointC, t);
        return Vector2.Lerp(ab, bc, t);
    }
}


