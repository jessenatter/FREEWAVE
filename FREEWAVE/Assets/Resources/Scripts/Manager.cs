using System.Collections.Generic;
using UnityEngine;
using System;

public class Manager : MonoBehaviour
{
    Player player = new Player();

    List<PrimaryClass> alwaysUpdate = new List<PrimaryClass>();

    private void Awake()
    {
        alwaysUpdate.Add(player);

        foreach (PrimaryClass primary in alwaysUpdate)
            primary.Start();
    }

    void Update()
    {
        foreach(PrimaryClass primary in alwaysUpdate)
            primary.Update();
    }
}

public class PrimaryClass
{
    public virtual void Start()
    {

    }
    public virtual void Update()
    {

    }
}

public class CameraClass : PrimaryClass
{
    Camera cam;

}