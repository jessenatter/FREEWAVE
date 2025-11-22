using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] LimbManager frontArm,backArm,frontLeg,backLeg;
    [SerializeField] GameObject spine1,spine2,head;
    float spine1angle,spine2angle,headAngle;
    float currentStateTimer;
    public class bodyState
    {
        public LimbManager.limbState front,back;
        protected CharacterAnimator characterAnimator;
        public float duration;
        public bodyState(LimbManager.limbState _front,LimbManager.limbState _back,CharacterAnimator _characterAnimator,float _duration)
        {
            front = _front;
            back = _back;
            characterAnimator = _characterAnimator;
            duration = _duration;
        }
    }
    public class lowerBodyState : bodyState
    {
        public lowerBodyState(LimbManager.limbState _front, LimbManager.limbState _back,CharacterAnimator _characterAnimator,float _duration) : base(_front, _back,_characterAnimator,_duration) {}

        public void EnterState()
        {
            characterAnimator.backLeg.currentLimbState = back;
            characterAnimator.frontLeg.currentLimbState = front;
        }
    }
    public class upperBodyState : bodyState
    {
        public bool loop;
        public upperBodyState(LimbManager.limbState _front, LimbManager.limbState _back,CharacterAnimator _characterAnimator,Vector2 spine1,Vector2 spine2,Vector2 head,float _duration,bool _loop) : base(_front, _back,_characterAnimator,_duration)
        {
            spine1rotation = spine1;
            spine2rotation = spine2;
            headRotation = head;
            loop = _loop;
        }
        public Vector2 spine1rotation,spine2rotation,headRotation;
        public void EnterState()
        {
            characterAnimator.backArm.currentLimbState = back;
            characterAnimator.frontArm.currentLimbState = front;
        }

        public void loopRotations()
        {
            spine1rotation = new Vector2(spine1rotation.y,spine1rotation.x);
            spine2rotation = new Vector2(spine2rotation.y,spine2rotation.x);
            headRotation = new Vector2(headRotation.y,headRotation.x);
        }
    }
    public lowerBodyState currentLowerBodyState;
    public upperBodyState currentUpperBodyState;
    
    upperBodyState prevUpperBodyState;
    public lowerBodyState lowerBodyRun,lowerBodyJump,lowerBodyIdle;
    public upperBodyState upperBodyRun,upperBodyJump,upperBodyIdle;
    float idleStateDuration = 150f,runStateDuration = 30f;
    void Start()
    {
        //idle
        List<Vector2> _lowerIdlePoints = new List<Vector2>();
        _lowerIdlePoints.Add(Vector2.zero); 
        _lowerIdlePoints.Add(Vector2.zero);
        LimbManager.limbState _lowerIdle = new LimbManager.limbState(_lowerIdlePoints,idleStateDuration,true);
        lowerBodyIdle = new lowerBodyState(_lowerIdle,_lowerIdle,this,idleStateDuration);

        List<Vector2> _upperIdlePoints = new List<Vector2>();
        _upperIdlePoints.Add(Vector2.zero); 
        _upperIdlePoints.Add(new Vector2(0,0.01f));
        LimbManager.limbState _upperIdle = new LimbManager.limbState(_upperIdlePoints,idleStateDuration,true);
        Vector2 idleRotationSpine2 = new Vector2(3,-3);
        Vector2 idleRotationHead = new Vector2(3,-3);
        upperBodyIdle = new upperBodyState(_upperIdle,_upperIdle,this,Vector2.zero,idleRotationSpine2,idleRotationHead,idleStateDuration,true);

        //run
        List<Vector2> _lowerRunPoints = new List<Vector2>();
        _lowerRunPoints.Add(new Vector2(-0.5f,0.2f));
        _lowerRunPoints.Add(new Vector2(-0.2f,0.3f));
        _lowerRunPoints.Add(new Vector2(0.1f,0.1f)); 
        _lowerRunPoints.Add(Vector2.zero);
        LimbManager.limbState _lowerRun = new LimbManager.limbState(_lowerRunPoints,runStateDuration,true);
        lowerBodyRun = new lowerBodyState(_lowerRun,_lowerRun,this,runStateDuration);

        List<Vector2> _upperRunPoints = new List<Vector2>();
        _upperRunPoints.Add(new Vector2(0f,0.3f)); 
        _upperRunPoints.Add(new Vector2(0.1f,0.4f));
        _upperRunPoints.Add(new Vector2(-0.1f,0.4f));
        _upperRunPoints.Add(new Vector2(0,0.3f));
        LimbManager.limbState _upperRun = new LimbManager.limbState(_upperRunPoints,runStateDuration,true);
        Vector2 spine2runRotation = new Vector2(350,345);
        upperBodyRun = new upperBodyState(_upperRun,_upperRun,this,spine2runRotation,Vector2.zero,Vector2.zero,runStateDuration,true);

        //jump
        List<Vector2> _lowerJumpPoints = new List<Vector2>(); 
        _lowerJumpPoints.Add(new Vector2(0.1f,0.2f));
        LimbManager.limbState _lowerJump = new LimbManager.limbState(_lowerJumpPoints,0.1f,false);
        lowerBodyJump = new lowerBodyState(_lowerJump,_lowerJump,this,1);

        List<Vector2> _upperJumpPoints = new List<Vector2>();
        _upperJumpPoints.Add(new Vector2(0.3f,0.7f)); 
        LimbManager.limbState _upperJump = new LimbManager.limbState(_upperJumpPoints,0.1f,false);
        upperBodyJump = new upperBodyState(_upperJump,_upperJump,this,Vector2.zero,Vector2.zero,Vector2.zero,1,false);

        currentLowerBodyState = lowerBodyIdle;
        currentUpperBodyState = upperBodyIdle;

        currentLowerBodyState.EnterState();
        currentUpperBodyState.EnterState();

        prevUpperBodyState = currentUpperBodyState;
    }
    void Update()
    {
        RotationUpdate();
    }

    void RotationUpdate()
    {
        float t = currentStateTimer / currentUpperBodyState.duration;
        float lerpSpeed = 5f;

        float spine1targetAngle = Mathf.Lerp(currentUpperBodyState.spine1rotation.x,currentUpperBodyState.spine1rotation.y,t);
        float spine2targetAngle = Mathf.Lerp(currentUpperBodyState.spine2rotation.x,currentUpperBodyState.spine2rotation.y,t);
        float headTargetAngle = Mathf.Lerp(currentUpperBodyState.headRotation.x,currentUpperBodyState.headRotation.y,t);

        spine1angle = Mathf.Lerp(spine1.transform.eulerAngles.z,spine1targetAngle,lerpSpeed);
        spine2angle = Mathf.Lerp(spine2.transform.eulerAngles.z,spine2targetAngle,lerpSpeed);
        headAngle = Mathf.Lerp(head.transform.eulerAngles.z,headTargetAngle,lerpSpeed);

        int xDir = (int)Mathf.Sign(transform.localScale.x);
        spine1.transform.eulerAngles = new Vector3(0, 0, spine1angle * xDir+ 90 * xDir);
        spine2.transform.eulerAngles = new Vector3(0, 0, spine1angle* xDir + spine2angle * xDir+ 90 * xDir);
        head.transform.eulerAngles = new Vector3(0, 0, spine1angle* xDir + spine2angle * xDir+ headAngle * xDir+ 90 * xDir);
    }
    void FixedUpdate()
    {
        if(currentUpperBodyState != prevUpperBodyState)
        {
            currentStateTimer = 0;
            prevUpperBodyState = currentUpperBodyState;
        }

        currentStateTimer++;
        if(currentStateTimer == currentUpperBodyState.duration)
        {
            currentStateTimer = 0;
            if(currentUpperBodyState.loop)
                currentUpperBodyState.loopRotations();
        }
    }
}
