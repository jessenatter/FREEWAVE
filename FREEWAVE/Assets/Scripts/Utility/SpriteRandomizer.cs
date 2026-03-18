using System.Collections.Generic;
using UnityEngine;

public class SpriteRandomizer : MonoBehaviour
{
    [SerializeField]List<Sprite> sprites = new List<Sprite>();
    void Start()
    {
        int i = Random.Range(0,sprites.Count);
        GetComponent<SpriteRenderer>().sprite = sprites[i];
    }
}
