using System.Collections.Generic;
using UnityEngine;
using System;

public class Character : PrimaryClass
{
    protected string name;
    public GameObject gameObject,head;
    Limb frontArm, backArm, frontLeg, backLeg;
    List<Limb> limbs = new List<Limb>();
    public int xDir;
    public Rigidbody2D rb;
    BoxCollider2D bc;
    SpriteRenderer sr;
    Vector2 spawnPoint,boxsize = new Vector2(0.4f,.9f);
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

        limbs.AddRange(new[] { frontArm, backArm, frontLeg, backLeg });

        sprites = Resources.LoadAll<Sprite>("Sprites/Characters/" + name.ToString());

        gameObject = new GameObject(name);

        rb = gameObject.AddComponent<Rigidbody2D>();
        bc = gameObject.AddComponent<BoxCollider2D>();
        bc.size = boxsize;
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprites[0];
        gameObject.transform.position = spawnPoint;

        frontLeg = StartLimb(true, false);
        backLeg = StartLimb(true, true);
        frontArm = StartLimb(false, false);
        backArm = StartLimb(false, true);

        head = new GameObject(name + "head");
        head.AddComponent<SpriteRenderer>().sprite = sprites[5];
        head.transform.position = new Vector2(gameObject.transform.position.x,bc.bounds.max.y + 0.2f);
        head.transform.SetParent(gameObject.transform);
        currentLowerBodyState = lowerBodyIdle;
        currentUpperBodyState = upperBodyIdle;

        List<BodyState> _bodyStates = new List<BodyState>();
        _bodyStates.AddRange(new BodyState[] { lowerBodyIdle,lowerBodyJump,lowerBodyRun,upperBodyIdle });

        foreach (BodyState bodyState in _bodyStates)
            bodyState.Start(this);
    }

    Limb StartLimb(bool isLeg, bool isBackLimb)
    {
        Limb limb = new Limb();
        limb.Start(isBackLimb, isLeg);

        if (isLeg)
        {
            limb.srA.sprite = sprites[1];
            limb.srB.sprite = sprites[2];

            limb.partA.transform.position = new Vector2(bc.bounds.min.x,gameObject.transform.position.y - limb.boxsize.y);
        }
        else
        {
            limb.srA.sprite = sprites[3];
            limb.srB.sprite = sprites[4];

            limb.partA.transform.position = new Vector2(bc.bounds.min.x, bc.bounds.max.y);
        }

        if (isBackLimb)
            limb.partA.transform.position = new Vector2(bc.bounds.max.x, limb.partA.transform.position.y);

        limb.partA.transform.SetParent(gameObject.transform);

        return limb;
    }

    override public void Update()
    {
        GroundedCheck();

        currentLowerBodyState.StateUpdate();
        currentUpperBodyState.StateUpdate();
    }

    void GroundedCheck()
    {
        Vector2 orgin = new Vector2(gameObject.transform.position.x,gameObject.transform.position.y - (bc.size.y/2));

        RaycastHit2D hit = Physics2D.BoxCast(orgin, boxsize, 0, Vector2.down, 0.1f, manager.groundMask);

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
            Debug.Log("wwww");
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

}

public class LowerBodyState : BodyState
{
    
}

public class UpperBodyIdle : UpperBodyState
{

}

public class LowerBodyIdle : LowerBodyState
{
    public override void StateUpdate()
    {
        base.StateUpdate();

        if (character.xDir != 0)
            StateExit();
    }

    public override void StateExit()
    {
        base.StateExit();
        character.currentLowerBodyState = character.lowerBodyRun;
    }
}

public class LowerBodyRun : LowerBodyState
{
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