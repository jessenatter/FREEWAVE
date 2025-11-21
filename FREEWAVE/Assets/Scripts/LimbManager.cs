using System.Collections.Generic;
using UnityEngine;

public class LimbManager : MonoBehaviour
{
    [SerializeField] GameObject orgin;
    [SerializeField] Character character;
    Vector2 initOffsetFromOrgin;
    int currentPoint = 0;
    public limbState currentLimbState;
    float currentTime;
    bool waitingForExit;

    [SerializeField] bool isBackLimb;
    public class limbState
    {
        public List<Vector2> points = new List<Vector2>();
        public float duration;
        public bool loop;
        public limbState(List<Vector2> _points,float _duration,bool _loop) 
        { 
            points = _points; 
            duration = _duration;
            loop = _loop;
        }
    }

    void Start()
    {
        initOffsetFromOrgin = transform.position - orgin.transform.position;
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
        float durationPerPoint = currentLimbState.duration / currentLimbState.points.Count;

        float t = currentTime / durationPerPoint;

        currentPoint = Mathf.FloorToInt(t) % currentLimbState.points.Count;

        float lerpAmmount = t - Mathf.Floor(t);

        int nextIndex = (currentPoint + 1) % currentLimbState.points.Count;
        
        Vector2 thisPoint = currentLimbState.points[currentPoint];
        Vector2 nextPoint = currentLimbState.points[nextIndex];

        Vector2 characterDirectionVec = new Vector2(Mathf.Sign(character.transform.localScale.x),1);
        Vector2 initRestPos = (Vector2)orgin.transform.position + (initOffsetFromOrgin * characterDirectionVec);

        Vector2 exactTarget = initRestPos + Vector2.Lerp(thisPoint, nextPoint, lerpAmmount);

        float _speed = 5f;
        transform.position = Vector2.Lerp(transform.position, exactTarget, Time.deltaTime * _speed);
    }   

    void Loop()
    {
        currentTime = 0;
        currentPoint = 0;
        waitingForExit = false;
    }
    public void ExitState()
    {
        currentTime = 0;
    }
}
