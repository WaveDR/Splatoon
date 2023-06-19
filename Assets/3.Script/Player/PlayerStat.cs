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
    public int maxHeath;

    [Header("���� �Ÿ�")]
    public float detectRange;

    [Header("�ٴ� ���̾�")]
    public LayerMask floor_Layer;
}
