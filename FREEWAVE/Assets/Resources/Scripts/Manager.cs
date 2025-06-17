using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class Manager : MonoBehaviour
{
    Player player = new Player();

    List<PrimaryClass> alwaysUpdate = new List<PrimaryClass>();

    public int groundLayer, characterLayer, buildingLayer, shipLayer, environmentLayer;
    public LayerMask groundMask, characterMask, buildingMask, shipMask, environmentMask;
    public InputAction moveAction, jumpAction, attackAction, interactAction, dodgeAction, aimAction, shootAction;

    private void Awake()
    {
        groundLayer = LayerMask.NameToLayer("Ground");
        characterLayer = LayerMask.NameToLayer("Character");
        buildingLayer = LayerMask.NameToLayer("Building");
        shipLayer = LayerMask.NameToLayer("Ship");

        groundMask = LayerMask.GetMask("Ground");
        characterMask = LayerMask.GetMask("Character");
        buildingMask = LayerMask.GetMask("Building");
        shipMask = LayerMask.GetMask("Ship");

        alwaysUpdate.Add(player);

        foreach (PrimaryClass primary in alwaysUpdate)
            primary.Start(this);

        StartInputs();
    }

    void StartInputs()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    void FixedUpdate()
    {
        foreach(PrimaryClass primary in alwaysUpdate)
            primary.Update();
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

public class CameraClass : PrimaryClass
{
    Camera cam;

}