using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot_System : MonoBehaviour
{
    public EWeapon weaponType;
    public WeaponStat weapon_Stat;
    public int weapon_MaxAmmo;
    public float weapon_CurAmmo;

    [SerializeField] private Transform firePoint_Files_Yellow;
    [SerializeField] private Transform firePoint_Files_Blue;
    [SerializeField] private Transform firePoint_Files;

    public ParticleSystem[] firePoint;

    public PlayerTeams team;

  // Start is called before the first frame update
    void Awake()
    {
        team = GetComponentInParent<PlayerTeams>();
        weapon_MaxAmmo = weapon_Stat.max_Ammo;
        weapon_CurAmmo = weapon_MaxAmmo;

        firePoint_Files_Yellow = transform.GetChild(0);
        firePoint_Files_Blue = transform.GetChild(1);
    }

    // Update is called once per frame
    void OnEnable()
    {
        Weapon_Color_Change();
    }

    private void Weapon_Color_Change()
    {
        firePoint_Files = null;
        firePoint_Files_Yellow.gameObject.SetActive(true);
        firePoint_Files_Blue.gameObject.SetActive(true);

        switch (team.team) //팀에 따른 물감 색 오브젝트 변환
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

        firePoint = new ParticleSystem[firePoint_Files.childCount];

        for (int i = 0; i < firePoint.Length; i++)
        {
            firePoint[i] = firePoint_Files.GetChild(i).gameObject.GetComponent<ParticleSystem>();
        }
    }
    public void Shot()
    {
        if(weapon_CurAmmo > 0)
        {
            foreach (ParticleSystem shot in firePoint)
            {
                shot.Play();
            }
            weapon_CurAmmo -= weapon_Stat.use_Ammo;
        }
        else
        {
            Debug.Log("총알이 없습니다!");
            weapon_CurAmmo = 0;
            return;
            //나중에 인게임 연출도 해줄것
        }
    }
}
