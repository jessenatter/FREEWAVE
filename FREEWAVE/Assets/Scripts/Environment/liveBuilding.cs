using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Accessibility;

public class LiveBuilding : MonoBehaviour
{
    List<Transform> buildingSegments = new List<Transform>();

    List<LimbManager> limbs = new List<LimbManager>();

    List<sack> sacks = new List<sack>();
 
    float maxBendAngle = 15f, angleModifier = 1f, lerpSpeed = 5f;
    GameObject target;
    Player player;
    Ship ship;

    bool alive = true;
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        ship = GameObject.FindGameObjectWithTag("Ship").GetComponent<Ship>();

        target = player.gameObject;

        for (int i = 0; i < transform.childCount; i++)
        {
            buildingSegments.Add(transform.GetChild(i));
            if(transform.GetChild(i).GetComponentInChildren<LimbManager>())
            {
                foreach(LimbManager lm in transform.GetChild(i).GetComponentsInChildren<LimbManager>())
                    limbs.Add(lm);
            }

            if(transform.GetChild(i).GetComponentInChildren<sack>())
            {
                foreach(sack _sack in transform.GetChild(i).GetComponentsInChildren<sack>())
                    sacks.Add(_sack);
            }
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
        target = player.characterIsActive ? player.gameObject : ship.gameObject;

        if(alive == false)
            return;

        UpdateLayers();
        UpdateLimbs();
        CheckSacks();
    }

    void UpdateLayers()
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

    void UpdateLimbs()
    {
        foreach(LimbManager limb in limbs)
        {
            limb.transform.position = Vector2.Lerp(limb.transform.position,target.transform.position,Time.deltaTime * lerpSpeed);
        }
    }

    void CheckSacks()
    {
        if(sacks.Where(s => !s.stabbed).Any())
            return;
        
        if(!alive)
            return;

        Die();
    }

    void Die()
    {
        alive = false;
    }
}
