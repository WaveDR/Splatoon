using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Shot_System : MonoBehaviour
{
    public EWeapon weaponType;
    public WeaponStat weapon_Stat;
    public int weapon_MaxAmmo;
    public float weapon_CurAmmo;
    public PlayerShooter player_Shot;
    [SerializeField] private Transform firePoint_Files_Yellow;
    [SerializeField] private Transform firePoint_Files_Blue;
    [SerializeField] private Transform firePoint_Files;

    public Bullet[] firePoint;

    public PlayerTeams team;

    // Start is called before the first frame update
    void Awake()
    {
        player_Shot = GetComponentInParent<PlayerShooter>();
    }

    public void Weapon_Color_Change(ETeam team)
    {
        weapon_MaxAmmo = weapon_Stat.max_Ammo;
        weapon_CurAmmo = weapon_MaxAmmo;

        firePoint_Files_Yellow = transform.GetChild(0);
        firePoint_Files_Blue = transform.GetChild(1);

        firePoint_Files = null;
        firePoint_Files_Yellow.gameObject.SetActive(true);
        firePoint_Files_Blue.gameObject.SetActive(true);


        switch (team) //팀에 따른 물감 색 오브젝트 변환
        {
            case ETeam.Yellow:
                firePoint_Files = firePoint_Files_Yellow;
                firePoint_Files_Blue.gameObject.SetActive(false);
                break;

            case ETeam.Blue:
                firePoint_Files = firePoint_Files_Blue;
                firePoint_Files_Yellow.gameObject.SetActive(false);
                break;
        }

        if (firePoint_Files.childCount != 0)
        {
            firePoint = new Bullet[firePoint_Files.childCount];
        }

        if (firePoint_Files.gameObject.activeSelf)
        {
            for (int i = 0; i < firePoint.Length; i++)
            {
                firePoint[i] = firePoint_Files.GetChild(i).gameObject.GetComponent<Bullet>();
                firePoint[i].dmg = weapon_Stat.weapon_Dmg;
                firePoint[i].Bullet_Set(player_Shot.Player_Con.player_Team);
            }
        }

      
    }

    public void Shot()
    {
        if (weapon_CurAmmo > 0)
        {
            weapon_CurAmmo -= weapon_Stat.use_Ammo;

            foreach (Bullet shot in firePoint)
            {
                shot.particle.Play();
            }
        }
        else
        {
            weapon_CurAmmo = 0;
            return;
        }
    }

}
