using System.Collections.Generic;
using UnityEngine;

public class InitialWeaponUnlocks : MonoBehaviour
{
    [SerializeField] private List<string> unlockedWeaponNames = new List<string>();

    private Player player;

    private void Awake()
    {
        player = FindObjectOfType<Player>();
    }

    private System.Collections.IEnumerator Start()
    {
        yield return null;

        if (player == null)
            player = FindObjectOfType<Player>();

        if (player == null)
        {
            Debug.LogWarning("[InitialWeaponUnlocks] Player not found. Unlocks were not applied.");
            yield break;
        }

        foreach (string weaponName in unlockedWeaponNames)
        {
            if (string.IsNullOrWhiteSpace(weaponName))
                continue;

            player.UnlockWeaponByName(weaponName.Trim().ToLowerInvariant());
        }
    }
}
