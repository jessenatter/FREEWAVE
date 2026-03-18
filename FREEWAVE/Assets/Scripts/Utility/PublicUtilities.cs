using System.Collections.Generic;
using UnityEngine;

public static class PublicUtilities
{
    public static GameObject closestObject(List<GameObject> gameObjects,Transform transform)
    {
        GameObject _closest = null;

        float lastPickupDistance = Mathf.Infinity;

        foreach(GameObject _gameObject in gameObjects)
        {
            Vector2 distance = transform.position - _gameObject.transform.position;

            if(distance.magnitude < lastPickupDistance || _closest == null)
            {
                _closest = _gameObject;
                lastPickupDistance = distance.magnitude;
            }
        }

        return _closest;
    }
}
