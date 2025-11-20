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

    float idleStateDuration = 100f;
    void Start()
    {
        List<Vector2> _lowerIdlePoints = new List<Vector2>(); //these vector2s need to be treated as offsets
        _lowerIdlePoints.Add(Vector2.zero); 
        _lowerIdlePoints.Add(Vector2.zero);
        LimbManager.limbState _lowerIdle = new LimbManager.limbState(_lowerIdlePoints,idleStateDuration,true);
        lowerBodyIdle = new lowerBodyState(_lowerIdle,_lowerIdle,this);

        List<Vector2> _upperIdlePoints = new List<Vector2>();
        _upperIdlePoints.Add(Vector2.zero); 
        _upperIdlePoints.Add(new Vector2(0,0.3f));
        LimbManager.limbState _upperIdle = new LimbManager.limbState(_upperIdlePoints,idleStateDuration,true);
        upperBodyIdle = new upperBodyState(_upperIdle,_upperIdle,this);

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

    void EnterBodyState()
    {
        
    }
}
