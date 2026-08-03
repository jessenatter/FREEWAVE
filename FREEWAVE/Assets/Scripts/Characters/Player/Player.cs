using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
public class Player : Character
{
    [HideInInspector] public Weapon knife,axe,hammer,grapple,radar;
    Ship ship;
    float maxDistanceFromShip = 1f;
    [HideInInspector] public bool canEnterShip,aiming;
    Vector2 mouseWorld;
    bool nearInteractable;
    Interactable lastClosestInteractable;
    [SerializeField] GameObject frontArmIK;
    LimbManager frontArmTarget;
    bool interactKeyReleased,attackKeyReleased,switchKeyReleased = true,dead;
    bool interactHeld, interactHoldTriggered;
    float interactHoldTime;
    float shipFlipHoldDuration = 0.35f;
    CameraScript cam;
    [HideInInspector] public bool inCombat;
    PublicTimer combatCheckTimer = new PublicTimer(70f);
    PublicTimer inCombatTimer = new PublicTimer(70f);
    List<Weapon> aimedWeapons = new List<Weapon>();
    List<Weapon> meleeWeapons = new List<Weapon>();
    [HideInInspector] public Weapon currentMelee,currentAimed;
    PlayerGrapple grappleController;
    Light2D radarLight;
    PublicTimer radarBeepTimer = new PublicTimer(30f);
    override protected void Start()
    {
        attackTimer.SetDuration(15f);
        attackCD.SetDuration(5f);
        dashAttackSpeed = 7f;
        moveSpeed = moveSpeed * 1.5f;
        dashAttackTimer.SetDuration(20f);
        hurtTimer.SetDuration(5f);
        postHitInvincibilitySeconds = 0.5f;
        
        base.Start();
        
        ship = GameObject.FindGameObjectWithTag("Ship").GetComponent<Ship>();

        AudioSource[] playerAudioSources = GetComponents<AudioSource>();

        if (playerAudioSources.Length > 1)
            playerAudioSources[1].loop = true;

        characterIsActive = true;
        cam = Manager.Instance.cam;

        #region //set up tools

        knife = new Weapon(frontHand.transform.GetChild(0).gameObject, false, 2f, 15f,10f);
        grapple = new Weapon(frontHand.transform.GetChild(1).gameObject, false, 1f, 15f,5f);
        radar = new Weapon(frontHand.transform.GetChild(2).gameObject, false, 1f, 15f,5f);
        axe = new Weapon(frontHand.transform.GetChild(3).gameObject, false, 4f, 15f,5f);
        hammer = new Weapon(frontHand.transform.GetChild(4).gameObject, false, 3f, 15f,5f);

        meleeWeapons.Add(knife);
        meleeWeapons.Add(axe);
        meleeWeapons.Add(hammer);

        aimedWeapons.Add(grapple);
        aimedWeapons.Add(radar);

        currentMelee = GetFirstUnlockedWeapon(meleeWeapons);
        currentAimed = GetFirstUnlockedWeapon(aimedWeapons);

        SetHeldWeaponVisuals(false);

        radarLight = radar.gameObject.transform.GetChild(2).GetComponent<Light2D>();
        frontArmTarget = frontArmIK.GetComponent<LimbManager>();

        airMovement = true;

        grappleController = GetComponent<PlayerGrapple>();
        if(grappleController == null)
            grappleController = gameObject.AddComponent<PlayerGrapple>();

        grappleController.Initialize(this);

        #endregion
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
        CheckForInteractables();
        bool isWalking = characterIsActive && !dead && currentCharacterState == characterState.movement && groundedHit && Mathf.Abs(xInput) > 0f && (grappleController == null || !grappleController.IsGrappling);
        UpdateFootstepAudio(isWalking);
        CheckCombat();
    }

    
    protected override void MovementUpdate()
    {
        grappleController.TickCooldown();

        if(grappleController == null || !grappleController.IsGrappling)
        {
            grappleController.StopPullAudio();
            base.MovementUpdate();
        }
        else 
            grappleController.MovementUpdate();
    }
    void GetInputs()
    {
        if(InputManager.Instance.moveAction.ReadValue<Vector2>().x != 0)
            xInput = Mathf.Sign(InputManager.Instance.moveAction.ReadValue<Vector2>().x);
        else 
            xInput = 0;
        
        yInput = InputManager.Instance.moveAction.ReadValue<Vector2>().y;

        if (InputManager.Instance.jumpAction.IsPressed())
        {
            if(currentCharacterState != characterState.frozen)
                Jump();
        }

        if (InputManager.Instance.interactAction.IsPressed())
        {
            if (!interactHeld && interactKeyReleased)
            {
                interactHeld = true;
                interactHoldTriggered = false;
                interactHoldTime = 0f;
                interactKeyReleased = false;

                if(currentCharacterState == characterState.frozen)
                    DialogueManager.Instance.UpdateDialouge();
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
        
        EnsureCurrentWeaponsUnlocked();

        bool hasRangedWeaponUnlocked = HasUnlockedWeapon(aimedWeapons);
        bool hasMeleeWeaponUnlocked = HasUnlockedWeapon(meleeWeapons);
        bool wantsToAim = (Mouse.current.rightButton.isPressed || InputManager.Instance.lookAction.ReadValue<Vector2>().magnitude != 0) && hasRangedWeaponUnlocked;

        if(wantsToAim)
        {
            //start aiming
            aiming = true;

            if(frontArmTarget != null)
                frontArmTarget.enabled = false;
        }
        else
        {
            //stop aiming
            aiming = false;

            if(frontArmTarget != null)
                frontArmTarget.enabled = true;
        }

        SetHeldWeaponVisuals(aiming);
        
        if(hasMeleeWeaponUnlocked && InputManager.Instance.attackAction.IsPressed())
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
                    grappleController.TryShoot();
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
        if(heldPickupable != null)
        {
            if(heldPickupable.GetComponent<Bomb>() != null)
            {
                heldPickupable.GetComponent<Bomb>().Throw((int)Mathf.Sign(transform.localScale.x));
                RemoveHeldPickupable();
            }
        }
        canEnterShip = CanEnterShip();

        if(nearInteractable)
        {
            InteractWithObject();
        }
        else if(canEnterShip)
            EnterShip();
    }

    void CheckForInteractables()
    {
        float minInteractDistance = 1.5f;
        float lastPickupDistance = Mathf.Infinity;
        int highestPriority = int.MinValue;
        Interactable closestInteractable = null;

        nearInteractable = false;

        foreach(Interactable interactable in Manager.Instance.interactables)
        {
            if(interactable.canInteract == false) continue;

            if(interactable is PickupAble pickupAble)
            {
                if(heldPickupable != null) continue;
                if(pickupAble.held) continue;
            }

            Vector2 distance = transform.position - interactable.transform.position;
            float distanceMagnitude = distance.magnitude;

            if(distanceMagnitude < minInteractDistance)
            {
                nearInteractable = true;
                if(closestInteractable == null
                    || interactable.InteractPriority > highestPriority
                    || (interactable.InteractPriority == highestPriority && distanceMagnitude < lastPickupDistance))
                {
                    closestInteractable = interactable;
                    highestPriority = interactable.InteractPriority;
                    lastPickupDistance = distanceMagnitude;

                    if(lastClosestInteractable != null && lastClosestInteractable != closestInteractable)
                        lastClosestInteractable.interactPrompt.SetActive(false);

                    lastClosestInteractable = closestInteractable;
                }
            }

            if(lastClosestInteractable != null)
                lastClosestInteractable.interactPrompt.SetActive(true);
        }

        if(closestInteractable == null && lastClosestInteractable != null)
        {
            lastClosestInteractable.interactPrompt.SetActive(false);
            lastClosestInteractable = null;
            nearInteractable = false;
        }
    }

    void InteractWithObject()
    {
        if(lastClosestInteractable == null) return;

        if(lastClosestInteractable is PickupAble pickupAble)
        {
            if(heldPickupable != null)
                return;

            OnPickup(pickupAble);
        }
        else
            OnInteract();
    }

    void OnPickup(PickupAble pickupAble)
    {
        pickupAble.Pickup();
        pickupAble.interactPrompt.SetActive(false);
        pickupAble.transform.position = backHand.transform.position;
        pickupAble.transform.rotation = backHand.transform.rotation;
        pickupAble.transform.SetParent(backHand.transform);
        pickupAble.held = true;
        heldPickupable = pickupAble;
        lastClosestInteractable = null;

        SoundManager.PlaySound("pickup",0.6f,0f);
        HapticsManager.PlayMedium(0.1f);
    }

    void OnInteract()
    {
        lastClosestInteractable.Interact();
        SoundManager.PlaySound("interact",0.6f,0f);
    }

    public void RemoveHeldPickupable()
    {
        if(heldPickupable != null)
        {
            heldPickupable.transform.SetParent(null);
            heldPickupable = null;
        }
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
        if (Mathf.Abs(toAim.x) > 0.1f)
        {
            float faceSign = Mathf.Sign(toAim.x);
            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x) * faceSign, transform.localScale.y);
        }

        Manager.Instance.mouseObject.transform.position = Vector2.Lerp(transform.position,mouseWorld,.5f);
        frontArmIK.transform.position = mouseWorld;
    }

