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
        if (!player.transform.GetChild(0).gameObject.activeSelf) return;
        player.WeaponType = EWeapon.Brush;
        player.gameObject.SetActive(false);
        player.gameObject.SetActive(true);
    }
    public void Change_Gun()
    {
        if (!player.transform.GetChild(0).gameObject.activeSelf) return;

        player.WeaponType = EWeapon.Gun;
        player.gameObject.SetActive(false);
        player.gameObject.SetActive(true);
    }
    public void Change_Bow()
    {
        if (!player.transform.GetChild(0).gameObject.activeSelf) return;

        player.WeaponType = EWeapon.Bow;
        player.gameObject.SetActive(false);
        player.gameObject.SetActive(true);
    }

    public void Change_Team(bool isYellow)
    {
        if (isYellow)
        {
            player.weapon.team.team = ETeam.Yellow;
        }
        else
        {
            player.weapon.team.team = ETeam.Blue;
        }
        player.gameObject.SetActive(false);
        player.gameObject.SetActive(true);
    }

}
