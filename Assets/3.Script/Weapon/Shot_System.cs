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

        // 0��° �ڽĿ�����Ʈ�� Yellow Bullet
        firePoint_Files_Yellow = transform.GetChild(0);
        // 1��° �ڽĿ�����Ʈ�� Blue Bullet
        firePoint_Files_Blue = transform.GetChild(1);

        //�� ������Ʈ �� �ϳ��� �����ų �� ��ü �ʱ�ȭ
        firePoint_Files = null;
        firePoint_Files_Yellow.gameObject.SetActive(true);
        firePoint_Files_Blue.gameObject.SetActive(true);


        switch (team) //���� ���� ���� �� ������Ʈ ��ȯ
        {
            case ETeam.Yellow:
                //�� ��ü�� �ش� ���� FirePoint ������Ʈ ����ȭ
                firePoint_Files = firePoint_Files_Yellow;
                //�� �� ��Ȱ��ȭ
                firePoint_Files_Blue.gameObject.SetActive(false);
                break;

            case ETeam.Blue:
                firePoint_Files = firePoint_Files_Blue;
                firePoint_Files_Yellow.gameObject.SetActive(false);
                break;
        }

        if (firePoint_Files.childCount != 0)
        {
            //Brush ���� �߻�ü ���⿡�� Particle�� ������ �� �ִ� Bullet Component�� ��������
            firePoint = new Bullet[firePoint_Files.childCount];
        }

        if (firePoint_Files.gameObject.activeSelf)
        {
            //����� firePoint_File�� Ȱ�������� ��� Stat�� ���� ������ ����ȭ
            for (int i = 0; i < firePoint.Length; i++)
            {
                firePoint[i] = firePoint_Files.GetChild(i).gameObject.GetComponent<Bullet>();
                firePoint[i].dmg = weapon_Stat.weapon_Dmg;
                firePoint[i].Bullet_Set(this.team);
            }
        }
    }

    //������ ���� ����
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
