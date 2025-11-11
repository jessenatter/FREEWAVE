using UnityEngine;

public class Player : MonoBehaviour
{
    float moveSpeed = 3.5f;
    float jumpForce = 2f;

    bool grounded,isJumping;

    float cayoteTimer = 10, cayoteTimerCurrent = 0;

    [SerializeField] LayerMask groundLayer;

    Rigidbody2D rb;

    BoxCollider2D bc;

    Manager manager;

    float xInput;

    Ship ship;

    float maxDistanceFromShip = 1f;

    public bool canEnterShip;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
        ship = GameObject.FindGameObjectWithTag("Ship").GetComponent<Ship>();
    }

    void Update() //reading input, visuals
    {
        if (manager.GameState == Manager.gameState.playerControl)
            GetInputs();
    }

    void FixedUpdate() //rb stuff
    {
        if (manager.GameState == Manager.gameState.playerControl)
            MovementUpdate();
    }
    
    void GetInputs()
    {
        xInput = manager.moveAction.ReadValue<Vector2>().x;

        if (manager.jumpAction.IsPressed())
            Jump();

        if (manager.interactAction.IsPressed())
            Interact();
    }
    void MovementUpdate()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, bc.size * 0.9f, 0, Vector2.down, 0.1f, groundLayer);

        if (hit.collider != null)
        {
            grounded = true;
            cayoteTimerCurrent = 0;
            isJumping = false;
        }
        else if (grounded)
        {
            cayoteTimerCurrent++;
            if (cayoteTimerCurrent == cayoteTimer)
            {
                grounded = false;
                cayoteTimerCurrent = 0;
            }
        }

        rb.linearVelocityX = xInput * moveSpeed;
    }
    void Jump()
    {
        if (grounded && !isJumping)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isJumping = true;
        }
    }
    void Interact()
    {
        Vector2 distanceFromShip = transform.position - ship.transform.position;

        if (distanceFromShip.magnitude < maxDistanceFromShip)
            canEnterShip = true;
        else
            canEnterShip = false;
    }
}
