using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] public LimbManager frontArm,backArm,frontLeg,backLeg;
    [SerializeField] GameObject spine1;

    GameObject spine2,head;
    float spine1angle,spine2angle,headAngle;
    PublicTimer currentStateTimer = new PublicTimer();
    public class bodyState
    {
        public LimbManager.limbState limb;
        protected CharacterAnimator characterAnimator;
        public float duration;
        public bodyState(LimbManager.limbState _limb,CharacterAnimator _characterAnimator)
        {
            limb = _limb;
            characterAnimator = _characterAnimator;
            duration = _limb.duration;
        }
    }
    public class lowerBodyState : bodyState
    {
        public lowerBodyState(LimbManager.limbState _limb,CharacterAnimator _characterAnimator) : base(_limb,_characterAnimator) {}

        public void EnterState()
        {
            characterAnimator.backLeg.currentLimbState = limb;
            characterAnimator.frontLeg.currentLimbState = limb;
        }
    }
    public class upperBodyState : bodyState
    {
        public bool loop;
        public float rotationDuration;
        public upperBodyState(LimbManager.limbState _limb,CharacterAnimator _characterAnimator,Vector2 spine1,Vector2 spine2,Vector2 head,bool _loop,float _rotationDuration) : base(_limb,_characterAnimator)
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
            characterAnimator.backArm.currentLimbState = limb;
            characterAnimator.frontArm.currentLimbState = limb;
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

    protected float standardTransitionTime = 300f,quickTransitionTime = 150f,onePointTime = 0.1f;

    public float runStateDuration,idleStateDuration;
    protected Character character;
    public virtual void CharacterAnimatorStart()
    {
        character = GetComponent<Character>();
        currentStateTimer.SetDuration(0f);
        currentStateTimer.Reset();

        spine2 = spine1.transform.GetChild(0).gameObject;
        head = spine2.transform.GetChild(0).gameObject;

        IdleObject = animationObjectHolder.transform.GetChild(0).gameObject;
        RunObject = animationObjectHolder.transform.GetChild(1).gameObject;
        JumpObject = animationObjectHolder.transform.GetChild(2).gameObject;
        AttackObject = animationObjectHolder.transform.GetChild(3).gameObject;
        DashAttackObject = animationObjectHolder.transform.GetChild(4).gameObject;
        DropAttackObject = animationObjectHolder.transform.GetChild(5).gameObject;
        HurtObject = animationObjectHolder.transform.GetChild(6).gameObject;

        float _attackTime = character.attackTimer.Duration; //align with timer 
        float _dashAttackTime = character.dashAttackTimer.Duration;

        //idle
        LimbManager.limbState _lowerIdle = new LimbManager.limbState(IdleObject.transform.GetChild(0).gameObject,idleStateDuration,true,frontLeg,standardTransitionTime,false);
        lowerBodyIdle = new lowerBodyState(_lowerIdle,this);

        LimbManager.limbState _upperIdle = new LimbManager.limbState(IdleObject.transform.GetChild(1).gameObject,idleStateDuration,true,frontArm,standardTransitionTime,false);
        upperBodyIdle = new upperBodyState(_upperIdle,this,Vector2.zero,Vector2.zero,Vector2.zero,true,0);

        //run
        LimbManager.limbState _lowerRun = new LimbManager.limbState(RunObject.transform.GetChild(0).gameObject,runStateDuration,true,frontLeg,standardTransitionTime,true);
        lowerBodyRun = new lowerBodyState(_lowerRun,this);

        LimbManager.limbState _upperRun = new LimbManager.limbState(RunObject.transform.GetChild(1).gameObject,runStateDuration,true,frontArm,standardTransitionTime,true);
        upperBodyRun = new upperBodyState(_upperRun,this,Vector2.zero,Vector2.zero,Vector2.zero,true,0);

        //jump
        LimbManager.limbState _lowerJump = new LimbManager.limbState(JumpObject.transform.GetChild(0).gameObject,onePointTime,false,frontLeg,standardTransitionTime,false);
        lowerBodyJump = new lowerBodyState(_lowerJump,this);

        LimbManager.limbState _upperJump = new LimbManager.limbState(JumpObject.transform.GetChild(1).gameObject,onePointTime,false,frontArm,standardTransitionTime,false);
        upperBodyJump = new upperBodyState(_upperJump,this,Vector2.zero,Vector2.zero,Vector2.zero,false,0);

        //attack
        LimbManager.limbState _lowerAttack = new LimbManager.limbState(AttackObject.transform.GetChild(0).gameObject,_attackTime,false,frontLeg,quickTransitionTime,false);
        lowerBodyAttack = new lowerBodyState(_lowerAttack,this);

        LimbManager.limbState _upperAttack = new LimbManager.limbState(AttackObject.transform.GetChild(1).gameObject,_attackTime,false,frontArm,quickTransitionTime,false);
        upperBodyAttack = new upperBodyState(_upperAttack,this,Vector2.zero,Vector2.zero,Vector2.zero,false,_attackTime);

        //dash attack
        LimbManager.limbState _dashAttack = new LimbManager.limbState(DashAttackObject.transform.GetChild(0).gameObject,_dashAttackTime,false,frontLeg,quickTransitionTime,false);
        lowerBodyDashAttack = new lowerBodyState(_dashAttack,this);

        LimbManager.limbState _upperDashAttack = new LimbManager.limbState(DashAttackObject.transform.GetChild(1).gameObject,_dashAttackTime,false,frontArm,quickTransitionTime,false);
        upperBodyDashAttack = new upperBodyState(_upperDashAttack,this,Vector2.zero,Vector2.zero,Vector2.zero,false,_dashAttackTime);

        //drop attack
        LimbManager.limbState _dropAttack = new LimbManager.limbState(DropAttackObject.transform.GetChild(0).gameObject,onePointTime,false,frontLeg,quickTransitionTime,false);
        lowerBodyDropAttack = new lowerBodyState(_dropAttack,this);

        LimbManager.limbState _upperDropAttack = new LimbManager.limbState(DropAttackObject.transform.GetChild(1).gameObject,onePointTime,false,frontArm,quickTransitionTime,false);
        upperBodyDropAttack = new upperBodyState(_upperDropAttack,this,Vector2.zero,Vector2.zero,Vector2.zero,false,0);

        //hurt
        LimbManager.limbState _hurt = new LimbManager.limbState(HurtObject.transform.GetChild(0).gameObject,onePointTime,false,frontLeg,quickTransitionTime,false);
        lowerBodyHurt = new lowerBodyState(_hurt,this);

        LimbManager.limbState _upperHurt = new LimbManager.limbState(HurtObject.transform.GetChild(1).gameObject,onePointTime,false,frontArm,quickTransitionTime,false);
        upperBodyHurt = new upperBodyState(_upperHurt,this,Vector2.zero,Vector2.zero,Vector2.zero,false,0);

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
        float t = currentStateTimer.Progress;
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
            currentStateTimer.SetDuration(currentUpperBodyState.rotationDuration);
            currentStateTimer.Reset();
            prevUpperBodyState = currentUpperBodyState;
        }

        if(currentStateTimer.TickLoop())
        {
            if(currentUpperBodyState.loop)
                currentUpperBodyState.loopRotations();
        }
    }
}
