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
    BoxCollider2D bc;
    SpriteRenderer sr;
    public Vector2 spawnPoint,climbPoint;
    public float moveSpeed = 1f,jumpForce = 5f, rotationLerp = 0.1f;
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

        armMaxRadius = frontArm.partA.transform.position.y - frontArm.partC.transform.position.y;
        legMaxRadius = frontLeg.partA.transform.position.y - frontLeg.partC.transform.position.y;

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

        Debug.Log(currentUpperBodyState);
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
        Vector2 climbOrgin = new Vector2(head.transform.position.x + (Mathf.Sign(gameObject.transform.localScale.x) * 0.5f), 0);

        Vector2 boxSize = new Vector2(0.4f, 0.2f);
        float distance = 0.1f;
        RaycastHit2D top = Physics2D.BoxCast(climbOrgin, boxSize, 0, Vector2.up, distance,manager.groundMask);
        RaycastHit2D bottom = Physics2D.BoxCast(climbOrgin, boxSize, 0, Vector2.down, distance, manager.groundMask);

        if(top.collider == null && bottom.collider != null)
        {
            if(gameObject.transform.position.x > bottom.collider.transform.position.x)
                climbPoint = new Vector2(bottom.collider.bounds.max.x,bottom.collider.bounds.max.y);
            else
                climbPoint = new Vector2(bottom.collider.bounds.min.x, bottom.collider.bounds.max.y);

            Climb();
        }
    }

    protected void Jump()
    {
        if (grounded && canJump)
        {
            Debug.Log("a");
            canJump = false;
            currentLowerBodyState.StateExit();
            currentUpperBodyState.StateExit();
            currentLowerBodyState = lowerBodyJump;
            currentUpperBodyState = upperBodyJump;
        }
    }

    protected void Climb()
    {
        currentLowerBodyState.StateExit();
        currentUpperBodyState.StateExit();
        currentLowerBodyState = lowerBodyClimb;
        currentUpperBodyState = upperBodyClimb;
    }

    protected void Rotations()
    {
        //gameObject.transform.rotation = Mathf.Lerp(gameObject.transform.rotation, currentUpperBodyState, rotationLerp);
    }
}

public class Player : Character
{
    public override void Start(Manager _manager)
    {
        name = "Player";
        moveSpeed = 3f;
        jumpForce = 5.8f;

        base.Start(_manager);
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
    bool stateEntered;
    protected LimbMode limbMode;

    public virtual void Start(Character _character)
    {
        character = _character;
    }

    public virtual void StateUpdate()
    {
        if(!stateEntered)
        {
            StateEnter();
            stateEntered = true;
        }
    }

    public virtual void StateExit()
    {
        stateEntered = false;
    }

    public virtual void StateEnter()
    {

    }
}

public class UpperBodyState : BodyState
{
    public override void StateEnter()
    {
        base.StateEnter();

        character.frontArm.currentLimbMode = limbMode;
        character.backArm.currentLimbMode = limbMode;
    }
}

public class LowerBodyState : BodyState
{
    public override void StateEnter()
    {
        base.StateEnter();

        character.frontLeg.currentLimbMode = limbMode;
        character.backLeg.currentLimbMode = limbMode;
    }
}

#region //upperbody states

public class UpperBodyIdle : UpperBodyState
{
    public override void Start(Character _character)
    {
        base.Start(_character);
        limbMode = null;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (character.xDir != 0)
            StateExit();
    }

    public override void StateExit()
    {
        base.StateExit();

        character.currentUpperBodyState = character.upperBodyRun;
    }
}

public class UpperBodyRun : UpperBodyState
{
    float reach = 0.05f;

    public override void Start(Character _character)
    {
        base.Start(_character);

        ThreePoints threePoints = new ThreePoints();
        threePoints.pointA = new Vector2(-character.armMaxRadius * reach, -character.armMaxRadius * reach);
        threePoints.pointB = new Vector2(0, -character.armMaxRadius * reach);
        threePoints.pointC = new Vector2(character.armMaxRadius * reach, -character.armMaxRadius * reach);
        threePoints.duration = 50;
        threePoints.initDuration = threePoints.duration;
        threePoints.loop = true;

        limbMode = threePoints;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (character.gameObject.transform.localScale.x != character.xDir && character.xDir != 0)
            character.gameObject.transform.localScale = new Vector2(character.xDir, 1);

        if (character.xDir == 0)
            StateExit();
    }

