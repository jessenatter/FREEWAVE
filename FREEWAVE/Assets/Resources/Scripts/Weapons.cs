using UnityEngine;

public class WeaponPickUpBehavior : InteractableBehavior
{
    public Weapon weapon;

    public WeaponPickUpBehavior(Weapon _weapon)
    {
        weapon = _weapon;
    }

    public override void Interact()
    {
        base.Interact();
    }
}

public class Weapon
{
    protected string name;
    protected GameObject gameObject;
    protected SpriteRenderer sr;
    protected WeaponPickUpBehavior weaponPickUpBehavior;

    virtual public void Start()
    {
        gameObject = new GameObject(name);
        sr = gameObject.AddComponent<SpriteRenderer>();
        weaponPickUpBehavior = new WeaponPickUpBehavior(this);
    }

    virtual public void Update()
    {

    }

    public void OnPickup()
    {

    }
}

public class MeeleWeapon : Weapon
{
    protected Sprite[] meleeWeapons;
    public override void Start()
    {
        base.Start();
        meleeWeapons = Resources.LoadAll<Sprite>("Sprites/Weapons/MeleeWeaopons");
    }
}

public class Knife : MeeleWeapon
{
    GameObject blade;
    SpriteRenderer bladeSr;

    public override void Start()
    {
        name = "Knife";
        base.Start();
        blade = new GameObject("Blade");
        
        bladeSr = blade.AddComponent<SpriteRenderer>();
        bladeSr.sortingOrder = sr.sortingOrder - 1;

        sr.sprite = meleeWeapons[3];
        bladeSr.sprite = meleeWeapons[1];
        blade.transform.position = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y + (sr.size.y / 2));
        blade.transform.SetParent(gameObject.transform);
    }
}
