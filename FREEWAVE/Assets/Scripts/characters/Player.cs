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
    [SerializeField] GameObject frontArmIK;
    LimbManager frontArmTarget;
    GameObject grappleBullet;
    LineRenderer lineRenderer;
    PublicTimer grappleTimer = new PublicTimer(30f);
    bool canGrapple,interactKeyReleased,attackKeyReleased,switchKeyReleased = true,dead;
    bool interactHeld, interactHoldTriggered;
    float interactHoldTime;
    float shipFlipHoldDuration = 0.35f;
    CameraScript cam;
    float maxGrappleSpeed = 15f;
    AudioSource grappleAudioSource;
    float grappleAudioBasePitch = 1f;
    float grappleAudioPitchRange = 0.35f;
    float grappleAudioPitchCycleSpeed = 2.5f;
    float grappleAudioActiveTime;
    float aimFlipDeadzone = 0.1f;

    [HideInInspector] public bool inCombat;
    
    PublicTimer combatCheckTimer = new PublicTimer(70f);
    PublicTimer inCombatTimer = new PublicTimer(70f);

    List<GameObject> aimedWeapons = new List<GameObject>();

    List<GameObject> meleeWeapons = new List<GameObject>();

    GameObject currentMelee,currentAimed, grappleFunctionPoint;

    Light2D radarLight;
    PublicTimer radarBeepTimer = new PublicTimer(30f);
    string radarBeepSound = "beep";
    float radarBeepVolume = 0.2f,radarBeepPitchVariance = 0.05f,radarSlowBeepFrames = 90f, radarFastBeepFrames = 20f;

    override protected void Start()
    {
        attackTimer.SetDuration(15f);
        attackCD.SetDuration(5f);
        knockbackForce = 5f;
        dashAttackSpeed = 7f;
        jumpForce = jumpForce + 0.1f;
        dashAttackTimer.SetDuration(20f);
        hurtTimer.SetDuration(30f);
        
        base.Start();

        ship = GameObject.FindGameObjectWithTag("Ship").GetComponent<Ship>();
        lineRenderer = GetComponent<LineRenderer>();
        
        lineRenderer.positionCount = 2;
        float width = 0.015f;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        if (grappleAudioSource == null)
            grappleAudioSource = GetComponent<AudioSource>();

        if (grappleAudioSource != null)
        {
            grappleAudioSource.loop = true;
            grappleAudioBasePitch = grappleAudioSource.pitch;
        }

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
        frontArmTarget = frontArmIK.GetComponent<LimbManager>();

        grappleBullet = GameObject.FindGameObjectWithTag("GrappleBullet").gameObject;
        grappleBullet.SetActive(false);
    }
    override protected void Update() //reading input, visuals
    {
        base.Update();

        if(characterIsActive && !dead)
        {
            GetInputs();
        }

        
    }

    override protected void LateUpdate()
    {
        base.LateUpdate();
        
        if(aiming)
        {
            AimUpdate();
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
        {
            UpdateGrapplePullAudio(false);
            base.MovementUpdate();
        }
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
        if(InputManager.Instance.moveAction.ReadValue<Vector2>().x != 0)
            xInput = Mathf.Sign(InputManager.Instance.moveAction.ReadValue<Vector2>().x);
        else 
            xInput = 0;
        
        yInput = InputManager.Instance.moveAction.ReadValue<Vector2>().y;

        if (InputManager.Instance.jumpAction.IsPressed())
            Jump();

        if (InputManager.Instance.interactAction.IsPressed())
        {
            if (!interactHeld && interactKeyReleased)
            {
                interactHeld = true;
                interactHoldTriggered = false;
                interactHoldTime = 0f;
                interactKeyReleased = false;
            }
            else if (interactHeld)
                interactHoldTime += Time.deltaTime;

            if (interactHeld && !interactHoldTriggered && CanEnterShip() && interactHoldTime >= shipFlipHoldDuration)
            {
                FlipShip();
                interactHoldTriggered = true;
            }
        }
        else
        {
            if (interactHeld && !interactHoldTriggered)
                Interact();

            interactHeld = false;
            interactHoldTriggered = false;
            interactHoldTime = 0f;
            interactKeyReleased = true;
        }
        
        bool wantsToAim = Mouse.current.rightButton.isPressed || InputManager.Instance.lookAction.ReadValue<Vector2>().magnitude != 0;

        if(wantsToAim)
        {
            //start aiming
            aiming = true;

            if(frontArmTarget != null)
                frontArmTarget.enabled = false;

            currentAimed.SetActive(true);
            currentMelee.SetActive(false);
        }
        else
        {
            //stop aiming
            aiming = false;

            if(frontArmTarget != null)
                frontArmTarget.enabled = true;

            currentAimed.SetActive(false);
            currentMelee.SetActive(true);
        }
        
        if(InputManager.Instance.attackAction.IsPressed())
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
                if(InputManager.Instance.useDrugAction.IsPressed() || InputManager.Instance.switchDrugAction.IsPressed())
                    GrappleShoot();
            }
            else if(currentAimed == radar)
            {
                UpdateRadarLight();
            }
        }

        int switchDir = 0;
        if(InputManager.Instance.switchRightAction.IsPressed())
            switchDir = 1;
        else if(InputManager.Instance.switchLeftAction.IsPressed())
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

        if(InputManager.Instance.switchDrugAction.ReadValue<float>() != 0)
            SwitchDrug((int)InputManager.Instance.switchDrugAction.ReadValue<float>());
    }
    void Interact()
    {
        canEnterShip = CanEnterShip();

        if(nearInteractable)
        {
            InteractWithObject();
        }
        else if(canEnterShip)
            EnterShip();
    }

    bool CanEnterShip()
    {
        Vector2 distanceFromShip = transform.position - ship.transform.position;
        return distanceFromShip.magnitude < maxDistanceFromShip;
    }
    void AimUpdate()
    {
        if(InputManager.Instance.lookAction.ReadValue<Vector2>().magnitude == 0)
        {
            Vector2 mousePos = InputManager.Instance.pointAction.ReadValue<Vector2>();
            mouseWorld = Manager.Instance.cam.cameraComponent.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Manager.Instance.cam.cameraComponent.WorldToScreenPoint(transform.position).z));
        }
        else
        {
            Vector2 aimDir = InputManager.Instance.lookAction.ReadValue<Vector2>();
            float distance = 6.5f;
            mouseWorld = (Vector2)transform.position + aimDir * distance;
        }

        Vector2 toAim = mouseWorld - (Vector2)transform.position;
        if (Mathf.Abs(toAim.x) > aimFlipDeadzone)
        {
            float faceSign = Mathf.Sign(toAim.x);
            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x) * faceSign, transform.localScale.y);
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
            grappleBullet.SetActive(true);
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
        UpdateGrapplePullAudio(!grappleIsShooting);

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
        //needs work
        Vector2 dir = grapplePoint - (Vector2)grappleFunctionPoint.transform.position;
        float grappleSpeed = 17f;

        Vector2 fakeGravity = Vector2.down * 8f;

        rb.AddForce(dir.normalized * grappleSpeed + fakeGravity);
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity,maxGrappleSpeed);
        
        if(dir.magnitude < 1f)
            GrappleCancel();
    }
    void GrappleCancel()
    {
        isGrappling = false;
        UpdateGrapplePullAudio(false);
        rb.gravityScale = 1;
        lineRenderer.enabled = false;
        grappleBullet.SetActive(false);
    }

    void UpdateGrapplePullAudio(bool isBeingPulled)
    {
        if (grappleAudioSource == null)
            return;

        if (!isBeingPulled)
        {
            grappleAudioActiveTime = 0f;
            grappleAudioSource.pitch = grappleAudioBasePitch;

            if (grappleAudioSource.isPlaying)
                grappleAudioSource.Stop();

            return;
        }

        grappleAudioActiveTime += Time.deltaTime;

        float halfRange = grappleAudioPitchRange * 0.5f;
        float pitchOffset = Mathf.PingPong(grappleAudioActiveTime * grappleAudioPitchCycleSpeed, grappleAudioPitchRange) - halfRange;
        grappleAudioSource.pitch = grappleAudioBasePitch + pitchOffset;

        if (!grappleAudioSource.isPlaying)
            grappleAudioSource.Play();
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
            radarBeepTimer.Reset();
            return;
        }

        Vector2 aimDirection = (mouseWorld - (Vector2)transform.position).normalized;
        Vector2 toDetectable = ((Vector2)closestDetectable.transform.position - (Vector2)transform.position).normalized;

        // Dot is 1 when aiming directly at target, -1 when aiming opposite.
        float dot = Vector2.Dot(aimDirection, toDetectable);
        float alignment = Mathf.Clamp01((dot + 1f) * 0.5f);

        float slowFrames = Mathf.Max(1f, radarSlowBeepFrames);
        float fastFrames = Mathf.Clamp(radarFastBeepFrames, 1f, slowFrames);
        float beepInterval = Mathf.Lerp(slowFrames, fastFrames, alignment);
        radarBeepTimer.SetDuration(beepInterval);

        if (radarBeepTimer.TickLoop())
            SoundManager.PlaySound(radarBeepVolume, radarBeepPitchVariance, radarBeepSound);

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
        interactKeyReleased = false;
        interactHeld = false;
        interactHoldTriggered = false;
        interactHoldTime = 0f;
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