    public override void StateExit()
    {
        base.StateExit();
        character.currentUpperBodyState = character.upperBodyIdle;
    }
}

public class UpperBodyJump : UpperBodyState
{
    public override void Start(Character _character)
    {
        base.Start(_character);

        FollowVector2 followVector2 = new FollowVector2();
        followVector2.vector2 = new Vector2(character.armMaxRadius, character.armMaxRadius);

        limbMode = followVector2;
    }

    public override void StateUpdate()
    {
        if (character.grounded == true && character.rb.linearVelocityY <= 0)
            StateExit();

        if (character.gameObject.transform.localScale.x != character.xDir && character.xDir != 0)
            character.gameObject.transform.localScale = new Vector2(character.xDir, 1);

        base.StateUpdate();
    }

    public override void StateExit()
    {
        base.StateExit();
        character.currentUpperBodyState = character.upperBodyIdle;
    }
}

public class UpperBodyClimb : UpperBodyState
{
    public override void Start(Character _character)
    {
        base.Start(_character);
    }

    public override void StateEnter()
    {
        base.StateEnter();

        FollowVector2 followVector2 = new FollowVector2();
        followVector2.vector2 = new Vector2(character.armMaxRadius, character.armMaxRadius);

        limbMode = followVector2;
    }
}

#endregion

#region //lowerbody states

public class LowerBodyIdle : LowerBodyState
{
    float decelerationLerp = 0.1f;

    public override void Start(Character _character)
    {
        base.Start(_character);
        limbMode = null;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (character.xDir != 0)
            StateExit();

        character.rb.linearVelocityX = Mathf.Lerp(character.rb.linearVelocityX, 0, decelerationLerp);
    }

    public override void StateExit()
    {
        base.StateExit();
        character.currentLowerBodyState = character.lowerBodyRun;
    }

    public override void StateEnter()
    {
        base.StateEnter();
        character.canJump = true;
    }
}

public class LowerBodyRun : LowerBodyState
{
    public override void Start(Character _character)
    {
        base.Start(_character);

        ThreePoints threePoints = new ThreePoints();
        threePoints.pointA = new Vector2(character.legMaxRadius, -character.legMaxRadius);
        threePoints.pointB = new Vector2(0, -character.legMaxRadius);
        threePoints.pointC = new Vector2(character.legMaxRadius, -character.legMaxRadius);
        threePoints.duration = 50;
        threePoints.initDuration = threePoints.duration;
        threePoints.loop = true;

        limbMode = threePoints;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (character.xDir == 0)
            StateExit();
        else
        {
            character.rb.linearVelocity = new Vector2(character.moveSpeed * character.xDir, character.rb.linearVelocity.y);
        }
    }

    public override void StateExit()
    {
        base.StateExit();
        character.currentLowerBodyState = character.lowerBodyIdle;
    }

    public override void StateEnter()
    {
        base.StateEnter();
    }
}

public class LowerBodyJump : LowerBodyState
{
    public override void Start(Character _character)
    {
        base.Start(_character);

        FollowVector2 followVector2 = new FollowVector2();
        followVector2.vector2 = new Vector2(0, -character.legMaxRadius);
        limbMode = followVector2;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        character.rb.linearVelocity = new Vector2((character.moveSpeed * .75f) * character.xDir, character.rb.linearVelocity.y);

        if (character.grounded == true && character.rb.linearVelocityY <= 0)
            StateExit();
    }

    public override void StateExit()
    {
        base.StateExit();
        character.currentLowerBodyState = character.lowerBodyIdle;
    }

    public override void StateEnter()
    {
        base.StateEnter();
        character.rb.AddForce(character.jumpForce * Vector2.up,ForceMode2D.Impulse);
    }
}

public class LowerBodyClimb : LowerBodyState
{
    public override void StateUpdate()
    {
        base.StateUpdate();
    }
}

#endregion