using UnityEngine;

[ExecuteInEditMode] 
public class LimbEditMode : MonoBehaviour
{
    [SerializeField] GameObject frontHand,backHand,frontLeg,backLeg;
    [SerializeField] GameObject frontHandT,backHandT,frontLegT,backLegT;
    [SerializeField] bool resetLimbs = false;

    void Update()
    {
        if(resetLimbs)
        {
            frontHand.transform.position = frontHandT.transform.position;
            backHand.transform.position = backHandT.transform.position;
            frontLeg.transform.position = frontLegT.transform.position;
            backLeg.transform.position = backLegT.transform.position;

            resetLimbs = false;
        }
    }
}
