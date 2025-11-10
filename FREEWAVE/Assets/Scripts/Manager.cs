using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public InputAction moveAction, jumpAction,interactAction;
    public Player player;

    public Ship ship;

    public CameraScript cam;

    public enum gameState
    {
        playerControl,
        shipControl,
    }

    public gameState GameState = gameState.playerControl;

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        interactAction = InputSystem.actions.FindAction("Interact");

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        ship = GameObject.FindGameObjectWithTag("Ship").GetComponent<Ship>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraScript>();
    }

    void Update()
    {
        if (Keyboard.current.rKey.isPressed)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void EnterShip()
    {
        GameState = gameState.shipControl;
        player.gameObject.SetActive(false);
    }

    public void ExitShip()
    {
        GameState = gameState.playerControl;
        player.gameObject.SetActive(true);
    }
}
