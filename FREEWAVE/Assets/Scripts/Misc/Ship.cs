using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ship : MonoBehaviour
{
    public Rigidbody2D rb;
    Manager manager;
    float turnForceMax = 5,turnAmmount, moveForce = 80;
    PublicTimer turnTimer = new PublicTimer(50f);
    [SerializeField] AnimationCurve turnCurve;
    float maxSpeed = 15;
    float boostForce = 75;
    float xInput,lastXinput;
    bool mainEngine, reverseEngine;
    [SerializeField] GameObject mainFlame, reverseFlame, leftFlame, rightFlame;
    [SerializeField] ParticleSystem mainSmoke, reverseSmoke1,reverseSmoke2, leftSmoke, rightSmoke;
    [SerializeField] LayerMask breakableWallLayer;
    PublicTimer boostCDTimer = new PublicTimer(40f);
    bool canBoost = true;
    PublicTimer tryBoostTimer = new PublicTimer(50f);
    PublicTimer boostTimer = new PublicTimer(75f);
    Breakable breakable;
    [SerializeField] GameObject explosion;
    Player player;
    bool interactKeyReleased;
    AudioSource engineAudioSource;
    float engineLerpSpeed = 2f;
    float engineBasePitch = 1f,engineThrottlePitch = 0.2f, engineTurnPitch = 0.08f;
    float engineAudioVolume;
    float engineAudioPitch;
    public enum ShipState
    {
        waitingForPlayer,
        flying,
        boosting,
        boostingToWindow,
    }

    public ShipState currentShipState = ShipState.waitingForPlayer;

    InputAction mainEngineAction,reverseEngineAction, moveAction,boostAction1,boostAction2;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (engineAudioSource == null)
            engineAudioSource = GetComponent<AudioSource>();

        if (engineAudioSource != null)
        {
            engineAudioSource.loop = true;
            engineAudioSource.playOnAwake = false;
            engineAudioSource.volume = 0f;
            engineAudioSource.pitch = engineBasePitch;
            engineAudioVolume = 0f;
            engineAudioPitch = engineBasePitch;
        }

        if(Manager.Instance != null)
            manager = Manager.Instance;
        
        player = manager.player;
    }

    void Update()
    {
        if(currentShipState != ShipState.waitingForPlayer)
            ReadInputs();

        UpdateEngineAudio();
    }

    void FixedUpdate()
    {
        if(currentShipState == ShipState.flying)
        {
            RegularMovementUpdate();
            
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
        }
        else if(currentShipState == ShipState.boosting)
        {
            UpdateBoost();
            RegularMovementUpdate();
        }
        else if(currentShipState == ShipState.boostingToWindow)
            TargetedBoostUpdate();

        UpdateBoostCDTimer();
    }

    void ReadInputs()
    {
        xInput = -Mathf.Sign(manager.moveAction.ReadValue<Vector2>().x) * Mathf.Abs(manager.moveAction.ReadValue<Vector2>().x);

        if(xInput != 0)
            lastXinput = Mathf.Sign(xInput);

        mainEngine = manager.jumpAction.IsPressed();
        reverseEngine = manager.dodgeAction.IsPressed();

        if(manager.interactAction.IsPressed())
        {
            if(interactKeyReleased)
                ExitShip();

            interactKeyReleased = false;
        }
        else
            interactKeyReleased = true;

        if(Manager.Instance.useDrugAction.IsPressed() || Manager.Instance.switchLeftAction.IsPressed() || Manager.Instance.switchRightAction.IsPressed())
            Boost();

        if(Manager.Instance.attackAction.IsPressed())
            Shoot();
    }

    void UpdateEngineAudio()
    {
        if (engineAudioSource == null)
            return;

        bool engineActive = currentShipState != ShipState.waitingForPlayer && (mainEngine || reverseEngine || xInput != 0f);
        float targetVolume = engineActive ? 1f : 0f;

        engineAudioVolume = Mathf.MoveTowards(engineAudioVolume, targetVolume, engineLerpSpeed * Time.deltaTime);

        float turnAmount = Mathf.Abs(xInput);
        float turnPitchOffset = turnAmount > 0f ? turnAmount * engineTurnPitch : 0f;
        float throttlePitchOffset = 0f;

        if (mainEngine)
            throttlePitchOffset += engineThrottlePitch;

        if (reverseEngine)
            throttlePitchOffset += engineThrottlePitch * 0.75f;

        float targetPitch = engineBasePitch + throttlePitchOffset + turnPitchOffset;
        engineAudioPitch = Mathf.MoveTowards(engineAudioPitch, targetPitch, engineLerpSpeed * Time.deltaTime);

        if (engineAudioVolume <= 0.001f)
        {
            engineAudioVolume = 0f;
            engineAudioPitch = engineBasePitch;
            engineAudioSource.Stop();
            engineAudioSource.volume = 0f;
            engineAudioSource.pitch = engineBasePitch;
            return;
        }

        if (!engineAudioSource.isPlaying)
            engineAudioSource.Play();

        engineAudioSource.volume = engineAudioVolume * 0.4f;
        engineAudioSource.pitch = engineAudioPitch;
    }
    void UpdateBoostCDTimer()
    {
        if(!canBoost)
        {
            if(boostCDTimer.TickLoop())
            {
                canBoost = true;
            }
        }
    }

    void UpdateBoost()
    {
        if(!canBoost)
        {
            if(boostTimer.TickLoop())
            {
                currentShipState = ShipState.flying;
            }
        }
    }
    void RegularMovementUpdate()
    {
        if(xInput != 0 && Mathf.Sign(xInput) == lastXinput) 
        {
            rb.angularVelocity = 0;
            turnTimer.Tick();
            float t = turnTimer.Progress;
            turnAmmount = turnCurve.Evaluate(t);
            transform.Rotate(0,0,turnAmmount * xInput * turnForceMax);
        }
        else
        {
            if(turnTimer.HasProgress)
            {
                rb.angularVelocity = 0;
                rb.AddTorque(turnAmmount * 200 * Mathf.Sign(lastXinput));
                turnTimer.Reset();
            }
        }

        if (Mathf.Sign(xInput) == 1 && xInput != 0)
        {
            rightFlame.SetActive(true);
            rightSmoke.Play();
        }
        else
        {
            rightFlame.SetActive(false);
            rightSmoke.Stop();
        }

        if (Mathf.Sign(xInput) == -1)
        {
            leftFlame.SetActive(true);
            leftSmoke.Play();
        }
        else
        {
            leftFlame.SetActive(false);
            leftSmoke.Stop();
        }

        if (mainEngine)
        {
            mainSmoke.Play();
            rb.AddForce(transform.up * moveForce);
            mainFlame.SetActive(true);
        }
        else
        {
            mainSmoke.Stop();
            mainFlame.SetActive(false);
        }

        if (reverseEngine)
        {
            reverseSmoke1.Play();
            reverseSmoke2.Play();
            rb.AddForce(-transform.up * moveForce);
            reverseFlame.SetActive(true);
        }
        else
        {
            reverseFlame.SetActive(false);
            reverseSmoke1.Stop();
            reverseSmoke2.Stop();
        }
    }  
    void TargetedBoostUpdate()
    {
        Vector2 toBreakable = breakable.transform.position - transform.position;

        float boostSpeed = 15;
        rb.linearVelocity = toBreakable.normalized * boostSpeed;
        if(toBreakable.magnitude < 1.2f)
        {
            breakable.Break();
            SoundManager.PlaySound(0.5f,0.2f,"glassSmash1","glassSmash2","glassSmash3");
            SoundManager.PlaySound(0.5f,0.2f,"crash1","crash2");
            manager.cam.StartScreenShake(15,0.05f);

            float breakBoostForce = 15f;
            rb.AddForce(toBreakable.normalized * breakBoostForce,ForceMode2D.Impulse);

            tryBoostTimer.Reset();
            currentShipState = ShipState.flying;
        }

        if(tryBoostTimer.TickLoop())
        {
            currentShipState = ShipState.flying;
        }
    }
    void Boost()
    {
        if(canBoost)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(transform.up * boostForce, ForceMode2D.Impulse);
            GameObject _explosion = Instantiate(explosion);
            _explosion.transform.position = mainFlame.transform.position;
            canBoost = false;
            boostCDTimer.Reset();
            boostTimer.Reset();
        }

        float distance = 10f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, distance, breakableWallLayer);
        if (hit == true)
        {
            currentShipState = ShipState.boostingToWindow;
            tryBoostTimer.Reset();
            breakable = hit.collider.gameObject.GetComponent<Breakable>();
        }
        else
            currentShipState = ShipState.boosting;
    }

    public void EnterShip()
    {
        currentShipState = ShipState.flying;
    }   

    public void ExitShip()
    {
        currentShipState = ShipState.waitingForPlayer;
        player.ExitShip();
        player.gameObject.SetActive(true);
    }    

    void Shoot()
    {
        
    }
}
