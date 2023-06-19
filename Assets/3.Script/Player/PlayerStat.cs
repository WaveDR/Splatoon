using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class PlayerStat : ScriptableObject
{
    [Header("이동")]
    public float moveZone_Speed;
    public float dashSpeed;
    public float enemyZone_Speed;

    [Header("기본 체력")]
    public int maxHeath;

    [Header("감지 거리")]
    public float detectRange;

    [Header("바닥 레이어")]
    public LayerMask floor_Layer;
}
