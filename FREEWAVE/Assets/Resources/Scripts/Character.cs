using System.Collections.Generic;
using UnityEngine;
using System;

public class Character : PrimaryClass
{
    protected string name;
    public GameObject gameObject,spine1,spine2, head;
    public Limb frontArm, backArm, frontLeg, backLeg;
    public float armMaxRadius, legMaxRadius;
    List<Limb> limbs = new List<Limb>();
    public int xDir;
    public Rigidbody2D rb;
    public BoxCollider2D bc;
    SpriteRenderer sr;
    public Vector2 spawnPoint,climbPoint;
    public float moveSpeed = 1f,jumpForce = 5f, rotationLerp = 0.5f,tiltRotation = 10;
    public UpperBodyState currentUpperBodyState;
    public LowerBodyState currentLowerBodyState;
    public Sprite[] sprites;

    public LowerBodyIdle lowerBodyIdle = new LowerBodyIdle();
    public LowerBodyRun lowerBodyRun = new LowerBodyRun();
    public LowerBodyJump lowerBodyJump = new LowerBodyJump();
    public LowerBodyClimb lowerBodyClimb = new LowerBodyClimb();

    public UpperBodyIdle upperBodyIdle = new UpperBodyIdle();
    public UpperBodyRun upperBodyRun = new UpperBodyRun();
    public UpperBodyJump upperBodyJump = new UpperBodyJump();
    public UpperBodyClimb upperBodyClimb = new UpperBodyClimb();

    public bool grounded,canJump = true;
    protected float groundedCD = 10, groundedTimer;

    override public void Start(Manager _manager)
    {
        base.Start(_manager);

        sprites = Resources.LoadAll<Sprite>("Sprites/Characters/" + name.ToString());

        gameObject = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Character/Character"));

        rb = gameObject.GetComponent<Rigidbody2D>();
        bc = gameObject.GetComponent<BoxCollider2D>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        sr.sprite = sprites[0];
        gameObject.transform.position = spawnPoint;

        spine1 = gameObject.transform.GetChild(0).gameObject;
        spine2 = spine1.transform.GetChild(0).gameObject;
        head = spine2.transform.GetChild(0).gameObject;

        frontLeg = StartLimb(true, false,gameObject.transform.GetChild(1).gameObject);
        backLeg = StartLimb(true, true, gameObject.transform.GetChild(2).gameObject);
        frontArm = StartLimb(false, false,spine2.transform.GetChild(1).gameObject);
        backArm = StartLimb(false, true,spine2.transform.GetChild(2).gameObject);

        limbs.AddRange(new[] { frontArm, backArm, frontLeg, backLeg });

        head.GetComponent<SpriteRenderer>().sprite = sprites[5];

        currentLowerBodyState = lowerBodyIdle;
        currentUpperBodyState = upperBodyIdle;

        armMaxRadius = frontArm.lengthCmax;
        legMaxRadius = frontLeg.lengthCmax;

        List<BodyState> _bodyStates = new List<BodyState>();
        _bodyStates.AddRange(new BodyState[] { lowerBodyIdle,lowerBodyJump,lowerBodyRun,upperBodyIdle,upperBodyJump,upperBodyRun,lowerBodyClimb,upperBodyClimb });

        foreach (BodyState bodyState in _bodyStates)
            bodyState.Start(this);
    }

    Limb StartLimb(bool isLeg, bool isBackLimb,GameObject _gameObject)
    {
        Limb limb = new Limb();
        limb.Start(isBackLimb, isLeg,_gameObject,this);

        if (isLeg)
        {
            limb.srA.sprite = sprites[1];
            limb.srB.sprite = sprites[2];
        }
        else
        {
            limb.srA.sprite = sprites[3];
            limb.srB.sprite = sprites[4];
        }

        return limb;
    }

    override public void Update()
    {
        GroundedCheck();
        ClimbCheck();

        foreach (Limb limb in limbs)
            limb.Update();

        currentLowerBodyState.StateUpdate();
        currentUpperBodyState.StateUpdate();

        UpdateRotations();
    }

    void GroundedCheck()
    {
        Vector2 boxSize = new Vector2(bc.size.x * 0.8f, bc.size.y * 0.8f);
        RaycastHit2D hit = Physics2D.BoxCast((Vector2)gameObject.transform.position + Vector2.down * 0.1f, boxSize, 0, Vector2.down, 0.3f, manager.groundMask);

        if (hit.collider != null)
            grounded = true;
        else if (grounded)
        {
            groundedTimer++;
            if (groundedTimer == groundedCD)
            {
                grounded = false;
                groundedTimer = 0;
            }
        }
    }

