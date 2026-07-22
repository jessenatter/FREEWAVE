using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.U2D.IK;

public class LimbManager : MonoBehaviour
{
     public GameObject orgin;
    [SerializeField] Character character;
    SingleFloor singleFloor;
    Vector2 initOffsetFromOrgin,recordedPoint;
    int currentPoint = 0;
    [HideInInspector]public limbState currentLimbState;
    PublicTimer currentTime = new PublicTimer();
    PublicTimer currentTransitionTime = new PublicTimer();
    bool waitingForExit,transitionedToNextState = true;

    [SerializeField] bool isBackLimb;
    limbState previousLimbState = null;
    public class limbState
    {
        public List<Vector2> points = new List<Vector2>();
        public List<Vector2> backLimbPoints = new List<Vector2>();
        public float duration,transitionDuration;
        public bool loop,startAtPos1,useBackLimbOffset;

        public limbState(GameObject pointsContainer,float _duration,bool _loop,LimbManager limbManager,float _transitionDuration,bool _useBackLimbOffset) 
        {
            transitionDuration = _transitionDuration;

            if(pointsContainer.gameObject.tag != "usesSeperateLimbs")
            {
                if(pointsContainer.transform.childCount == 0)
                    points.Add(Vector2.zero);
                else
                {
                    for(int i = 0; i < pointsContainer.transform.childCount; i++)
                    {
                        Vector2 point = ((Vector2)pointsContainer.transform.GetChild(i).transform.position - (Vector2)limbManager.orgin.transform.position) - limbManager.initOffsetFromOrgin ;
                        points.Add(point);
                    }
                }
            }
            else
            {
                GameObject frontLimb = pointsContainer.transform.GetChild(0).gameObject;
                GameObject backLimb = pointsContainer.transform.GetChild(1).gameObject;

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
            useBackLimbOffset = _useBackLimbOffset;
        }
    }

    void Awake()
    {
        if (orgin != null)
        {
            initOffsetFromOrgin = transform.position - orgin.transform.position;
            recordedPoint = transform.position;
        }
    }

    void Start()
    {
        if (orgin != null)
        {
            initOffsetFromOrgin = transform.position - orgin.transform.position;
            recordedPoint = transform.position;
        }

        if(character == null)
            singleFloor = transform.parent.gameObject.GetComponent<SingleFloor>();
    }
    void Update()
    {
        UpdatePoints();
    }

    void FixedUpdate()
    {
        if(currentLimbState != null)
            currentTime.TickLoop();
    }
    void UpdatePoints()
    {
        if(currentLimbState == null) return;
        
        List<Vector2> pointsToUse = currentLimbState.points;

        if(isBackLimb && currentLimbState.backLimbPoints.Count != 0)
            pointsToUse = currentLimbState.backLimbPoints;

        float durationPerPoint = currentLimbState.duration / pointsToUse.Count;

        float t = durationPerPoint <= 0f ? 0f : currentTime.Current / durationPerPoint;

        currentPoint = Mathf.FloorToInt(t) % pointsToUse.Count;

        float lerpAmmount = t - Mathf.Floor(t);

        int nextIndex = (currentPoint + 1) % pointsToUse.Count;
        
        if(isBackLimb && currentLimbState.useBackLimbOffset)
        {
            float offset = pointsToUse.Count/2f;
            currentPoint = (int)Mathf.Repeat(currentPoint + offset,pointsToUse.Count);
            nextIndex = (int)Mathf.Repeat(nextIndex + offset,pointsToUse.Count);
        }

        Vector2 thisPoint = pointsToUse[currentPoint];
        Vector2 nextPoint = pointsToUse[nextIndex];

        //only need chr heere

        GameObject characterTransform = null;
        if(character != null)
            characterTransform = character.gameObject;
        else
            characterTransform = singleFloor.gameObject;

        Vector2 characterDirectionVec = new Vector2(Mathf.Sign(characterTransform.transform.localScale.x),1);

        Vector2 initRestPos = (Vector2)orgin.transform.position + (initOffsetFromOrgin * characterDirectionVec);

        Vector2 exactTarget = initRestPos + (Vector2.Lerp(thisPoint, nextPoint, lerpAmmount) * characterDirectionVec);

        if(previousLimbState == null || previousLimbState != currentLimbState)
        {
            previousLimbState = currentLimbState;
            if(currentLimbState.startAtPos1)
                transform.position = initRestPos + pointsToUse[0] * characterDirectionVec;

            currentTime.SetDuration(currentLimbState.duration);
            currentTransitionTime.SetDuration(currentLimbState.transitionDuration);
            currentTime.Reset();
            currentTransitionTime.Reset();

            recordedPoint = exactTarget;
            transitionedToNextState = false;
        }

        if(transitionedToNextState == false)
        {
            if(currentTransitionTime.Tick())
            {
                transitionedToNextState = true;
            }
        }

        float t2 = currentTransitionTime.Progress;
        transform.position = Vector2.Lerp(transform.position, exactTarget, t2);


    }   
}
