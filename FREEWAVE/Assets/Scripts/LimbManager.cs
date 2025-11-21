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

    }
    void UpdatePoints()
    {
        Vector2 thisPoint = currentLimbState.points[currentPoint];
        Vector2 nextPoint = currentLimbState.points[currentPoint + 1];
        
        float durationPerPoint = currentLimbState.duration / (currentLimbState.points.Count - 1);

        if (currentTime >= (currentPoint + 1) * durationPerPoint)
        {
            currentPoint++;

            if (currentPoint >= currentLimbState.points.Count - 1)
            {
                if(currentLimbState.loop)
                    Loop();
                else 
                    ExitState();
            }

            thisPoint = currentLimbState.points[currentPoint];
            nextPoint = currentLimbState.points[currentPoint + 1];
        }

        float lerpAmmount = (currentTime % durationPerPoint) / durationPerPoint;

        Vector2 characterDirectionVec = new Vector2(Mathf.Sign(character.transform.localScale.x),1);
        Vector2 initRestPos = (Vector2)orgin.transform.position + (initOffsetFromOrgin * characterDirectionVec);
        Vector2 exactTarget = initRestPos + Vector2.Lerp(thisPoint, nextPoint,lerpAmmount);
        float _speed = 2f;
        transform.position = Vector2.Lerp(transform.position,exactTarget,Time.deltaTime * _speed);
    }
    void Loop()
    {
        currentTime = 0;
        currentPoint = 0;
    }
    public void ExitState()
    {
        currentTime = 0;
    }
}