    void ClimbCheck()
    {
        Vector2 climbOrgin = new Vector2(gameObject.transform.position.x + Mathf.Sign(gameObject.transform.localScale.x) * 0.1f, gameObject.transform.position.y + 0.2f);

        float distance = 0.25f;
        Vector2 boxSize = new Vector2(bc.size.x * 0.5f, bc.size.y * 0.25f);
        RaycastHit2D top = Physics2D.BoxCast(climbOrgin, boxSize, 0, Vector2.up, distance, manager.groundMask);
        RaycastHit2D bottom = Physics2D.BoxCast(climbOrgin, boxSize, 0, Vector2.down, distance, manager.groundMask);

        if(top.collider == null && bottom.collider != null)
            Climb();
    }

    protected void Jump()
    {
        if (currentLowerBodyState != lowerBodyClimb)
        {
            if (grounded && canJump)
            {
                canJump = false;
                currentLowerBodyState.StateExit(lowerBodyJump);
                currentUpperBodyState.StateExit(upperBodyJump);
            }
        }
        else if(!lowerBodyClimb.climbing)
        {
            lowerBodyClimb.climbing = true;
        }
    }

    protected void Climb()
    {
        if (currentLowerBodyState != lowerBodyClimb)
        {
            currentLowerBodyState.StateExit(lowerBodyClimb);
            currentUpperBodyState.StateExit(upperBodyClimb);
        }
    }

    protected void UpdateRotations()
    {
        float bodyRotation = Mathf.LerpAngle(gameObject.transform.eulerAngles.z, currentLowerBodyState.rotation, rotationLerp);
        float spine1rotation = Mathf.LerpAngle(spine1.transform.eulerAngles.z, bodyRotation + currentUpperBodyState.spine1rotation + 90 * Mathf.Sign(gameObject.transform.localScale.x), rotationLerp);
        float spine2rotation = Mathf.LerpAngle(spine2.transform.eulerAngles.z, spine1rotation + currentUpperBodyState.spine2rotation, rotationLerp);
        float headRotation = Mathf.LerpAngle(head.transform.eulerAngles.z, spine2rotation + currentUpperBodyState.headRotation - 90 * Mathf.Sign(gameObject.transform.localScale.x), rotationLerp);

        gameObject.transform.rotation = Quaternion.Euler(0, 0, bodyRotation);
        spine1.transform.rotation = Quaternion.Euler(0, 0, spine1rotation);
        spine2.transform.rotation = Quaternion.Euler(0, 0, spine2rotation);
        head.transform.rotation = Quaternion.Euler(0, 0, headRotation);
    }
}

public class Player : Character
{
    public override void Start(Manager _manager)
    {
        name = "Player";
        moveSpeed = 4.5f;
        jumpForce = 5.5f;

        base.Start(_manager);
        GameObject face = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Character/Face"));
        face.transform.position = head.transform.position;
        face.transform.SetParent(head.transform);
    }

    public override void Update()
    {
        xDir = MathF.Sign(manager.moveAction.ReadValue<Vector2>().x);

        if (manager.jumpAction.IsPressed())
            Jump();

        base.Update();
    }
}

public class BodyState 
{
    protected Character character;
    protected LimbMode limbMode;
    public float rotation;

    public virtual void Start(Character _character) { character = _character; }

    public virtual void StateUpdate() { }

    public virtual void StateExit(BodyState nextState) { }

    public virtual void StateEnter() { }

    protected virtual LimbMode CreateLimbMode() { return null; }
}

public class UpperBodyState : BodyState
{
    public float headRotation, spine1rotation, spine2rotation;

    public override void StateEnter()
    {
        base.StateEnter();

        limbMode = CreateLimbMode();

        character.frontArm.currentLimbMode = limbMode;
        character.backArm.currentLimbMode = limbMode;
    }

    public override void StateExit(BodyState nextState)
    {
        base.StateExit(nextState);
        character.currentUpperBodyState = (UpperBodyState)nextState;
        nextState.StateEnter();
    }
}

public class LowerBodyState : BodyState
{
    public override void StateEnter()
    {
        base.StateEnter();

        limbMode = CreateLimbMode();

        character.frontLeg.currentLimbMode = limbMode;
        character.backLeg.currentLimbMode = limbMode;
    }

    public override void StateExit(BodyState nextState)
    {
        base.StateExit(nextState);
        character.currentLowerBodyState = (LowerBodyState)nextState;
        nextState.StateEnter();
    }
}

#region //upperbody states
public class UpperBodyIdle : UpperBodyState
{
    public override void Start(Character _character) { base.Start(_character); }

    protected override LimbMode CreateLimbMode() { return null; }

    public override void StateUpdate()
    {
        base.StateUpdate();
        if (character.xDir != 0) StateExit(character.upperBodyRun);
    }
}

public class UpperBodyRun : UpperBodyState
{
    protected override LimbMode CreateLimbMode()
    {
        ThreePoints threePoints = new ThreePoints();
        threePoints.pointA = new Vector2(-0.8f, -0.6f) * character.armMaxRadius;
        threePoints.pointB = new Vector2(0.8f, -0.6f) * character.armMaxRadius;
        threePoints.pointC = new Vector2(0f, -0.1f) * character.armMaxRadius;
        threePoints.duration = 75;
        threePoints.initDuration = threePoints.duration;
        threePoints.loop = true;
        return threePoints;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        if (character.gameObject.transform.localScale.x != character.xDir && character.xDir != 0)
            character.gameObject.transform.localScale = new Vector2(character.xDir, 1);

        if (character.xDir == 0) StateExit(character.upperBodyIdle);
    }
}

