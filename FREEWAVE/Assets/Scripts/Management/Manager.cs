using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public static Manager Instance;
    [HideInInspector]public InputAction moveAction, jumpAction,interactAction,dodgeAction,pointAction,attackAction,lookAction,switchAimedAction,aimAction,useDrugAction,switchMeleeAction,switchDrugAction;
    [HideInInspector]public Player player;
    [HideInInspector]public Ship ship;
    [HideInInspector]public CameraScript cam;
    [SerializeField] public GameObject mouseObject;
    [HideInInspector]public List<Enemy> enemies = new List<Enemy>();
    [HideInInspector]public List<Interactable> interactables = new List<Interactable>();
    Volume healthVolume;

    PublicTimer playerRespawnTimer = new PublicTimer(50f);
    bool playerDead = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        interactAction = InputSystem.actions.FindAction("Interact");
        dodgeAction = InputSystem.actions.FindAction("Dodge");
        pointAction = InputSystem.actions.FindAction("Point");
        attackAction = InputSystem.actions.FindAction("Attack");
        lookAction = InputSystem.actions.FindAction("Look");
        switchAimedAction = InputSystem.actions.FindAction("SwitchAimed");
        useDrugAction = InputSystem.actions.FindAction("UseDrug");
        aimAction = InputSystem.actions.FindAction("Aim");
        switchDrugAction = InputSystem.actions.FindAction("SwitchDrug");
        switchMeleeAction = InputSystem.actions.FindAction("SwitchMelee");

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
            if(playerRespawnTimer.TickLoop())
            {
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
        playerRespawnTimer.Reset();
        playerDead = true;
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
}
