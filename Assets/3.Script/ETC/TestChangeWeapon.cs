using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestChangeWeapon : MonoBehaviour
{

    PlayerShooter player;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerShooter>();
    }

    public void Change_Brush()
    {
        player.WeaponType = Weapon.Brush;
        player.gameObject.SetActive(false);
        player.gameObject.SetActive(true);
    }
    public void Change_Gun()
    {
        player.WeaponType = Weapon.Gun;
        player.gameObject.SetActive(false);
        player.gameObject.SetActive(true);
    }
    public void Change_Bow()
    {
        player.WeaponType = Weapon.Bow;
        player.gameObject.SetActive(false);
        player.gameObject.SetActive(true);
    }

}
