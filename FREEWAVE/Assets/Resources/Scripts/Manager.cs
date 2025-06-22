using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class Manager : MonoBehaviour
{
    public Player player = new Player();
    CameraClass cameraClass = new CameraClass();

    List<PrimaryClass> alwaysUpdate = new List<PrimaryClass>();
    List<Zombie> zombies = new List<Zombie>();

    List<Vector2> zombieSpawnPoints = new List<Vector2>();
    float spawnDistance = 3f;

    [HideInInspector] public int groundLayer, characterLayer, buildingLayer, shipLayer, environmentLayer;
    [HideInInspector] public LayerMask groundMask, characterMask, buildingMask, shipMask, environmentMask;
    [HideInInspector] public InputAction moveAction, jumpAction, attackAction, interactAction, dodgeAction, aimAction, shootAction,grabAction;

    public Zombie closestZombie;
    float shortestZombieDistance;

    private void Awake()
    {
        StartGameData();

        groundLayer = LayerMask.NameToLayer("Ground");
        characterLayer = LayerMask.NameToLayer("Character");
        buildingLayer = LayerMask.NameToLayer("Building");
        shipLayer = LayerMask.NameToLayer("Ship");

        groundMask = LayerMask.GetMask("Ground");
        characterMask = LayerMask.GetMask("Character");
        buildingMask = LayerMask.GetMask("Building");
        shipMask = LayerMask.GetMask("Ship");

        alwaysUpdate.Add(player);
        alwaysUpdate.Add(cameraClass);

        zombieSpawnPoints.Add(new Vector2(20, 0));
        zombieSpawnPoints.Add(new Vector2(40, 0));

        foreach (Vector2 zombieSpawnPoint in zombieSpawnPoints)
        {
            Zombie newZombie = new Zombie();
            newZombie.Start(this);
            newZombie.gameObject.transform.position = zombieSpawnPoint;
            zombies.Add(newZombie);
            newZombie.gameObject.SetActive(false);
        }

        foreach (PrimaryClass primary in alwaysUpdate)
            primary.Start(this);

        StartInputs();
    }

    void StartGameData()
    {
        GameData.manager = this;
    }

    void StartInputs()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        grabAction = InputSystem.actions.FindAction("Grab");
    }

    void FixedUpdate()
    {
        if (Keyboard.current.rKey.IsPressed())
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }

        foreach (PrimaryClass primary in alwaysUpdate)
            primary.Update();

        closestZombie = null;
        shortestZombieDistance = 1000;

        foreach (Zombie zombie in zombies)
        {
            if (zombie.gameObject.activeInHierarchy)
            {
                float distanceFromZombie = Vector2.Distance(zombie.gameObject.transform.position, player.gameObject.transform.position);

                if (distanceFromZombie < shortestZombieDistance)
                {
                    shortestZombieDistance = distanceFromZombie;
                    closestZombie = zombie;
                }

                zombie.Update();
            }
        }

        for (int i = 0; i < zombieSpawnPoints.Count; i++)
        {
            Vector2 spawnPoint = zombieSpawnPoints[i];

            if (Vector2.Distance(player.gameObject.transform.position, spawnPoint) < spawnDistance)
                zombies[i].gameObject.SetActive(true);
            else if(Vector2.Distance(player.gameObject.transform.position, zombies[i].gameObject.transform.position) > spawnDistance)
                zombies[i].gameObject.SetActive(false);
        }
    }
}

public class PrimaryClass
{
    public Manager manager;

    public virtual void Start(Manager _manager)
    {
        manager = _manager;
    }
    public virtual void Update()
    {

    }
}