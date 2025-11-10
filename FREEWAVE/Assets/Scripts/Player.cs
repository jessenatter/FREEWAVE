using UnityEngine;

public class Player : MonoBehaviour
{
    float moveSpeed = 3f;
    float jumpForce = 5f;

    bool grounded,isJumping;

    float cayoteTimer = 10, cayoteTimerCurrent = 0;

    [SerializeField] LayerMask groundLayer;

    Rigidbody2D rb;

    BoxCollider2D bc;

    Manager manager;

    float xInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
    }

    void Update() //reading input, visuals
    {
        xInput = manager.moveAction.ReadValue<Vector2>().x;

        if (manager.jumpAction.IsPressed())
            Jump();
    }

    void FixedUpdate() //rb stuff
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, bc.size * 0.9f, 0, Vector2.down, 0.1f,groundLayer);

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
}
