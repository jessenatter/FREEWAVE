using UnityEngine;

[System.Serializable]
public class Weapon
{
    public GameObject gameObject;
    public bool unlocked;
    public float damage;
    public float attackDuration;
    public float knockbackForce;

    public Weapon(GameObject gameObject, bool unlocked, float damage, float attackDuration,float knockbackForce)
    {
        this.gameObject = gameObject;
        this.unlocked = unlocked;
        this.damage = damage;
        this.attackDuration = attackDuration;
        this.knockbackForce = knockbackForce;
    }
}