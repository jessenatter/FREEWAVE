using UnityEngine;
using UnityEngine.InputSystem;
public class Player : MonoBehaviour
{
    float moveSpeed = 3.5f;
    float jumpForce = 2f;

    bool grounded,isJumping,isGrappling,grappleIsShooting;

    float cayoteTimer = 10, cayoteTimerCurrent = 0;

    [SerializeField] LayerMask groundLayer,grappleLayer;

    Rigidbody2D rb;

    BoxCollider2D bc;

    Manager manager;

    float xInput;

    Ship ship;

    float maxDistanceFromShip = 1f;

    public bool canEnterShip,aiming;

    Vector2 mouseWorld,grapplePoint;

    [SerializeField] GameObject grappleBullet;

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
        {
            if(!isGrappling)
                MovementUpdate();
            else
                GrappleStateUpdate();
        }

        if(aiming)
        {
            UpdateMouseObject();
        }
    }
    
    void GetInputs()
    {
        xInput = manager.moveAction.ReadValue<Vector2>().x;

        if (manager.jumpAction.IsPressed())
            Jump();

        if (manager.interactAction.IsPressed())
            Interact();
        
        if(Mouse.current.rightButton.isPressed)
            aiming = true;
        else
            aiming = false;
        
        if(Mouse.current.leftButton.isPressed && aiming)
            Shoot();
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
    void UpdateMouseObject()
    {
        Vector2 mousePos = manager.pointAction.ReadValue<Vector2>();
        mouseWorld = manager.cam.cameraComponent.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, manager.cam.cameraComponent.WorldToScreenPoint(transform.position).z));
        manager.mouseObject.transform.position = mouseWorld;
    }

    void Shoot()
    {
        Vector2 dir = mouseWorld - (Vector2)transform.position;
        float distance = 50f;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance, grappleLayer);

        if(hit == true)
        {
            if(!isGrappling)
            {
                isGrappling = true;
                grapplePoint = hit.point;
                grappleBullet.transform.position = grapplePoint;
                rb.gravityScale = 0;
            }
        }
    }
    void GrappleStateUpdate()
    {
        if(grappleIsShooting)
            GrappleShootingUpdate();
        else
            GrapplingUpdate();
    }
    void GrappleShootingUpdate()
    {
        
    }
    
    void GrapplingUpdate()
    {
        Vector2 dir = grapplePoint - (Vector2)transform.position;
        float grappleSpeed = 15f;

        rb.AddForce(dir.normalized * grappleSpeed);

        if(dir.magnitude < 1f)
        {
            isGrappling = false;
            rb.gravityScale = 1;
        }
    }
}
