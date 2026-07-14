using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    [HideInInspector] public InputAction moveAction, jumpAction, interactAction, dodgeAction, pointAction, attackAction, lookAction, switchLeftAction, aimAction, useDrugAction, switchRightAction, switchDrugAction;

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
        switchLeftAction = InputSystem.actions.FindAction("SwitchLeft");
        useDrugAction = InputSystem.actions.FindAction("UseDrug");
        aimAction = InputSystem.actions.FindAction("Aim");
        switchDrugAction = InputSystem.actions.FindAction("SwitchDrug");
        switchRightAction = InputSystem.actions.FindAction("SwitchRight");
    }
}
