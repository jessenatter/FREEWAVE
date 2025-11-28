using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public InputAction moveAction, jumpAction,interactAction,dashAction,pointAction,attackAction;
    public Player player;
    public Ship ship;
    public CameraScript cam;
    [SerializeField] public GameObject mouseObject;

    public List<PickupAble> pickupAbles = new List<PickupAble>();
    void Awake() //awake runs before start 
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        interactAction = InputSystem.actions.FindAction("Interact");
        dashAction = InputSystem.actions.FindAction("Dash");
        pointAction = InputSystem.actions.FindAction("Point");
        attackAction = InputSystem.actions.FindAction("Attack");

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        ship = GameObject.FindGameObjectWithTag("Ship").GetComponent<Ship>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraScript>();
    }

    void Update()
    {
        if (Keyboard.current.rKey.isPressed)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
    }
}
