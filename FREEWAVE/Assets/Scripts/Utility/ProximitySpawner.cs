using System;
using UnityEngine;

public class ProximitySpawner : MonoBehaviour
{
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] float distanceToPlayerToSpawn = 10f;
    void Update()
    {
        GameObject target;

        if(Manager.Instance.ship.currentShipState == Ship.ShipState.waitingForPlayer)
            target = Manager.Instance.player.gameObject;
        else
            target = Manager.Instance.ship.gameObject;

        float distance = (target.transform.position - gameObject.transform.position).magnitude;

        if(distance < distanceToPlayerToSpawn)
        {
            GameObject obj = Instantiate(objectToSpawn,transform);
            obj.transform.SetParent(transform.parent);
            Destroy(gameObject);
        }
    }
}