    public Vector2 AimWorld => mouseWorld;

    public LayerMask GroundLayer => groundLayer;

    public float InitialGravityScale => initGravityScale;

    void UpdateFootstepAudio(bool isWalking)
    {
        const float footstepAudioBasePitch = 1f;
        const float footstepAudioPitchRange = 0.35f;
        const float footstepAudioPitchCycleSpeed = 2.5f;

        AudioSource[] playerAudioSources = GetComponents<AudioSource>();
        if (playerAudioSources.Length < 2)
            return;

        AudioSource footstepAudioSource = playerAudioSources[1];

        if (!isWalking)
        {
            footstepAudioSource.pitch = footstepAudioBasePitch;

            if (footstepAudioSource.isPlaying)
                footstepAudioSource.Stop();

            return;
        }

        float halfRange = footstepAudioPitchRange * 0.5f;
        float pitchOffset = Mathf.PingPong(Time.time * footstepAudioPitchCycleSpeed, footstepAudioPitchRange) - halfRange;
        footstepAudioSource.pitch = footstepAudioBasePitch + pitchOffset;

        if (!footstepAudioSource.isPlaying)
            footstepAudioSource.Play();
    }

    void UpdateRadarLight()
    {
        const string radarBeepSound = "beep";
        const float radarBeepVolume = 0.2f;
        const float radarBeepPitchVariance = 0.05f;
        const float radarSlowBeepFrames = 90f;
        const float radarFastBeepFrames = 20f;

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
        grappleController.StopGrapple();
    }

