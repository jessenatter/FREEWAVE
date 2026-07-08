using System;
using System.Collections.Generic;
using UnityEngine;

public static class PublicUtilities
{
    public static float TimerProgress(float current, float duration)
    {
        if(duration <= 0f)
            return 1f;

        return Mathf.Clamp01(current / duration);
    }

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

[Serializable]
public class PublicTimer
{
    [SerializeField] float duration;
    [SerializeField] [HideInInspector] float current;

    public float Duration => duration;
    public float Current => current;
    public float Remaining => Mathf.Max(0f, duration - current);
    public float Progress => PublicUtilities.TimerProgress(current, duration);
    public bool IsComplete => duration <= 0f || current >= duration;
    public bool HasProgress => current > 0f;

    public PublicTimer(float timerDuration = 0f)
    {
        SetDuration(timerDuration);
        Reset();
    }

    public void SetDuration(float timerDuration, bool clampCurrent = true)
    {
        duration = Mathf.Max(0f, timerDuration);

        if(clampCurrent)
            current = Mathf.Clamp(current, 0f, duration);
    }

    public bool Tick(float step = 1f)
    {
        if(duration <= 0f)
            return true;

        float previous = current;
        current = Mathf.Min(current + step, duration);
        return previous < duration && current >= duration;
    }

    public bool TickLoop(float step = 1f)
    {
        if(duration <= 0f)
            return true;

        current += step;
        if(current >= duration)
        {
            current = 0f;
            return true;
        }

        return false;
    }

    public void Reset(float startAt = 0f)
    {
        if(duration <= 0f)
        {
            current = 0f;
            return;
        }

        current = Mathf.Clamp(startAt, 0f, duration);
    }

    public void Complete()
    {
        current = duration;
    }
}
