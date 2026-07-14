using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public static Manager Instance;
    [HideInInspector]public Player player;
    [HideInInspector]public Ship ship;
    [HideInInspector]public CameraScript cam;
    [SerializeField] public GameObject mouseObject;
    [HideInInspector]public List<Enemy> enemies = new List<Enemy>();
    [HideInInspector]public List<Interactable> interactables = new List<Interactable>();

    [HideInInspector] public List<Detectable> detectables = new List<Detectable>();
    Volume healthVolume;

    PublicTimer playerRespawnTimer = new PublicTimer(50f);
    bool playerDead = false;

    float maxShipYposition = 60f;

    bool shipCanExitAtmosphere = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

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

    void FixedUpdate()
    {
        CheckShipBreakAtmosphere();
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

    void CheckShipBreakAtmosphere()
    {
        float ShipSlowDownLerpSpeed = 3f;

        if(ship.transform.position.y > maxShipYposition)
        {
            if(shipCanExitAtmosphere)
            {
                SceneManager.LoadScene("WorldTransition");
            }
            else
            {
                Vector2 shipAproachAtmosphereVector = new Vector2(ship.rb.linearVelocityX,0);
                ship.rb.linearVelocity = Vector2.Lerp(ship.rb.linearVelocity,shipAproachAtmosphereVector,ShipSlowDownLerpSpeed * Time.fixedDeltaTime);
                print(ship.rb.linearVelocityY);
            }
        }


    }
    
}
