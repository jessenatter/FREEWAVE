using System.Collections.Generic;
using UnityEngine;

public class LiveBuilding : MonoBehaviour
{
    List<Transform> buildingSegments = new List<Transform>();

    [SerializeField] float maxBendAngle = 15f;

    [SerializeField] float angleModifier = 1f;

    GameObject target;

    [SerializeField] float lerpSpeed = 5f;

    void Awake()
    {
        target = GameObject.FindGameObjectWithTag("Player");

        for (int i = 0; i < transform.childCount; i++)
        {
            buildingSegments.Add(transform.GetChild(i));
        }

        // Floor index should run from bottom (0) to top.
        buildingSegments.Sort((a, b) => a.position.y.CompareTo(b.position.y));

        // Build a chain: segment 0 -> 1 -> 2 -> ... while keeping current world transforms.
        for (int i = 1; i < buildingSegments.Count; i++)
        {
            buildingSegments[i].SetParent(buildingSegments[i - 1], true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < buildingSegments.Count; i++)
        {
            Transform segment = buildingSegments[i];

            float targetZAngle = 0f;

            if (i > 0)
            {
                Vector2 toPlayer = target.transform.position - segment.position;

                if (toPlayer.sqrMagnitude > 0.0001f)
                {
                    float playerAngle = Mathf.Clamp(Vector2.SignedAngle(Vector2.up, toPlayer.normalized), -maxBendAngle, maxBendAngle);
                    float bendForSegment = Mathf.Max(0f, Mathf.Abs(playerAngle) - (angleModifier * i));
                    targetZAngle = Mathf.Sign(playerAngle) * bendForSegment;
                }
            }

            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetZAngle);
            segment.localRotation = Quaternion.Lerp(segment.localRotation, targetRotation, Time.deltaTime * lerpSpeed);
        }
    }
}
