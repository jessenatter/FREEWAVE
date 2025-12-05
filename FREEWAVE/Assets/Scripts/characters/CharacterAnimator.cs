using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] public LimbManager frontArm,backArm,frontLeg,backLeg;
    [SerializeField] GameObject spine1;

    GameObject spine2,head;
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
        public float rotationDuration;
        public upperBodyState(LimbManager.limbState _front, LimbManager.limbState _back,CharacterAnimator _characterAnimator,Vector2 spine1,Vector2 spine2,Vector2 head,float _duration,bool _loop,float _rotationDuration) : base(_front, _back,_characterAnimator,_duration)
        {
            spine1rotation = spine1;
            spine2rotation = spine2;
            headRotation = head;
            loop = _loop;
            rotationDuration = _rotationDuration;
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
    
    [SerializeField] protected GameObject animationObjectHolder;
    GameObject IdleObject,RunObject,JumpObject,AttackObject,DashAttackObject,DropAttackObject,HurtObject;

    public lowerBodyState lowerBodyRun,lowerBodyJump,lowerBodyIdle;
    public upperBodyState upperBodyRun,upperBodyJump,upperBodyIdle;

    public lowerBodyState lowerBodyAttack,lowerBodyDashAttack,lowerBodyDropAttack;
    public upperBodyState upperBodyAttack,upperBodyDashAttack,upperBodyDropAttack;

    public lowerBodyState lowerBodyHurt;
    public upperBodyState upperBodyHurt;

    [HideInInspector]public float idleStateDuration = 150f,runStateDuration = 30f,hurtStateDuration = 0.1f;
    [HideInInspector]public float attackStateDuration = 30f,dashAttackStateDuration = 40f,dropAttackStateDuration = 30f;

    protected float standardTransitionTime = 300f,quickTransitionTime = 150f;
    public virtual void CharacterAnimatorStart()
    {
        spine2 = spine1.transform.GetChild(0).gameObject;
        head = spine2.transform.GetChild(0).gameObject;

        IdleObject = animationObjectHolder.transform.GetChild(0).gameObject;
        RunObject = animationObjectHolder.transform.GetChild(1).gameObject;
        JumpObject = animationObjectHolder.transform.GetChild(2).gameObject;
        AttackObject = animationObjectHolder.transform.GetChild(3).gameObject;
        DashAttackObject = animationObjectHolder.transform.GetChild(4).gameObject;
        DropAttackObject = animationObjectHolder.transform.GetChild(5).gameObject;
        HurtObject = animationObjectHolder.transform.GetChild(6).gameObject;

        //idle
        LimbManager.limbState _lowerIdle = new LimbManager.limbState(IdleObject.transform.GetChild(0).gameObject,idleStateDuration,true,frontLeg,standardTransitionTime,false);
        lowerBodyIdle = new lowerBodyState(_lowerIdle,_lowerIdle,this,idleStateDuration);

        LimbManager.limbState _upperIdle = new LimbManager.limbState(IdleObject.transform.GetChild(1).gameObject,idleStateDuration,true,frontArm,standardTransitionTime,false);
        Vector2 idleRotationSpine2 = new Vector2(3,-3);
        Vector2 idleRotationHead = new Vector2(3,-3);
        upperBodyIdle = new upperBodyState(_upperIdle,_upperIdle,this,Vector2.zero,idleRotationSpine2,idleRotationHead,idleStateDuration,true,idleStateDuration);

        //run
        LimbManager.limbState _lowerRun = new LimbManager.limbState(RunObject.transform.GetChild(0).gameObject,runStateDuration,true,frontLeg,standardTransitionTime,true);
        lowerBodyRun = new lowerBodyState(_lowerRun,_lowerRun,this,runStateDuration);

        LimbManager.limbState _upperRun = new LimbManager.limbState(RunObject.transform.GetChild(1).gameObject,runStateDuration,true,frontArm,standardTransitionTime,true);
        Vector2 spine2runRotation = new Vector2(350,345);
        upperBodyRun = new upperBodyState(_upperRun,_upperRun,this,spine2runRotation,Vector2.zero,Vector2.zero,runStateDuration,true,runStateDuration);

        //jump
        LimbManager.limbState _lowerJump = new LimbManager.limbState(JumpObject.transform.GetChild(0).gameObject,0.1f,false,frontLeg,standardTransitionTime,false);
        lowerBodyJump = new lowerBodyState(_lowerJump,_lowerJump,this,1);

        LimbManager.limbState _upperJump = new LimbManager.limbState(JumpObject.transform.GetChild(1).gameObject,0.1f,false,frontArm,standardTransitionTime,false);
        upperBodyJump = new upperBodyState(_upperJump,_upperJump,this,Vector2.zero,Vector2.zero,Vector2.zero,1,false,0);

        //attack
        LimbManager.limbState _lowerAttack = new LimbManager.limbState(AttackObject.transform.GetChild(0).gameObject,attackStateDuration,false,frontLeg,quickTransitionTime,false);
        lowerBodyAttack = new lowerBodyState(_lowerAttack,_lowerAttack,this,attackStateDuration);

        Vector2 upperBodySpine1AttackRotation = new Vector2(5,-10);
        Vector2 upperBodySpine2AttackRotation = new Vector2(5,-10);
        LimbManager.limbState _upperAttack = new LimbManager.limbState(AttackObject.transform.GetChild(1).gameObject,attackStateDuration,false,frontArm,quickTransitionTime,false);
        upperBodyAttack = new upperBodyState(_upperAttack,_upperAttack,this,upperBodySpine1AttackRotation,upperBodySpine2AttackRotation,Vector2.zero,attackStateDuration,false,attackStateDuration);

        //dash attack
        LimbManager.limbState _dashAttack = new LimbManager.limbState(DashAttackObject.transform.GetChild(0).gameObject,dashAttackStateDuration,false,frontLeg,quickTransitionTime,false);
        lowerBodyDashAttack = new lowerBodyState(_dashAttack,_dashAttack,this,attackStateDuration);

        Vector2 upperBodySpine1DashAttackRotation = new Vector2(10,-20);
        Vector2 upperBodySpine2DashAttackRotation = new Vector2(5,-10);
        LimbManager.limbState _upperDashAttack = new LimbManager.limbState(DashAttackObject.transform.GetChild(1).gameObject,attackStateDuration,false,frontArm,quickTransitionTime,false);
        upperBodyDashAttack = new upperBodyState(_upperDashAttack,_upperDashAttack,this,upperBodySpine1DashAttackRotation,upperBodySpine2DashAttackRotation,Vector2.zero,dashAttackStateDuration,false,dashAttackStateDuration);

        //drop attack
        LimbManager.limbState _dropAttack = new LimbManager.limbState(DropAttackObject.transform.GetChild(0).gameObject,dropAttackStateDuration,false,frontLeg,quickTransitionTime,false);
        lowerBodyDropAttack = new lowerBodyState(_dropAttack,_dropAttack,this,dropAttackStateDuration);

        LimbManager.limbState _upperDropAttack = new LimbManager.limbState(DropAttackObject.transform.GetChild(1).gameObject,attackStateDuration,false,frontArm,quickTransitionTime,false);
        upperBodyDropAttack = new upperBodyState(_upperDropAttack,_upperDropAttack,this,Vector2.zero,Vector2.zero,Vector2.zero,dropAttackStateDuration,false,0);

        //hurt
        LimbManager.limbState _hurt = new LimbManager.limbState(HurtObject.transform.GetChild(0).gameObject,0.1f,false,frontLeg,quickTransitionTime,false);
        lowerBodyHurt = new lowerBodyState(_hurt,_hurt,this,1);

        Vector2 upperBodySpine2HurtRotation = new Vector2(30,30);
        LimbManager.limbState _upperHurt = new LimbManager.limbState(HurtObject.transform.GetChild(1).gameObject,0.1f,false,frontArm,quickTransitionTime,false);
        upperBodyHurt = new upperBodyState(_upperHurt,_upperHurt,this,Vector2.zero,upperBodySpine2HurtRotation,Vector2.zero,1,false,hurtStateDuration);


        currentLowerBodyState = lowerBodyIdle;
        currentUpperBodyState = upperBodyIdle;

        currentLowerBodyState.EnterState();
        currentUpperBodyState.EnterState();

        prevUpperBodyState = currentUpperBodyState;
    }
    public void CharacterAnimatorUpdate()
    {
        RotationUpdate();
    }

    void RotationUpdate()
    {
        float t = currentStateTimer / currentUpperBodyState.rotationDuration;
        float lerpSpeed = 1f;

        float spine1targetAngle = Mathf.Lerp(currentUpperBodyState.spine1rotation.x,currentUpperBodyState.spine1rotation.y,t);
        float spine2targetAngle = Mathf.Lerp(currentUpperBodyState.spine2rotation.x,currentUpperBodyState.spine2rotation.y,t);
        float headTargetAngle = Mathf.Lerp(currentUpperBodyState.headRotation.x,currentUpperBodyState.headRotation.y,t);

        spine1angle = Mathf.Lerp(spine1.transform.eulerAngles.z,spine1targetAngle,lerpSpeed);
        spine2angle = Mathf.Lerp(spine2.transform.eulerAngles.z,spine2targetAngle,lerpSpeed);
        headAngle = Mathf.Lerp(head.transform.eulerAngles.z,headTargetAngle,lerpSpeed);

        int xDir = (int) Mathf.Sign(transform.localScale.x);
        spine1.transform.eulerAngles = new Vector3(0, 0, spine1angle * xDir + 90 * xDir);
        spine2.transform.eulerAngles = new Vector3(0, 0, spine1angle * xDir + spine2angle * xDir + 90 * xDir);
        head.transform.eulerAngles = new Vector3(0, 0, spine1angle * xDir + spine2angle * xDir + headAngle * xDir + 90 * xDir);
    }
    public void CharacterAnimatorFixedUpdate()
    {
        if(currentUpperBodyState != prevUpperBodyState)
        {
            currentStateTimer = 0;
            prevUpperBodyState = currentUpperBodyState;
        }

        currentStateTimer++;
        if(currentStateTimer == currentUpperBodyState.rotationDuration)
        {
            currentStateTimer = 0;
            if(currentUpperBodyState.loop)
                currentUpperBodyState.loopRotations();
        }
    }
}
