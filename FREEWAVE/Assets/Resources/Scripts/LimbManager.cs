using System.Collections.Generic;
using UnityEngine;
using System;

public class Limb
{
    float lengthA, lengthB, lengthCcurrent, lengthCmax, angleA, angleB, rotation;
    public GameObject partA, partB, partC;
    public SpriteRenderer srA, srB, srC;
    int inverted, flipped = 1;
    Vector2 initFollowPosition, followTarget, circleOrgin,followPos,boxsize = new Vector2(0.1f,.2f);
    bool isBackLimb;
    public LimbMode currentLimbMode;

    public void Start(bool _isBackLimb,bool _isInverted)
    {
        if (_isBackLimb)
            isBackLimb = true;

        if (_isInverted)
            inverted = -1;

        partA = new GameObject("PartA");
        partB = new GameObject("PartB");
        partC = new GameObject("PartC");

        srA = partA.AddComponent<SpriteRenderer>();
        srB = partB.AddComponent<SpriteRenderer>();
        srC = partC.AddComponent<SpriteRenderer>();

        partB.transform.position = new Vector2(partA.transform.position.y, partA.transform.position.y - boxsize.y);
        partB.transform.SetParent(partA.transform);

        partC.transform.position = new Vector2(partB.transform.position.y, partB.transform.position.y - boxsize.y);
        partC.transform.SetParent(partB.transform);
    }

    Vector2 GetOffset(Vector2 origionalPosition)
    {
        Vector2 transformVec2 = new Vector2(partA.transform.position.x, partA.transform.position.y);
        Vector2 offset = origionalPosition - transformVec2;
        return offset;
    }

    Vector2 ApplyOffset(Vector2 offset)
    {
        Vector2 transformVec2 = new Vector2(partA.transform.position.x, partA.transform.position.y);
        return transformVec2 + offset;
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
    public void Update()
    {

    }
}