using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public InputAction moveAction, jumpAction,interactAction,dashAction,pointAction,attackAction,lookAction;
    public Player player;
    public Ship ship;
    public CameraScript cam;
    [SerializeField] public GameObject mouseObject;

    public List<PickupAble> pickupAbles = new List<PickupAble>();

    public List<GameObject> corpses = new List<GameObject>();
    Volume healthVolume;

    float playerRespawnTimer = 50,playerRespawnCurrent;
    bool playerDead = false;
    void Awake() //awake runs before start 
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        interactAction = InputSystem.actions.FindAction("Interact");
        dashAction = InputSystem.actions.FindAction("Dash");
        pointAction = InputSystem.actions.FindAction("Point");
        attackAction = InputSystem.actions.FindAction("Attack");
        lookAction = InputSystem.actions.FindAction("Look");

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        ship = GameObject.FindGameObjectWithTag("Ship").GetComponent<Ship>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraScript>();

        healthVolume = GetComponent<Volume>();
    }

    void Update()
    {
        if (Keyboard.current.rKey.isPressed)
            RestartScene();
        
        if(playerDead)
        {
            playerRespawnCurrent++;
            if(playerRespawnCurrent == playerRespawnTimer)
            {
                playerRespawnCurrent = 0;
                RespawnPlayer();
            }
        }

        healthVolume.weight = 1 - (player.health / 10);
    }
    
    void RespawnPlayer()
    {
        playerDead = false;
        RestartScene();
    }
    public void PlayerDie()
    {
        playerDead = true;
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
