using UnityEngine;

public class Ship : MonoBehaviour
{
    Rigidbody2D rb;
    Manager manager;

    float turnForce = 20, turnTorque = 3, moveForce = 75;
    float maxSpeed = 10;
    float xInput;

    bool inShip, mainEngine, reverseEngine;

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
        mainEngine = manager.jumpAction.IsPressed();
        reverseEngine = manager.dashAction.IsPressed();
    }
    void UpdateMovement()
    {
        Vector2 forceDir = new Vector2(xInput, 0);
        rb.AddForce(transform.up * forceDir * turnForce);
        rb.AddTorque(turnTorque * forceDir.x);

        if (mainEngine)
            rb.AddForce(transform.up * moveForce);

        if (reverseEngine)
            rb.AddForce(-transform.up * moveForce);

        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);

        //Debug.Log(transform.rotation.z);
        Debug.Log(transform.eulerAngles.z);
    }       
}
