using Unity.VisualScripting;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public Rigidbody2D rb;
    Manager manager;

    float turnForce = 25, turnTorque = 5, moveForce = 90;
    float maxSpeed = 15;

    float boostForce = 40;
    float xInput;

    bool inShip, mainEngine, reverseEngine;

    float ajustLerp = 0.1f;

    [SerializeField] GameObject mainFlame, reverseFlame, leftFlame, rightFlame;

    [SerializeField] LayerMask breakableWallLayer;

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
        xInput = Mathf.Sign(manager.moveAction.ReadValue<Vector2>().x) * Mathf.Abs(manager.moveAction.ReadValue<Vector2>().x);
        xInput = Mathf.Sign(manager.moveAction.ReadValue<Vector2>().x) * Mathf.Abs(manager.moveAction.ReadValue<Vector2>().x);
        mainEngine = manager.jumpAction.IsPressed();
        reverseEngine = manager.dashAction.IsPressed();
    }
    void UpdateMovement()
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
        }
        else
            mainFlame.SetActive(false);

        if (reverseEngine)
        {
            rb.AddForce(-transform.up * moveForce);
            reverseFlame.SetActive(true);
        }
        else
            reverseFlame.SetActive(false);

        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
    }

    void BreakableWallCheck()
    {
        float distance = 10f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, distance, breakableWallLayer);
        if(hit == true)
        {
            
        }
    }

    void Boost()
    {
        rb.AddForce(transform.up * boostForce,ForceMode2D.Impulse);
    }       
}
