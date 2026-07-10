using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;
public class Player : Character
{
    GameObject knife,axe,hammer,grapple,radar;
    bool isGrappling,grappleIsShooting;
    [SerializeField] LayerMask grappleLayer;
    Ship ship;
    float maxDistanceFromShip = 1f;
    [HideInInspector] public bool canEnterShip,aiming;
    Vector2 mouseWorld,grapplePoint;
    [SerializeField] GameObject grappleBullet,frontArmIK;
    LineRenderer lineRenderer;
    PublicTimer grappleTimer = new PublicTimer(30f);
    bool canGrapple,interactKeyReleased,attackKeyReleased,switchKeyReleased = true,dead;
    CameraScript cam;
    float maxGrappleSpeed = 15f;

    [HideInInspector] public bool inCombat;
    
    PublicTimer combatCheckTimer = new PublicTimer(70f);
    PublicTimer inCombatTimer = new PublicTimer(70f);

    List<GameObject> aimedWeapons = new List<GameObject>();

    List<GameObject> meleeWeapons = new List<GameObject>();

    GameObject currentMelee,currentAimed, grappleFunctionPoint;

    Light2D radarLight;

    override protected void Start()
    {
        attackTimer.SetDuration(15f);
        attackCD.SetDuration(5f);
        knockbackForce = 5f;
        dashAttackSpeed = 7f;
        dashAttackTimer.SetDuration(20f);
        hurtTimer.SetDuration(30f);
        
        base.Start();

        ship = GameObject.FindGameObjectWithTag("Ship").GetComponent<Ship>();
        lineRenderer = GetComponent<LineRenderer>();
        
        lineRenderer.positionCount = 2;
        float width = 0.015f;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        characterIsActive = true;
        cam = Manager.Instance.cam;

        knife = frontHand.transform.GetChild(0).gameObject;
        grapple = frontHand.transform.GetChild(1).gameObject;
        radar = frontHand.transform.GetChild(2).gameObject;
        axe = frontHand.transform.GetChild(3).gameObject;
        hammer = frontHand.transform.GetChild(4).gameObject;

        currentMelee = knife;
        currentAimed = grapple;

        meleeWeapons.Add(knife);
        meleeWeapons.Add(axe);
        meleeWeapons.Add(hammer);

        aimedWeapons.Add(grapple);
        aimedWeapons.Add(radar);

        grappleFunctionPoint = grapple.transform.GetChild(3).gameObject;
        radarLight = radar.transform.GetChild(2).GetComponent<Light2D>();
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
            if(grappleTimer.TickLoop())
            {
                canGrapple = true;
            }
        }
    }
    void GetInputs()
    {
        if(Manager.Instance.moveAction.ReadValue<Vector2>().x != 0)
            xInput = Mathf.Sign(Manager.Instance.moveAction.ReadValue<Vector2>().x);
        else 
            xInput = 0;
        
        yInput = Manager.Instance.moveAction.ReadValue<Vector2>().y;

        if (Manager.Instance.jumpAction.IsPressed())
            Jump();

        if (Manager.Instance.interactAction.IsPressed())
        {
            if(interactKeyReleased)
                Interact();

            interactKeyReleased = false;
        }
        else
            interactKeyReleased = true;
        
        if(Mouse.current.rightButton.isPressed || Manager.Instance.lookAction.ReadValue<Vector2>().magnitude != 0)
        {
            //start aiming
            aiming = true;

            currentAimed.SetActive(true);
            currentMelee.SetActive(false);
        }
        else
        {
            //stop aiming
            aiming = false;

            currentAimed.SetActive(false);
            currentMelee.SetActive(true);
        }
        
        if(Manager.Instance.attackAction.IsPressed())
        {
            if(attackKeyReleased)
                getAttackInput = true;

            attackKeyReleased = false;
        }
        else
        {
            attackKeyReleased = true;
            getAttackInput = false;
        }

        if(aiming)
        {
            if(currentAimed == grapple)
            {
                if(Manager.Instance.useDrugAction.IsPressed() || Manager.Instance.switchDrugAction.IsPressed())
                    GrappleShoot();
            }
            else if(currentAimed == radar)
            {
                UpdateRadarLight();
            }
        }

        int switchDir = 0;
        if(Manager.Instance.switchRightAction.IsPressed())
            switchDir = 1;
        else if(Manager.Instance.switchLeftAction.IsPressed())
            switchDir = -1;

        if(switchDir != 0)
        {
            if(switchKeyReleased)
            {
                SwitchWeapon(switchDir);
                switchKeyReleased = false;
            }
        }
        else
            switchKeyReleased = true;

        if(Manager.Instance.switchDrugAction.ReadValue<float>() != 0)
            SwitchDrug((int)Manager.Instance.switchDrugAction.ReadValue<float>());
    }
    void Interact()
    {
        Vector2 distanceFromShip = transform.position - ship.transform.position;

        if (distanceFromShip.magnitude < maxDistanceFromShip)
            canEnterShip = true;
        else
            canEnterShip = false;

        if(nearInteractable)
        {
            InteractWithObject();
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
        if(Manager.Instance.lookAction.ReadValue<Vector2>().magnitude == 0)
        {
            Vector2 mousePos = Manager.Instance.pointAction.ReadValue<Vector2>();
            mouseWorld = Manager.Instance.cam.cameraComponent.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Manager.Instance.cam.cameraComponent.WorldToScreenPoint(transform.position).z));
        }
        else
        {
            Vector2 aimDir = Manager.Instance.lookAction.ReadValue<Vector2>();
            float distance = 6.5f;
            mouseWorld = (Vector2)transform.position + aimDir * distance;
        }

        Manager.Instance.mouseObject.transform.position = Vector2.Lerp(transform.position,mouseWorld,.5f);
        frontArmIK.transform.position = mouseWorld;
    }
    void GrappleShoot()
    {
        Vector2 dir = mouseWorld - (Vector2)transform.position;
        float distance = 50f;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance, grappleLayer);

        if(hit == true && canGrapple)
        {
            SoundManager.PlaySound(0.5f,0.2f,"grapple");
            isGrappling = true;
            grapplePoint = hit.point;
            grappleBullet.transform.position = grapplePoint;
            grappleBullet.transform.SetParent(hit.collider.gameObject.transform);
            rb.gravityScale = 0;
            lineRenderer.enabled = true;
            canGrapple = false;
            grappleTimer.Reset();
        }
    }
    void GrappleStateUpdate()
    {
        if(grappleIsShooting)
            GrappleShootingUpdate();
        else
            GrappleUpdate();

        lineRenderer.SetPosition(0,grappleFunctionPoint.transform.position);
        lineRenderer.SetPosition(1,grappleBullet.transform.position);
    }
    void GrappleShootingUpdate()
    {
        
    } 
    void GrappleUpdate()
    {
        Vector2 dir = grapplePoint - (Vector2)grappleFunctionPoint.transform.position;
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

    void UpdateRadarLight()
    {
        List<GameObject> detectableObjects = new List<GameObject>();

        foreach (Detectable detectable in Manager.Instance.detectables)
        {
            if (detectable != null)
                detectableObjects.Add(detectable.gameObject);
        }

        GameObject closestDetectable = PublicUtilities.closestObject(detectableObjects, transform);

        if (closestDetectable == null)
        {
            radarLight.color = Color.red;
            return;
        }

        Vector2 aimDirection = (mouseWorld - (Vector2)transform.position).normalized;
        Vector2 toDetectable = ((Vector2)closestDetectable.transform.position - (Vector2)transform.position).normalized;

        // Dot is 1 when aiming directly at target, -1 when aiming opposite.
        float dot = Vector2.Dot(aimDirection, toDetectable);
        float alignment = Mathf.Clamp01((dot + 1f) * 0.5f);

        radarLight.color = Color.Lerp(Color.red, Color.green, alignment);
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

    void SwitchWeapon(int dir)
    {
        if(aiming)
            currentAimed = SwitchWeaponFromList(aimedWeapons, currentAimed, dir);
        else
            currentMelee = SwitchWeaponFromList(meleeWeapons, currentMelee, dir);
    }

    GameObject SwitchWeaponFromList(List<GameObject> weaponList, GameObject currentWeapon, int dir)
    {
        int newIndex = weaponList.IndexOf(currentWeapon) + dir;
        newIndex = (newIndex + weaponList.Count) % weaponList.Count;

        currentWeapon.SetActive(false);
        GameObject newWeapon = weaponList[newIndex];
        newWeapon.SetActive(true);
        return newWeapon;
    }

    void SwitchDrug(int dir)
    {
        
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

        foreach(Enemy enemy in Manager.Instance.enemies)
        {
            Vector2 _dist = enemy.transform.position - transform.position;
            if(_dist.magnitude < minDistance)
            {
                hasEnemy = true;
                inCombatTimer.Reset();
            }    
        }

        if(hasEnemy)
        {
            if(combatCheckTimer.TickLoop())
                inCombat = true;
        }
        else
        {
            combatCheckTimer.Reset();
            inCombatTimer.Tick();
            if(inCombatTimer.IsComplete)
            {
                inCombatTimer.Reset();
                inCombat = false;
            }
        }
    }

    protected override void Die()
    {
        base.Die();
        Manager.Instance.PlayerDie();
        dead = true;
    }

    protected override void Hurt(Vector2 hurtDir, float damage)
    {
        base.Hurt(hurtDir, damage);
        cam.StartScreenShake(10,0.01f);
    }
}
