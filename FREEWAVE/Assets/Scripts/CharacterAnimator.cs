using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] LimbManager frontArm,backArm,frontLeg,backLeg;
    [SerializeField] GameObject spine1,spine2,head;
    public class bodyState
    {
        public LimbManager.limbState front,back;
        protected CharacterAnimator characterAnimator;
        public bodyState(LimbManager.limbState _front,LimbManager.limbState _back,CharacterAnimator _characterAnimator)
        {
            front = _front;
            back = _back;
            characterAnimator = _characterAnimator;
        }
    }
    public class lowerBodyState : bodyState
    {
        public lowerBodyState(LimbManager.limbState _front, LimbManager.limbState _back,CharacterAnimator _characterAnimator) : base(_front, _back,_characterAnimator) {}

        public void EnterState()
        {
            characterAnimator.backLeg.currentLimbState = back;
            characterAnimator.frontLeg.currentLimbState = front;
        }
    }
    public class upperBodyState : bodyState
    {
        public upperBodyState(LimbManager.limbState _front, LimbManager.limbState _back,CharacterAnimator _characterAnimator) : base(_front, _back,_characterAnimator) {}

        public void EnterState()
        {
            characterAnimator.backArm.currentLimbState = back;
            characterAnimator.frontArm.currentLimbState = front;
        }
    }
    public lowerBodyState currentLowerBodyState;
    public upperBodyState currentUpperBodyState;
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
        lowerBodyIdle = new lowerBodyState(_lowerIdle,_lowerIdle,this);

        List<Vector2> _upperIdlePoints = new List<Vector2>();
        _upperIdlePoints.Add(Vector2.zero); 
        _upperIdlePoints.Add(new Vector2(0,0.01f));
        LimbManager.limbState _upperIdle = new LimbManager.limbState(_upperIdlePoints,idleStateDuration,true);
        upperBodyIdle = new upperBodyState(_upperIdle,_upperIdle,this);

        //run
        List<Vector2> _lowerRunPoints = new List<Vector2>();
        _lowerRunPoints.Add(new Vector2(-0.3f,0.1f));
        _lowerRunPoints.Add(new Vector2(0,0.2f));
        _lowerRunPoints.Add(new Vector2(0.1f,0.1f)); 
        _lowerRunPoints.Add(Vector2.zero);
        LimbManager.limbState _lowerRun = new LimbManager.limbState(_lowerRunPoints,runStateDuration,true);
        lowerBodyRun = new lowerBodyState(_lowerRun,_lowerRun,this);

        List<Vector2> _upperRunPoints = new List<Vector2>();
        _upperRunPoints.Add(Vector2.zero); 
        _upperRunPoints.Add(new Vector2(0,0.01f));
        LimbManager.limbState _upperRun = new LimbManager.limbState(_upperRunPoints,runStateDuration,true);
        upperBodyRun = new upperBodyState(_upperRun,_upperRun,this);

        //jump
        List<Vector2> _lowerJumpPoints = new List<Vector2>(); 
        _lowerJumpPoints.Add(new Vector2(0.1f,0.2f));
        LimbManager.limbState _lowerJump = new LimbManager.limbState(_lowerJumpPoints,0.1f,false);
        lowerBodyJump = new lowerBodyState(_lowerJump,_lowerJump,this);

        List<Vector2> _upperJumpPoints = new List<Vector2>();
        _upperJumpPoints.Add(new Vector2(0.3f,0.7f)); 
        LimbManager.limbState _upperJump = new LimbManager.limbState(_upperJumpPoints,0.1f,false);
        upperBodyJump = new upperBodyState(_upperJump,_upperJump,this);

        currentLowerBodyState = lowerBodyIdle;
        currentUpperBodyState = upperBodyIdle;

        currentLowerBodyState.EnterState();
        currentUpperBodyState.EnterState();
    }

    void Update()
    {
        
    }
    void FixedUpdate()
    {
        
    }
}
