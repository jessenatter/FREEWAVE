using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager
{
    [Header("Input Actions")]
    public InputAction moveAction, jumpAction, attackAction, interactAction,dodgeAction, aimAction, shootAction;


    public virtual void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        //grabAction = InputSystem.actions.FindAction("Grab");
    }

    public virtual void Update()
    {
        //movingEntityBehaviour.moveInput = moveAction.ReadValue<Vector2>();
        //movingEntityBehaviour.dashInput = dashAction.WasPressedThisFrame();
        //movingEntityBehaviour.grabInput = grabAction.IsPressed();
    }

}