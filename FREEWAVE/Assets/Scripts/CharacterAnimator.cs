using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] LimbManager frontArm,backArm,frontLeg,backLeg;
    [SerializeField] GameObject spine1,spine2,head;
    public class characterState
    {
        public LimbManager.limbState frontArmState,backArmState,frontLegState,backLegState;

        
    }

    public characterState currentCharacterState;

    void Start()
    {
        
    }
    void Update()
    {
        
    }
}
