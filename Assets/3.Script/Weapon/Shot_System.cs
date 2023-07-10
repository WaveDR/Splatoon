using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shot_System : MonoBehaviourPun
{

    [Header("Player Component")]
    [Header("============================================")]
    public EWeapon weaponType;
    public WeaponStat weapon_Stat;
    public PlayerShooter player_Shot;
    public PlayerTeams team;

    [Header("Player Ammo")]
    [Header("============================================")]
    public int weapon_MaxAmmo;
    public float weapon_CurAmmo;

    [Header("Weapon FirePoint")]
    [Header("============================================")]
    [SerializeField] private Transform firePoint_Files_Yellow;
    [SerializeField] private Transform firePoint_Files_Blue;
    [SerializeField] private Transform firePoint_Files;
    public Bullet[] firePoint;



    // Start is called before the first frame update
    void Awake()
    {
        player_Shot = GetComponentInParent<PlayerShooter>();
    }

    public void Weapon_Color_Change(ETeam team)
    {
        this.team = GetComponentInParent<PlayerTeams>();
        weapon_MaxAmmo = weapon_Stat.max_Ammo;
        weapon_CurAmmo = weapon_MaxAmmo;

        // 0번째 자식오브젝트는 Yellow Bullet
        firePoint_Files_Yellow = transform.GetChild(0);
        // 1번째 자식오브젝트는 Blue Bullet
        firePoint_Files_Blue = transform.GetChild(1);

        //위 오브젝트 중 하나를 적용시킬 빈 객체 초기화
        firePoint_Files = null;
        firePoint_Files_Yellow.gameObject.SetActive(true);
        firePoint_Files_Blue.gameObject.SetActive(true);


        switch (team) //팀에 따른 물감 색 오브젝트 변환
        {
            case ETeam.Yellow:
                //빈 객체에 해당 팀의 FirePoint 오브젝트 동기화
                firePoint_Files = firePoint_Files_Yellow;
                //그 외 비활성화
                firePoint_Files_Blue.gameObject.SetActive(false);
                break;

            case ETeam.Blue:
                firePoint_Files = firePoint_Files_Blue;
                firePoint_Files_Yellow.gameObject.SetActive(false);
                break;
        }

        if (firePoint_Files.childCount != 0)
        {
            //Brush 제외 발사체 무기에서 Particle을 실행할 수 있는 Bullet Component를 가져오기
            firePoint = new Bullet[firePoint_Files.childCount];
        }

        if (firePoint_Files.gameObject.activeSelf)
        {
            //적용된 firePoint_File이 활성상태일 경우 Stat의 무기 정보를 동기화
            for (int i = 0; i < firePoint.Length; i++)
            {
                firePoint[i] = firePoint_Files.GetChild(i).gameObject.GetComponent<Bullet>();
                firePoint[i].dmg = weapon_Stat.weapon_Dmg;
                firePoint[i].Bullet_Set(this.team);
            }
        }
    }

    //실질적 공격 로직
    public void Shot()
    {
        if (weapon_CurAmmo > 0)
        {
                Attack();
        }
        else
        {
            weapon_CurAmmo = 0;
            return;
        }
    }
    public void Attack()
    {
        UseAmmo();
        foreach (Bullet shot in firePoint)
        {
            shot.Paint_Play();
        }
    }

    [PunRPC]
    public void UseAmmo()
    {
        if(player_Shot.Player_Con._enemy == null)
        weapon_CurAmmo -= weapon_Stat.use_Ammo;
        else
            weapon_CurAmmo -= weapon_Stat.use_Ammo / 2;
    }
}