    protected override void DoJump()
    {
        base.DoJump();
        SoundManager.PlaySound("jump",.3f,0.2f);
    }

    protected override void Attack()
    {
        base.Attack();
        grappleController.StopGrapple();
        SoundManager.PlaySound("knife",0.7f,0.2f);
    }

    protected override void DashAttack()
    {
        base.DashAttack();
        grappleController.StopGrapple();
        SoundManager.PlaySound("knifeSlash",0.7f,0.2f);
    }

    protected override void DownAttack()
    {
        grappleController.StopGrapple();//first bc gravity opperations
        base.DownAttack();
        SoundManager.PlaySound("downAttack",0.7f,0.2f);
    }

    void SwitchWeapon(int dir)
    {
        if(aiming)
            currentAimed = SwitchWeaponFromList(aimedWeapons, currentAimed, dir);
        else
            currentMelee = SwitchWeaponFromList(meleeWeapons, currentMelee, dir);

        SetHeldWeaponVisuals(aiming);
    }

    public bool UnlockWeaponByName(string weaponName)
    {
        if(string.IsNullOrWhiteSpace(weaponName))
            return false;

        Weapon weaponToUnlock = null;

        if(weaponName == "knife")
            weaponToUnlock = knife;
        else if(weaponName == "axe")
            weaponToUnlock = axe;
        else if(weaponName == "hammer")
            weaponToUnlock = hammer;
        else if(weaponName == "grapple")
            weaponToUnlock = grapple;
        else if(weaponName == "radar")
            weaponToUnlock = radar;

        weaponToUnlock.unlocked = true;

        if(currentMelee == null)
            currentMelee = GetFirstUnlockedWeapon(meleeWeapons);

        if(currentAimed == null)
            currentAimed = GetFirstUnlockedWeapon(aimedWeapons);

        SetHeldWeaponVisuals(aiming);
        return true;
    }

