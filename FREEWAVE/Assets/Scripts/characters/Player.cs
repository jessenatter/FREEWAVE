using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
public class Player : Character
{
    [SerializeField] GameObject knife,grapple;
    bool isGrappling,grappleIsShooting;
    [SerializeField] LayerMask grappleLayer;
    Ship ship;
    float maxDistanceFromShip = 1f;
    public bool canEnterShip,aiming;
    Vector2 mouseWorld,grapplePoint;
    [SerializeField] GameObject grappleBullet,frontArmIK;
    LineRenderer lineRenderer;
    float grappleTimer = 30,grappleTimerCurrent;
    bool canGrapple,interactKeyReleased,attackKeyReleased,dead;
    CameraScript cam;
    float maxGrappleSpeed = 15f;

    [HideInInspector] public bool inCombat;
    
    float combatCheckTimer = 70,combatCheckCurrent;
    float inCombatTimer = 70,inCombatTimerCurrent;

    override protected void Start()
    {
        attackTimer = 15;
        attackCD = 5;
        knockbackForce = 5f;
        dashAttackSpeed = 7f;
        dashAttackTimer = 20f;
        hurtTimer = 30f;
        
        base.Start();

        ship = GameObject.FindGameObjectWithTag("Ship").GetComponent<Ship>();
        lineRenderer = GetComponent<LineRenderer>();
        
        lineRenderer.positionCount = 2;
        float width = 0.015f;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        characterIsActive = true;
        cam = manager.cam;
    }
    override protected void Update() //reading input, visuals
    {
        base.Update();

        if(characterIsActive && !dead)
        {
            GetInputs();
        }

        if(aiming)
        {
            UpdateMouseObject();
        }
    }
    override protected void FixedUpdate() //rb stuff
    {
        base.FixedUpdate();
        CheckCombat();
    }

    
    protected override void MovementUpdate()
    {
        if(!isGrappling)
            base.MovementUpdate();
        else 
            GrappleStateUpdate();

        if(!canGrapple)
        {
            grappleTimerCurrent++;
            if(grappleTimerCurrent == grappleTimer)
            {
                canGrapple = true;
                grappleTimerCurrent = 0;
            }
        }
    }
    void GetInputs()
    {
        if(manager.moveAction.ReadValue<Vector2>().x != 0)
            xInput = Mathf.Sign(manager.moveAction.ReadValue<Vector2>().x);
        else 
            xInput = 0;
        
        yInput = manager.moveAction.ReadValue<Vector2>().y;

        if (manager.jumpAction.IsPressed())
            Jump();

        if (manager.interactAction.IsPressed())
        {
            if(interactKeyReleased)
                Interact();

            interactKeyReleased = false;
        }
        else
            interactKeyReleased = true;
        
        if(Mouse.current.rightButton.isPressed || manager.lookAction.ReadValue<Vector2>().magnitude != 0)
            aiming = true;
        else
            aiming = false;
        
        if(manager.attackAction.IsPressed())
        {
            if(attackKeyReleased)
            {
                if(aiming)
                    Shoot();
                else
                    getAttackInput = true;
            }

            attackKeyReleased = false;
        }
        else
        {
            attackKeyReleased = true;
            getAttackInput = false;
        }
    }
    void Interact()
    {
        Vector2 distanceFromShip = transform.position - ship.transform.position;

        if (distanceFromShip.magnitude < maxDistanceFromShip)
            canEnterShip = true;
        else
            canEnterShip = false;

        if(nearPickupable)
        {
            PickupObject();
        }
        else if(canEnterShip)
        {
            if (ship.rb.rotation >= 0 && ship.rb.rotation <= 45 || ship.rb.rotation <= 360 && ship.rb.rotation >= 315)
                EnterShip();
            else
                FlipShip();
        }
    }
    void UpdateMouseObject()
    {
        if(manager.lookAction.ReadValue<Vector2>().magnitude == 0)
        {
            Vector2 mousePos = manager.pointAction.ReadValue<Vector2>();
            mouseWorld = manager.cam.cameraComponent.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, manager.cam.cameraComponent.WorldToScreenPoint(transform.position).z));
        }
        else
        {
            Vector2 aimDir = manager.lookAction.ReadValue<Vector2>();
            float distance = 6.5f;
            mouseWorld = (Vector2)transform.position + aimDir * distance;
        }

        manager.mouseObject.transform.position = Vector2.Lerp(transform.position,mouseWorld,.5f);
        frontArmIK.transform.position = mouseWorld;
    }
    void Shoot()
    {
        Vector2 dir = mouseWorld - (Vector2)transform.position;
        float distance = 50f;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance, grappleLayer);

        if(hit == true && canGrapple)
        {
            isGrappling = true;
            grapplePoint = hit.point;
            grappleBullet.transform.position = grapplePoint;
            rb.gravityScale = 0;
            lineRenderer.enabled = true;
            canGrapple = false;
        }
    }
    void GrappleStateUpdate()
    {
        if(grappleIsShooting)
            GrappleShootingUpdate();
        else
            GrapplingUpdate();

        lineRenderer.SetPosition(0,transform.position);
        lineRenderer.SetPosition(1,grappleBullet.transform.position);
    }
    void GrappleShootingUpdate()
    {
        
    } 
    void GrapplingUpdate()
    {
        Vector2 dir = grapplePoint - (Vector2)transform.position;
        float grappleSpeed = 15f;

        Vector2 fakeGravity = Vector2.down * 6.5f;

        rb.AddForce(dir.normalized * grappleSpeed + fakeGravity);
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity,maxGrappleSpeed);
        
        if(dir.magnitude < 1.5f)
            GrappleCancel();
    }
    void GrappleCancel()
    {
        isGrappling = false;
        rb.gravityScale = 1;
        lineRenderer.enabled = false;
    }

    protected override void Jump()
    {
        base.Jump();
        GrappleCancel();
    }

    protected override void Attack()
    {
        base.Attack();
        GrappleCancel();
    }

    protected override void DashAttack()
    {
        base.DashAttack();
        GrappleCancel();
    }

    protected override void DownAttack()
    {
        GrappleCancel();//first bc gravity opperations
        base.DownAttack();
    }

    override protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (currentCharacterState == characterState.hurting) return;
        
        //set up a real way to do this
        //Enemy enemy = collision.gameObject.transform.parent.GetComponent<Enemy>();
        //damageToRecive = enemy.damage;
        damageToRecive = 1f;
        base.OnTriggerEnter2D(collision);
    }

    public void EnterShip()
    {
        characterIsActive = false;
        gameObject.SetActive(false);
        cam.EnterShip();
        ship.EnterShip();
    }
    public void ExitShip()
    {
        transform.position = ship.transform.position;
        characterIsActive = true;
        float exitMultiplier = 2f;
        rb.AddForce(ship.rb.linearVelocity * exitMultiplier,ForceMode2D.Impulse);
        cam.ExitShip();

    }
    public void FlipShip()
    {
        ship.transform.position += new Vector3(0, 1f,0f);
        ship.rb.rotation = 0;
    }

    void CheckCombat()
    {
        float minDistance = 4f;
        bool hasEnemy = false;

        foreach(Zombie zombie in manager.zombies)
        {
            Vector2 _dist = zombie.transform.position - transform.position;
            if(_dist.magnitude < minDistance)
            {
                hasEnemy = true;
                inCombatTimerCurrent = 0;
            }    
        }

        if(hasEnemy)
            combatCheckCurrent++;
        else
        {
            combatCheckCurrent = 0;
            inCombatTimerCurrent++;
            if(inCombatTimerCurrent == inCombatTimer)
            {
                inCombatTimerCurrent = 0;
                inCombat = false;
            }
        }

        if(combatCheckCurrent == combatCheckTimer)
        {
            combatCheckCurrent = 0;
            inCombat = true;
        }
    }

    protected override void Die()
    {
        base.Die();
        manager.PlayerDie();
        dead = true;
    }

    protected override void Hurt(Vector2 hurtDir, float damage)
    {
        base.Hurt(hurtDir, damage);
        cam.StartScreenShake(10,0.01f);
    }
}
