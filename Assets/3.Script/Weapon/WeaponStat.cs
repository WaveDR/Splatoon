using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WeaponStat : ScriptableObject
{

    [Header("���� �ӵ�")]
    public float fire_Rate;

    [Header("źȯ ����")]
    public int max_Ammo;

    [Header("�Һ� źȯ")]
    public int use_Ammo;
}

