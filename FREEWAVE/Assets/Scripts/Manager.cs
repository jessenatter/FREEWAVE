using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public InputAction moveAction, jumpAction;
    public Player player;

    public CameraScript cam;

    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraScript>();
    }

    void Update()
    {
        if (Keyboard.current.rKey.isPressed)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
