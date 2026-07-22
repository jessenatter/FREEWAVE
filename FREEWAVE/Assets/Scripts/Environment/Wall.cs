using System.Collections.Generic;
using UnityEngine;

public enum WallColor
{
    Red,
    White,
    Grey
}

public class Wall : MonoBehaviour
{
    List <GameObject> bricks = new List<GameObject>();
    [SerializeField] public WallColor wallColor = WallColor.Red;
    [SerializeField] GameObject greyBrick,whiteBrick;

    void Awake()
    {
        for(int i = 0; i < 6; i++)
        {
            bricks.Add(transform.GetChild(i).gameObject);
        }

        if (wallColor == WallColor.White)
        {
            ReplaceBricks(whiteBrick);
        }
        else if (wallColor == WallColor.Grey)
        {
            ReplaceBricks(greyBrick);
        }
    }

    void ReplaceBricks(GameObject brickPrefab)
    {
        if (brickPrefab == null) return;

        for (int i = 0; i < bricks.Count; i++)
        {
            GameObject currentBrick = bricks[i];
            if (currentBrick == null) continue;

            GameObject newBrick = Instantiate(brickPrefab, currentBrick.transform.position, currentBrick.transform.rotation, currentBrick.transform.parent);
            newBrick.transform.localScale = currentBrick.transform.localScale;
            newBrick.name = currentBrick.name;

            bricks[i] = newBrick;
            Destroy(currentBrick);
        }
    }
}
