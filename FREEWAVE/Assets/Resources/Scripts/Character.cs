using System.Collections.Generic;
using UnityEngine;
using System;

public class Character : PrimaryClass
{
    public string name;
    public GameObject gameObject,spine1,spine2, head;
    public Limb frontArm, backArm, frontLeg, backLeg;
    public float armMaxRadius, legMaxRadius;
    protected List<Limb> limbs = new List<Limb>();
    protected List<Limb> arms = new List<Limb>();
    protected List<Limb> legs = new List<Limb>();
    public int xDir;
    public Rigidbody2D rb;
    public BoxCollider2D bc;
    public SpriteRenderer sr;
    public Vector2 spawnPoint,climbPoint;
    public float moveSpeed = 1f,jumpForce = 5f, rotationLerp = 0.5f,tiltRotation = 10;
    public UpperBodyState currentUpperBodyState;
    public LowerBodyState currentLowerBodyState;

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

        gameObject = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Character/Character"));
        gameObject.name = name;

        rb = gameObject.GetComponent<Rigidbody2D>();
        bc = gameObject.GetComponent<BoxCollider2D>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        gameObject.transform.position = spawnPoint;

        spine1 = gameObject.transform.GetChild(0).gameObject;
        spine2 = spine1.transform.GetChild(0).gameObject;
        head = spine2.transform.GetChild(0).gameObject;

        frontLeg = StartLimb(false, false,gameObject.transform.GetChild(1).gameObject);
        backLeg = StartLimb(false, true, gameObject.transform.GetChild(2).gameObject);
        frontArm = StartLimb(true, false,spine2.transform.GetChild(1).gameObject);
        backArm = StartLimb(true, true,spine2.transform.GetChild(2).gameObject);

        limbs.AddRange(new[] { frontArm, backArm, frontLeg, backLeg });
        legs.Add(frontLeg); legs.Add(backLeg);
        arms.Add(frontArm); arms.Add(backArm);

        currentLowerBodyState = lowerBodyIdle;
        currentUpperBodyState = upperBodyIdle;

        armMaxRadius = frontArm.lengthCmax;
        legMaxRadius = frontLeg.lengthCmax;

        List<BodyState> _bodyStates = new List<BodyState>();
        _bodyStates.AddRange(new BodyState[] { lowerBodyIdle,lowerBodyJump,lowerBodyRun,upperBodyIdle,upperBodyJump,upperBodyRun,lowerBodyClimb,upperBodyClimb });

        foreach (BodyState bodyState in _bodyStates)
            bodyState.Start(this);
    }

    Limb StartLimb(bool isArm, bool isBackLimb,GameObject _gameObject)
    {
        Limb limb = new Limb();
        limb.Start(isBackLimb,isArm,_gameObject,this);

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

        if (top.collider == null && bottom.collider != null)
        {
            if(gameObject.transform.position.x > bottom.collider.transform.position.x)
                climbPoint = new Vector2(bottom.collider.bounds.max.x, bottom.collider.bounds.max.y);
            else
                climbPoint = new Vector2(bottom.collider.bounds.min.x, bottom.collider.bounds.max.y);

            Climb();
        }
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
            lowerBodyClimb.climbing = true;
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

public class Zombie : Character
{
    public Sprite[] headSprites, bodySprites, limbSprites;
        
    List<Sprite> legSprites = new List<Sprite>();
    List<Sprite> armSprites = new List<Sprite>();
    List<Sprite> footSprites = new List<Sprite>();
    List<Sprite> bicepSprites = new List<Sprite>();

    public override void Start(Manager _manager)
    {
        name = "Zombie";
        base.Start(_manager);
        moveSpeed = 3.5f;
        jumpForce = 5.5f;

        headSprites = Resources.LoadAll<Sprite>("Sprites/Characters/ZombieParts/ZombieHeads");
        bodySprites = Resources.LoadAll<Sprite>("Sprites/Characters/ZombieParts/ZombieBodies");
        limbSprites = Resources.LoadAll<Sprite>("Sprites/Characters/ZombieParts/ZombieLimbs");

        sr.sprite = bodySprites[UnityEngine.Random.Range(0, bodySprites.Length)];
        head.GetComponent<SpriteRenderer>().sprite = headSprites[UnityEngine.Random.Range(0, headSprites.Length)];

        foreach (var sprite in limbSprites)
        {
            if (sprite.name.Contains("Leg"))
                legSprites.Add(sprite);
            else if (sprite.name.Contains("Arm"))
                armSprites.Add(sprite);
            else if (sprite.name.Contains("Bicep"))
                bicepSprites.Add(sprite);
            else if (sprite.name.Contains("Foot"))
                footSprites.Add(sprite);
        }

        foreach(Limb limb in arms)
        {
            limb.srA.sprite = bicepSprites[UnityEngine.Random.Range(0, bicepSprites.Count)];
            limb.srB.sprite = armSprites[UnityEngine.Random.Range(0, armSprites.Count)];
            limb.srC.sprite = null;
        }

        foreach(Limb limb in legs)
        {
            limb.srA.sprite = legSprites[UnityEngine.Random.Range(0, legSprites.Count)];
            limb.srB.sprite = footSprites[UnityEngine.Random.Range(0, footSprites.Count)];
        }    
    }

    public override void Update()
    {
        xDir = Mathf.RoundToInt(-Mathf.Sign(gameObject.transform.position.x - manager.player.gameObject.transform.position.x));

        base.Update();
    }
}

#region body state logic

public class BodyState 
{
    protected Character character;
    public LimbMode limbMode;
    public float rotation;

    public virtual void Start(Character _character) { character = _character; }

    public virtual void StateUpdate() { }

    public virtual void StateExit(BodyState nextState) { }

    public virtual void StateEnter() { }
}

public class UpperBodyState : BodyState
{
    public float headRotation, spine1rotation, spine2rotation;

    public override void StateEnter()
    {
        base.StateEnter();
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

    public override void StateUpdate()
    {
        base.StateUpdate();
        if (character.xDir != 0)
            StateExit(character.upperBodyRun);
    }
}

public class UpperBodyRun : UpperBodyState
{
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
    
}

public class UpperBodyAttack : UpperBodyState
{

}

#endregion

#region //lowerbody states

public class LowerBodyIdle : LowerBodyState
{
    float decelerationLerp = 0.2f;

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
    float climbSpeed = .1f;
    public bool climbing = false, hitY;

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
                int _xDir = MathF.Sign(character.gameObject.transform.localScale.x);
                character.gameObject.transform.Translate(new Vector2(_xDir,0)* climbSpeed);

                if (_xDir == 1)
                {
                    if (character.gameObject.transform.position.x > character.climbPoint.x)
                        Exit();
                }
                else if (character.gameObject.transform.position.x < character.climbPoint.x)
                    Exit();
            }
        }
    }

    public override void StateEnter()
    {
        base.StateEnter();
        climbing = false;
        hitY = false;
        character.rb.linearVelocity = Vector2.zero;
        character.grounded = false;
        character.rb.gravityScale = 0;
        character.bc.enabled = false;
    }

    void Exit()
    {
        StateExit(character.lowerBodyIdle);
        character.currentUpperBodyState.StateExit(character.upperBodyIdle);
    }

    public override void StateExit(BodyState nextState)
    {
        base.StateExit(nextState);
        character.rb.gravityScale = 1;
        character.bc.enabled = true;
    }
}

public class LowerBodyAttack : LowerBodyState
{

}

#endregion

#endregion