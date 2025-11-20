using System.Collections.Generic;
using UnityEngine;

public class LimbManager : MonoBehaviour
{
    Vector2 initTargetPos;
    int currentPoint = 0;
    public limbState currentLimbState;
    float currentTime;
    public class limbState
    {
        public List<Vector2> points = new List<Vector2>();
        public float duration;
        public bool loop;
        public limbState(List<Vector2> _points,float _duration,bool _loop) { }
    }

    void Start()
    {
        initTargetPos = transform.position;
    }

    void Update()
    {
        UpdatePoints();
    }
    void FixedUpdate()
    {
        currentTime++;
        if(currentTime == currentLimbState.duration)
        {
            if(currentLimbState.loop)
                Loop();
            else 
                ExitState();
        }
    }
    void UpdatePoints()
    {
        Vector2 thisPoint = currentLimbState.points[currentPoint];
        Vector2 nextPoint = currentLimbState.points[currentPoint + 1];
        
        float durationPerPoint = currentLimbState.duration / (currentLimbState.points.Count - 1);
        float lerpAmmount = (currentTime % durationPerPoint) / durationPerPoint;

        Vector2 exactTarget = Vector2.Lerp(thisPoint, nextPoint,lerpAmmount);
        float _speed = 1f;
        transform.position = Vector2.Lerp(transform.position,exactTarget,Time.deltaTime * _speed);
    }
    void Loop()
    {
        currentTime = 0;
    }
    public void ExitState()
    {
        currentTime = 0;
    }
}
