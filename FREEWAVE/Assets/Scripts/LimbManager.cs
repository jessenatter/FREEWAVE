using System.Collections.Generic;
using UnityEngine;

public class LimbManager : MonoBehaviour
{
    [SerializeField] GameObject target;

    Vector2 initTargetPos;

    public limbState currentLimbState;
    public class limbState
    {
        List<Vector2> points = new List<Vector2>();
        public float duration,currentTime;
        public bool loop,reverseOnLoop;

        public void UpdateLimbState()
        {
            UpdatePoints();

            currentTime++;
            if(currentTime == duration)
            {
                if(loop)
                    Loop();
                else 
                    ExitState();
            }
        }

        void UpdatePoints()
        {
            
        }

        void Loop()
        {
            currentTime = 0;
        }

        public void ExitState() //make sure u call when u switch state
        {
            currentTime = 0;
        }
    }

    void Start()
    {
        initTargetPos = target.transform.position;
    }

    void Update()
    {
        
    }
}
