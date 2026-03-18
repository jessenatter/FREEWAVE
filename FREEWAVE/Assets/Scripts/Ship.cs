using Unity.VisualScripting;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public Rigidbody2D rb;
    Manager manager;
    float turnForceMax = 5, turnTimer = 50,turnTimerCurrent,turnAmmount, moveForce = 80;
    [SerializeField] AnimationCurve turnCurve;
    float maxSpeed = 15;
    float boostForce = 75;
    float xInput,lastXinput;
    bool mainEngine, reverseEngine;
    [SerializeField] GameObject mainFlame, reverseFlame, leftFlame, rightFlame;
    [SerializeField] ParticleSystem mainSmoke, reverseSmoke1,reverseSmoke2, leftSmoke, rightSmoke;
    [SerializeField] LayerMask breakableWallLayer;
    bool mainEngineReleased = true, waitingForDoubleClick;
    float doubleClickTimer = 15, doubleClickTimerCurrent;
    float boostCDTimer = 40,boostCDTimerCurrent;
    bool canBoost = true;
    float tryBoostTimer = 50,tryBoostTimerCurrent;
    float boostTimer = 75,boostTimerCurrent;
    Breakable breakable;
    [SerializeField] GameObject explosion;
    Player player;
    bool interactKeyReleased;
    public enum ShipState
    {
        waitingForPlayer,
        flying,
        boosting,
        boostingToWindow,
    }

    public ShipState currentShipState = ShipState.waitingForPlayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        player = manager.player;
    }

    void Update()
    {
        if(currentShipState != ShipState.waitingForPlayer)
            ReadInputs();
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
        reverseEngine = manager.dashAction.IsPressed();

        if(manager.interactAction.IsPressed())
        {
            if(interactKeyReleased)
                ExitShip();

            interactKeyReleased = false;
        }
        else
            interactKeyReleased = true;
    }
    void UpdateBoostCDTimer()
    {
        if(!canBoost)
        {
            boostCDTimerCurrent++;
            if(boostCDTimerCurrent == boostCDTimer)
            {
                canBoost = true;
                boostCDTimerCurrent = 0;
            }
        }
    }

    void UpdateBoost()
    {
        if(!canBoost)
        {
            boostTimerCurrent++;
            if(boostTimerCurrent == boostTimer)
            {
                currentShipState = ShipState.flying;
                boostTimerCurrent = 0;
            }
        }
    }
    void RegularMovementUpdate()
    {
        if(xInput != 0 && Mathf.Sign(xInput) == lastXinput) 
        {
            rb.angularVelocity = 0;
            turnTimerCurrent++;
            turnTimerCurrent = Mathf.Clamp(turnTimerCurrent,0,turnTimer);
            float t = turnTimerCurrent/turnTimer;
            turnAmmount = turnCurve.Evaluate(t);
            transform.Rotate(0,0,turnAmmount * xInput * turnForceMax);
        }
        else
        {
            if(turnTimerCurrent != 0)
            {
                rb.angularVelocity = 0;
                rb.AddTorque(turnAmmount * 200 * Mathf.Sign(lastXinput));
                turnTimerCurrent = 0;
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

            if (waitingForDoubleClick)
            {
                if (mainEngineReleased)
                {
                    Boost();
                    waitingForDoubleClick = false;
                }
            }
            else
                waitingForDoubleClick = true;

            mainEngineReleased = false;
        }
        else
        {
            mainSmoke.Stop();
            mainFlame.SetActive(false);
            mainEngineReleased = true;
        }

        if(waitingForDoubleClick)
        {
            doubleClickTimerCurrent++;
            if(doubleClickTimerCurrent == doubleClickTimer)
            {
                doubleClickTimerCurrent = 0;
                waitingForDoubleClick = false;
            }
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
            manager.cam.StartScreenShake(15,0.05f);

            float breakBoostForce = 15f;
            rb.AddForce(toBreakable.normalized * breakBoostForce,ForceMode2D.Impulse);

            tryBoostTimerCurrent = 0;
            currentShipState = ShipState.flying;
        }

        tryBoostTimerCurrent++;

        if(tryBoostTimerCurrent == tryBoostTimer)
        {
            tryBoostTimerCurrent = 0;
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
        }

        float distance = 10f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, distance, breakableWallLayer);
        if (hit == true)
        {
            currentShipState = ShipState.boostingToWindow;
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
}
