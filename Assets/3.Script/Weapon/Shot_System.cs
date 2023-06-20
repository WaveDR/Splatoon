using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot_System : MonoBehaviour
{
    public Weapon weaponType;
    public WeaponStat weapon_Stat;
    public int weapon_MaxAmmo;
    public float weapon_CurAmmo;

    [SerializeField] private Transform firePoint_File;
    [SerializeField] private ParticleSystem[] firePoint;


  // Start is called before the first frame update
    void Awake()
    {
        firePoint_File = transform.GetChild(0);
        firePoint = new ParticleSystem[firePoint_File.childCount];

        weapon_MaxAmmo = weapon_Stat.max_Ammo;
        weapon_CurAmmo = weapon_MaxAmmo;

        for (int i = 0; i < firePoint.Length; i++)
        {
            firePoint[i] = firePoint_File.GetChild(i).gameObject.GetComponent<ParticleSystem>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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
