using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public InputAction moveAction, jumpAction,interactAction,dashAction;
    public Player player;

    public Ship ship;

    public CameraScript cam;

    public enum gameState
    {
        playerControl,
        shipControl,
    }

    public gameState GameState = gameState.playerControl;

    bool interactKeyReleased = true;

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        interactAction = InputSystem.actions.FindAction("Interact");
        dashAction = InputSystem.actions.FindAction("Dash");

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        ship = GameObject.FindGameObjectWithTag("Ship").GetComponent<Ship>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraScript>();
    }

    void Update()
    {
        if (Keyboard.current.rKey.isPressed)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        EnterExitShipCheck();
        
    }
    void EnterExitShipCheck()
    {
        if (interactAction.IsPressed())
        {
            if (interactKeyReleased == true)
            {
                interactKeyReleased = false;

                if (GameState == gameState.shipControl)
                    ExitShip();
                else if (player.canEnterShip)
                    EnterShip();
            }
        }
        else
            interactKeyReleased = true;
    }
    public void EnterShip()
    {
        if (Mathf.Round(ship.rb.rotation) == 0 || Mathf.Round(ship.rb.rotation) == 360)
        {
            GameState = gameState.shipControl;
            player.gameObject.SetActive(false);
            cam.EnterShip();
        }
        else
            FlipShip();
    }
    public void ExitShip()
    {
        GameState = gameState.playerControl;
        player.gameObject.SetActive(true);
        player.transform.position = ship.transform.position;
        cam.ExitShip();
    }

    public void FlipShip()
    {
        ship.transform.position += new Vector3(0, 1f,0f);
        ship.rb.rotation = 0;
    }
}
