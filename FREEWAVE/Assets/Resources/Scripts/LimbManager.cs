using System.Collections.Generic;
using UnityEngine;
using System;

public class Limb
{
    Character character;
    public float lengthA, lengthB, lengthCcurrent, lengthCmax, angleA, angleB, rotation,lerpSpeed = 0.01f,baseRotation;
    public GameObject partA, partB, partC;
    public SpriteRenderer srA, srB, srC;
    int inverted, flipped = 1;
    Vector2 followPos, followTarget;
    bool isBackLimb;
    public LimbMode currentLimbMode;
    Rest rest = new Rest();

    public void Start(bool _isBackLimb,bool _isInverted,GameObject _gameObject,Character _character)
    {
        character = _character;

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

        lengthA = partA.transform.position.y - partB.transform.position.y;
        lengthB = partB.transform.position.y - partC.transform.position.y;
        lengthCmax = lengthA + lengthB;

        baseRotation = partA.transform.eulerAngles.z;

        rest.restPos = new Vector2(0,-lengthCmax);
        followPos = rest.restPos;
    }

    public void Update()
    {

        flipped = Mathf.RoundToInt(Mathf.Sign(character.gameObject.transform.localScale.x));

        if (currentLimbMode == null)
            followTarget = rest.Update(isBackLimb);
        else
            followTarget = currentLimbMode.Update(isBackLimb);

        Vector2 rotatedTarget = Quaternion.Euler(0, 0, baseRotation) * followTarget;

        float _x = Mathf.Lerp(partA.transform.position.x, (rotatedTarget.x * Mathf.Sign(character.gameObject.transform.localScale.x)) + partA.transform.position.x, lerpSpeed);
        float _y = Mathf.Lerp(partA.transform.position.y, rotatedTarget.y + partA.transform.position.y, lerpSpeed);

        followPos = new Vector2((rotatedTarget.x * Mathf.Sign(character.gameObject.transform.localScale.x)) + partA.transform.position.x, rotatedTarget.y + partA.transform.position.y);

        ApplyRotations();
    }

    void ApplyRotations()
    {
        Vector3 _followPos = new Vector3(followPos.x, followPos.y, 0);

        Vector2 direction = (_followPos - partA.transform.position).normalized;
        float _angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rotation = _angle + baseRotation + 90;

        Vector2 _lengthC = _followPos - partA.transform.position;
        lengthCcurrent = Mathf.Clamp(_lengthC.magnitude, 0.001f, lengthCmax - 0.001f);

        if (lengthCcurrent >= lengthCmax - 0.001f)
        {
            angleA = rotation;
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
        cosA = Mathf.Clamp(cosA, -1f, 1f);
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




