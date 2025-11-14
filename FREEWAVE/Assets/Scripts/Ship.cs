using Unity.VisualScripting;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public Rigidbody2D rb;
    Manager manager;

    float turnForce = 40, turnTorque = 10, moveForce = 90;
    float maxSpeed = 15;

    float boostForce = 40;
    float xInput;

    bool inShip, mainEngine, reverseEngine;

    float ajustLerp = 0.1f;

    [SerializeField] GameObject mainFlame, reverseFlame, leftFlame, rightFlame;

    [SerializeField] LayerMask breakableWallLayer;

    bool mainEngineReleased = true, waitingForDoubleClick;

    float doubleClickTimer = 15, doubleClickTimerCurrent;

    bool breakingWall = false;

    Breakable breakable;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
    }

    void Update()
    {
        if (manager.GameState == Manager.gameState.shipControl)
            inShip = true;
        else
            inShip = false;

        if (inShip)
            ReadInputs();
    }

    void FixedUpdate()
    {
        if (inShip)
            UpdateMovement();
    }

    void ReadInputs()
    {
        xInput = -Mathf.Sign(manager.moveAction.ReadValue<Vector2>().x) * Mathf.Abs(manager.moveAction.ReadValue<Vector2>().x);
        mainEngine = manager.jumpAction.IsPressed();
        reverseEngine = manager.dashAction.IsPressed();
    }
    void UpdateMovement()
    {

        if (!breakingWall)
            RegularMovementUpdate();
        else
            BreakingUpdate();

        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
    }

    void BreakableWallCheck()
    {
        float distance = 10f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, distance, breakableWallLayer);
        if (hit == true)
        {
            breakingWall = true;
            breakable = hit.collider.gameObject.GetComponent<Breakable>();
        }
    }

    void RegularMovementUpdate()
    {
        Vector2 forceDir = new Vector2(xInput, 0);
        rb.AddForce(transform.up * forceDir * turnForce);
        rb.AddTorque(turnTorque * forceDir.x);

        if (Mathf.Sign(xInput) == 1 && xInput != 0)
            rightFlame.SetActive(true);
        else
            rightFlame.SetActive(false);

        if (Mathf.Sign(xInput) == -1)
            leftFlame.SetActive(true);
        else
            leftFlame.SetActive(false);

        if (mainEngine)
        {
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
            rb.AddForce(-transform.up * moveForce);
            reverseFlame.SetActive(true);
        }
        else
            reverseFlame.SetActive(false);
    }
    
    void BreakingUpdate()
    {
        Vector2 toBreakable = breakable.transform.position - transform.position;

        float boostSpeed = 15;
        rb.linearVelocity = toBreakable.normalized * boostSpeed;
        if(toBreakable.magnitude < 1.2f)
        {
            breakable.Break();
            breakingWall = false;

            float breakBoostForce = 15f;
            rb.AddForce(toBreakable.normalized * breakBoostForce,ForceMode2D.Impulse);
        }
    }

    void Boost()
    {
        rb.AddForce(transform.up * boostForce, ForceMode2D.Impulse);
        BreakableWallCheck();
    }       
}