    Weapon SwitchWeaponFromList(List<Weapon> weaponList, Weapon currentWeapon, int dir)
    {
        if(weaponList == null || weaponList.Count == 0)
            return null;

        int unlockedCount = 0;
        foreach(Weapon weapon in weaponList)
        {
            if(weapon.unlocked)
                unlockedCount += 1;
        }

        if(unlockedCount == 0)
            return null;

        int startIndex = weaponList.IndexOf(currentWeapon);
        if(startIndex < 0)
            startIndex = dir > 0 ? -1 : 0;

        int index = startIndex;
        for(int i = 0; i < weaponList.Count; i++)
        {
            index = (index + dir + weaponList.Count) % weaponList.Count;
            if(weaponList[index].unlocked)
                return weaponList[index];
        }

        return currentWeapon;
    }

    Weapon GetFirstUnlockedWeapon(List<Weapon> weaponList)
    {
        foreach(Weapon weapon in weaponList)
        {
            if(weapon.unlocked)
                return weapon;
        }

        return null;
    }

    bool HasUnlockedWeapon(List<Weapon> weaponList)
    {
        foreach(Weapon weapon in weaponList)
        {
            if(weapon.unlocked)
                return true;
        }

        return false;
    }

    void EnsureCurrentWeaponsUnlocked()
    {
        if(currentMelee != null && !currentMelee.unlocked)
            currentMelee = GetFirstUnlockedWeapon(meleeWeapons);

        if(currentAimed != null && !currentAimed.unlocked)
            currentAimed = GetFirstUnlockedWeapon(aimedWeapons);
    }

    void SetHeldWeaponVisuals(bool showAimedWeapon)
    {
        foreach(Weapon weapon in meleeWeapons)
            weapon.gameObject.SetActive(false);

        foreach(Weapon weapon in aimedWeapons)
            weapon.gameObject.SetActive(false);

        if(showAimedWeapon)
        {
            if(currentAimed != null && currentAimed.unlocked)
                currentAimed.gameObject.SetActive(true);
        }
        else
        {
            if(currentMelee != null && currentMelee.unlocked)
                currentMelee.gameObject.SetActive(true);
        }
    }

    void SwitchDrug(int dir)
    {
        
    }

    override protected void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "CheckpointTrigger")
        {
            
        }
        else if(collision.tag == "TransitionTrigger")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else if(collision.tag == "DialougeTrigger")
        {
            currentCharacterState = characterState.frozen;
            DialogueManager.Instance.StartNextConversation();
            Destroy(collision.gameObject);
        }
        else if(collision.gameObject.layer == 11)
        {
            if (currentCharacterState != characterState.movement) return;

            Enemy enemy = collision.gameObject.GetComponentInParent<Enemy>();
            if(enemy.currentCharacterState == characterState.movement ||
            enemy.currentCharacterState == characterState.hurting ||
            enemy.currentCharacterState == characterState.idle)
            {
                damageToRecive = 1f;
                knockbackForceToRecive = 18f;
            }
            else
            {
                damageToRecive = enemy.currentAttack.damage;
                knockbackForceToRecive = enemy.currentAttack.knockbackForce;
            }
        }
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

    protected override void Die(int dir)
    {
        base.Die(dir);
        Manager.Instance.PlayerDie();
        dead = true;
    }

    protected override void Hurt(int hurtDir, float damage, float knockback)
    {
        base.Hurt(hurtDir, damage,knockback);
        cam.StartScreenShake(10,0.02f);
        HapticsManager.PlayHeavy(0.3f);
    }

    protected override void Land()
    {
        base.Land();
        HapticsManager.PlayLight(0.1f);
    }
}
