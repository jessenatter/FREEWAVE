using System.Collections.Generic;
using UnityEngine;
using System;

public class Character : PrimaryClass
{
    protected string name;
    public GameObject gameObject,spine1,spine2, head;
    public Limb frontArm, backArm, frontLeg, backLeg;
    List<Limb> limbs = new List<Limb>();
    public int xDir;
    public Rigidbody2D rb;
    BoxCollider2D bc;
    SpriteRenderer sr;
    Vector2 spawnPoint;
    public float moveSpeed = 1f,jumpForce = 5f;
    public UpperBodyState currentUpperBodyState;
    public LowerBodyState currentLowerBodyState;
    public Sprite[] sprites;

    public LowerBodyIdle lowerBodyIdle = new LowerBodyIdle();
    public LowerBodyRun lowerBodyRun = new LowerBodyRun();
    public LowerBodyJump lowerBodyJump = new LowerBodyJump();

    public UpperBodyIdle upperBodyIdle = new UpperBodyIdle();

    public bool grounded;
    protected float groundedCD = 10, groundedTimer;

    override public void Start(Manager _manager)
    {
        base.Start(_manager);

        sprites = Resources.LoadAll<Sprite>("Sprites/Characters/" + name.ToString());

        gameObject = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Character"));

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

        List<BodyState> _bodyStates = new List<BodyState>();
        _bodyStates.AddRange(new BodyState[] { lowerBodyIdle,lowerBodyJump,lowerBodyRun,upperBodyIdle });

        foreach (BodyState bodyState in _bodyStates)
            bodyState.Start(this);
    }

    Limb StartLimb(bool isLeg, bool isBackLimb,GameObject _gameObject)
    {
        Limb limb = new Limb();
        limb.Start(isBackLimb, isLeg,_gameObject);

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

        foreach (Limb limb in limbs)
            limb.Update();

        currentLowerBodyState.StateUpdate();
        currentUpperBodyState.StateUpdate();
    }

    void GroundedCheck()
    {
        RaycastHit2D hit = Physics2D.BoxCast(gameObject.transform.position, bc.size, 0, Vector2.down, 0.15f, manager.groundMask);

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

    protected void Jump()
    {
        if (grounded)
        {
            currentLowerBodyState = lowerBodyJump;
        }
    }
}

public class Player : Character
{
    public override void Start(Manager _manager)
    {
        name = "Player";
        moveSpeed = 3f;

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
    }
}

public class UpperBodyRun : UpperBodyState
{
    public override void Start(Character _character)
    {
        base.Start(_character);

        ThreePoints threePoints = new ThreePoints();
        threePoints.pointA = new Vector2(0, 0);
        threePoints.pointB = new Vector2(0, 0);
        threePoints.pointC = new Vector2(0, 0);
        threePoints.duration = 1f;

        limbMode = threePoints;
    }
}

public class UpperBodyJump : UpperBodyState
{
    public override void Start(Character _character)
    {
        base.Start(_character);

        FollowVector2 followVector2 = new FollowVector2();
        followVector2.vector2 = new Vector2(0.2f, 0.3f);

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
}

public class LowerBodyRun : LowerBodyState
{
    public override void Start(Character _character)
    {
        base.Start(_character);

        ThreePoints threePoints = new ThreePoints();
        threePoints.pointA = new Vector2(0, 0);
        threePoints.pointB = new Vector2(0, 0);
        threePoints.pointC = new Vector2(0, 0);
        threePoints.duration = 1f;

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
}

public class LowerBodyJump : LowerBodyState
{
    public override void Start(Character _character)
    {
        base.Start(_character);

        limbMode = new FollowVector2();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        character.rb.linearVelocity = new Vector2((character.moveSpeed/1.65f) * character.xDir, character.rb.linearVelocity.y);

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

#endregion