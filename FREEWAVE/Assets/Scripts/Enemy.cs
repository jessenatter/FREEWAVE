using UnityEngine;

public class Enemy : Character
{
    Player player;

    float awarenessDistance = 3f;

    float awarenessTimer = 100,awarenessTimerCurrent;

    protected override void Start()
    {
        base.Start();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        moveSpeed = 1.5f;
        jumpForce = 1.5f;
    }
    protected override void Update()
    {
        base.Update();

        Vector2 playerEnemyVector = player.transform.position - transform.position;

        if(playerEnemyVector.magnitude < awarenessDistance)
            characterIsActive = true;
        else if(playerEnemyVector.magnitude > awarenessDistance + 1f)
        {
            awarenessTimerCurrent++;
            if(awarenessTimerCurrent == awarenessTimer)
            {
                awarenessTimerCurrent = 0;
                characterIsActive = false;
            }
        }

        xInput = playerEnemyVector.x;
    }
}
