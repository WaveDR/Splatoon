using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class PlayerStat : ScriptableObject
{
    [Header("�̵�")]
    public float moveZone_Speed;
    public float dashSpeed;
    public float enemyZone_Speed;

    [Header("�⺻ ü��")]
    public int max_Heath;

    [Header("���� �Ÿ�")]
    public float detect_Range;
    
    [Header("�ٴ� ���̾�")]
    public LayerMask floor_Layer;
}

