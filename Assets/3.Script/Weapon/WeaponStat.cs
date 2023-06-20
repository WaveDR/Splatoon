using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WeaponStat : ScriptableObject
{

    [Header("공격 속도")]
    public float fire_Rate;

    [Header("탄환 개수")]
    public int max_Ammo;

    [Header("소비 탄환")]
    public int use_Ammo;
}

