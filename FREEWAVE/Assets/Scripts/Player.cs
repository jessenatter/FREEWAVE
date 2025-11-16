using UnityEngine;
using UnityEngine.InputSystem;
public class Player : Character
{
    bool isGrappling,grappleIsShooting;
    [SerializeField] LayerMask grappleLayer;
    Ship ship;
    float maxDistanceFromShip = 1f;
    public bool canEnterShip,aiming;
    Vector2 mouseWorld,grapplePoint;
    [SerializeField] GameObject grappleBullet;
    LineRenderer lineRenderer;
    override protected void Start()
    {
        base.Start();

        ship = GameObject.FindGameObjectWithTag("Ship").GetComponent<Ship>();
        lineRenderer = GetComponent<LineRenderer>();
        
        lineRenderer.positionCount = 2;
        float width = 0.015f;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
    }
    override protected void Update() //reading input, visuals
    {
        base.Update();

        if (manager.GameState == Manager.gameState.playerControl)
        {
            characterIsActive = true;
            GetInputs();
        }
        else
            characterIsActive = false;
    }
    override protected void FixedUpdate() //rb stuff
    {
        base.FixedUpdate();

        if(aiming)
        {
            UpdateMouseObject();
        }
    }
    protected override void MovementUpdate()
    {
        if(!isGrappling)
            base.MovementUpdate();
        else 
            GrappleStateUpdate();
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
            isGrappling = true;
            grapplePoint = hit.point;
            grappleBullet.transform.position = grapplePoint;
            rb.gravityScale = 0;
            lineRenderer.enabled = true;
        }
    }
    void GrappleStateUpdate()
    {
        if(grappleIsShooting)
            GrappleShootingUpdate();
        else
            GrapplingUpdate();

        lineRenderer.SetPosition(0,transform.position);
        lineRenderer.SetPosition(1,grappleBullet.transform.position);
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
            lineRenderer.enabled = false;
        }
    }
}
