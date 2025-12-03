using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class LimbManager : MonoBehaviour
{
    [SerializeField] public GameObject orgin;
    [SerializeField] Character character;
    Vector2 initOffsetFromOrgin,recordedPoint;
    int currentPoint = 0;
    public limbState currentLimbState;
    float currentTime,currentTransitionTime;
    bool waitingForExit,transitionedToNextState = true;

    [SerializeField] bool isBackLimb;
    limbState previousLimbState = null;
    public class limbState
    {
        public List<Vector2> points = new List<Vector2>();
        public List<Vector2> backLimbPoints = new List<Vector2>();
        public float duration,transitionDuration;
        public bool loop,startAtPos1;

        public limbState(GameObject pointsObject,float _duration,bool _loop,LimbManager limbManager,float _transitionDuration) 
        {
            transitionDuration = _transitionDuration;

            if(pointsObject.gameObject.tag != "usesSeperateLimbs")
            {
                if(pointsObject.transform.childCount == 0)
                    points.Add(Vector2.zero);
                else
                {
                    for(int i = 0; i < pointsObject.transform.childCount; i++)
                    {
                        Vector2 point = ((Vector2)pointsObject.transform.GetChild(i).transform.position - (Vector2)limbManager.orgin.transform.position) - limbManager.initOffsetFromOrgin ;
                        points.Add(point);
                    }
                }
            }
            else
            {
                GameObject frontLimb = pointsObject.transform.GetChild(0).gameObject;
                GameObject backLimb = pointsObject.transform.GetChild(1).gameObject;

                if(frontLimb.transform.childCount == 0)
                    points.Add(Vector2.zero);
                else
                {
                    for(int i = 0; i < frontLimb.transform.childCount; i++)
                    {
                        Vector2 point = ((Vector2)frontLimb.transform.GetChild(i).transform.position - (Vector2)limbManager.orgin.transform.position) - limbManager.initOffsetFromOrgin ;
                        points.Add(point);
                    }
                }

                if(backLimb.transform.childCount == 0)
                    backLimbPoints.Add(Vector2.zero);
                else
                {
                    for(int i = 0; i < backLimb.transform.childCount; i++)
                    {
                        Vector2 point = ((Vector2)backLimb.transform.GetChild(i).transform.position - (Vector2)limbManager.orgin.transform.position) - limbManager.initOffsetFromOrgin ;
                        backLimbPoints.Add(point);
                    }
                }
            }

            duration = _duration;
            loop = _loop;
        }
    }

    void Start()
    {
        initOffsetFromOrgin = transform.position - orgin.transform.position;
        recordedPoint = transform.position;
    }
    void Update()
    {
        UpdatePoints();
    }

    void FixedUpdate()
    {
        currentTime++;
    }
    void UpdatePoints()
    {
        List<Vector2> pointsToUse = currentLimbState.points;

        if(isBackLimb && currentLimbState.backLimbPoints.Count != 0)
            pointsToUse = currentLimbState.backLimbPoints;

        float durationPerPoint = currentLimbState.duration / pointsToUse.Count;

        float t = currentTime / durationPerPoint;

        currentPoint = Mathf.FloorToInt(t) % pointsToUse.Count;

        float lerpAmmount = t - Mathf.Floor(t);

        int nextIndex = (currentPoint + 1) % pointsToUse.Count;
        
        if(isBackLimb)
        {
            currentPoint = (int)Mathf.Repeat(currentPoint + 2,pointsToUse.Count);
            nextIndex = (int)Mathf.Repeat(nextIndex + 2,pointsToUse.Count);
        }

        Vector2 thisPoint = pointsToUse[currentPoint];
        Vector2 nextPoint = pointsToUse[nextIndex];

        Vector2 characterDirectionVec = new Vector2(Mathf.Sign(character.transform.localScale.x),1);
        Vector2 initRestPos = (Vector2)orgin.transform.position + (initOffsetFromOrgin * characterDirectionVec);

        Vector2 exactTarget = initRestPos + (Vector2.Lerp(thisPoint, nextPoint, lerpAmmount) * characterDirectionVec);

        if(previousLimbState == null || previousLimbState != currentLimbState)
        {
            previousLimbState = currentLimbState;
            if(currentLimbState.startAtPos1)
                transform.position = initRestPos + pointsToUse[0] * characterDirectionVec;

            currentTime = 0;
            currentTransitionTime = 0;

            recordedPoint = exactTarget;
            transitionedToNextState = false;
        }

        if(transitionedToNextState == false)
        {
            currentTransitionTime++;
            if(currentTransitionTime == currentLimbState.transitionDuration)
            {
                transitionedToNextState = true;
            }
        }

        float t2 = currentTransitionTime/currentLimbState.transitionDuration;
        transform.position = Vector2.Lerp(transform.position, exactTarget, t2);
    }   
}