public class UpperBodyJump : UpperBodyState
{
    float reach = 0.5f;

    protected override LimbMode CreateLimbMode()
    {
        FollowVector2 followVector2 = new FollowVector2();
        followVector2.vector2 = new Vector2(0, -reach) * character.armMaxRadius;
        return followVector2;
    }

    public override void StateUpdate()
    {
        if (character.grounded == true && character.rb.linearVelocityY <= 0)
            StateExit(character.upperBodyIdle);

        if (character.gameObject.transform.localScale.x != character.xDir && character.xDir != 0)
            character.gameObject.transform.localScale = new Vector2(character.xDir, 1);

        base.StateUpdate();
    }
}

public class UpperBodyClimb : UpperBodyState
{
    protected override LimbMode CreateLimbMode()
    {
        FollowVector2 followVector2 = new FollowVector2();
        followVector2.vector2 = new Vector2(character.armMaxRadius, character.armMaxRadius);
        return followVector2;
    }
}

#endregion

#region //lowerbody states
public class LowerBodyIdle : LowerBodyState
{
    float decelerationLerp = 0.2f;

    protected override LimbMode CreateLimbMode() { return null; }

    public override void StateUpdate()
    {
        base.StateUpdate();
        if (character.xDir != 0) StateExit(character.lowerBodyRun);
        character.rb.linearVelocityX = Mathf.Lerp(character.rb.linearVelocityX, 0, decelerationLerp);
    }

    public override void StateEnter()
    {
        base.StateEnter();
        character.canJump = true;
    }
}

public class LowerBodyRun : LowerBodyState
{
    float reach = 0.7f;

    protected override LimbMode CreateLimbMode()
    {
        ThreePoints threePoints = new ThreePoints();
        threePoints.pointA = new Vector2(reach, -1) * character.legMaxRadius;
        threePoints.pointB = new Vector2(-1, -1) * character.legMaxRadius;
        threePoints.pointC = new Vector2(0, -reach) * character.legMaxRadius;
        threePoints.duration = 60;
        threePoints.initDuration = threePoints.duration;
        threePoints.loop = true;
        return threePoints;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        if (character.xDir == 0) StateExit(character.lowerBodyIdle);
        else
            character.rb.linearVelocity = new Vector2(character.moveSpeed * character.xDir, character.rb.linearVelocity.y);

        rotation = -character.xDir * character.tiltRotation;
    }
}

public class LowerBodyJump : LowerBodyState
{
    protected override LimbMode CreateLimbMode()
    {
        FollowVector2 followVector2 = new FollowVector2();
        followVector2.vector2 = new Vector2(character.legMaxRadius * 0.3f, -character.legMaxRadius * 0.6f);
        return followVector2;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
        character.rb.linearVelocity = new Vector2((character.moveSpeed * .75f) * character.xDir, character.rb.linearVelocity.y);

        if (character.grounded == true && character.rb.linearVelocityY <= 0)
            StateExit(character.lowerBodyIdle);

        rotation = -character.xDir * character.tiltRotation;
    }

    public override void StateEnter()
    {
        base.StateEnter();
        character.rb.AddForce(character.jumpForce * Vector2.up, ForceMode2D.Impulse);
    }
}

public class LowerBodyClimb : LowerBodyState
{
    float climbSpeed = .2f;
    public bool climbing = false, hitY;

    protected override LimbMode CreateLimbMode()
    {
        // if you want a specific mode for climb, return it here
        return null;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if(climbing)
        {
            Vector2 characterFixedPos = new Vector2(character.gameObject.transform.position.x,character.gameObject.transform.position.y - (character.bc.size.y/2));

            if(!hitY)
            { 
                character.gameObject.transform.Translate(Vector2.up * climbSpeed);
                if (characterFixedPos.y > character.climbPoint.y)
                    hitY = true;
            }
            else
            {
                character.gameObject.transform.Translate(new Vector2(MathF.Sign(character.gameObject.transform.localScale.x),0)* climbSpeed);

                if (character.grounded)
                    StateExit(character.lowerBodyIdle);
            }

            Debug.Log("climbing");
        }
    }

    public override void StateEnter()
    {
        base.StateEnter();
        climbing = false;
        character.rb.linearVelocity = Vector2.zero;
        character.grounded = false;
        character.rb.gravityScale = 0;
        character.bc.enabled = false;
    }

    public override void StateExit(BodyState nextState)
    {
        base.StateExit(nextState);
        character.rb.gravityScale = 1;
        character.bc.enabled = true;
    }
}


#endregion