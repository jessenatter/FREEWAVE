using System.Collections.Generic;
using UnityEngine;
using System;

public class Character : PrimaryClass
{
    protected string name;
    public GameObject gameObject,head;
    Limb frontArm, backArm, frontLeg, backLeg;
    List<Limb> limbs = new List<Limb>();
    int xdir;
    Rigidbody2D rb;
    BoxCollider2D bc;
    SpriteRenderer sr;
    Vector2 spawnPoint,boxsize = new Vector2(0.4f,.9f);
    float acceleration = .5f, maxSpeed = 1f;
    UpperBodyState currentUpperBodyState;
    LowerBodyState currenLowerBodyState;
    public Sprite[] sprites;

    override public void Start()
    {
        limbs.AddRange(new[] { frontArm, backArm, frontLeg, backLeg });

        sprites = Resources.LoadAll<Sprite>("Sprites/Characters/" + name.ToString());

        gameObject = new GameObject(name);
        head = new GameObject(name + "head");
        head.AddComponent<SpriteRenderer>().sprite = sprites[5];
        rb = gameObject.AddComponent<Rigidbody2D>();
        bc = gameObject.AddComponent<BoxCollider2D>();
        bc.size = boxsize;
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprites[0];
        gameObject.transform.position = spawnPoint;

        frontLeg = StartLimb(true, false);
        backLeg = StartLimb(true, true);
        frontArm = StartLimb(false, false);
        backArm = StartLimb(false, true);
    }

    Limb StartLimb(bool isLeg, bool isBackLimb)
    {
        Limb limb = new Limb();
        limb.Start(isBackLimb, isLeg);

        if (isLeg)
        {
            limb.srA.sprite = sprites[1];
            limb.srB.sprite = sprites[2];
        }
        else
        {
            limb.srA.sprite = sprites[3];
            limb.srB.sprite = sprites[4];
        }

        limb.partA.transform.SetParent(gameObject.transform);

        return limb;
    }

    override public void Update()
    {
        
    }
}

public class Player : Character
{
    public override void Start()
    {
        name = "Player";

        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }
}

public class BodyState
{

}

public class UpperBodyState : BodyState
{

}

public class LowerBodyState : BodyState
{
    
}

public class UpperBodyIdle : UpperBodyState
{

}

public class LowerBodyIdle : LowerBodyState
{

}