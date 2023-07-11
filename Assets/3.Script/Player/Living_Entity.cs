using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Living_Entity : MonoBehaviourPun, IDamage
{
    [Header("Player Data")]
    [SerializeField] protected PlayerStat player_Stat;

    [Header("Player Hp")]
    public ParticleSystem[] hitEffect;
    private float _player_CurHealth;
    public float player_CurHealth
    {
        get { return _player_CurHealth; }
        set
        {
            _player_CurHealth = value;
            _player_CurHealth = Mathf.Clamp(_player_CurHealth, 0, player_Stat.max_Heath);
        }
    }

    protected float recovery_Speed = 5;
    protected bool isFalling;
    public bool isDead;
    public bool isStop;

    protected virtual void OnEnable()
    {
        isDead = false;
    }

    public void ApplyUpdate_HP(float newHP, bool newDead)
    {
        _player_CurHealth = newHP;
        isDead = newDead;
    }

    public virtual void OnDamage(float damage)
    {
        //플레이어의 총알에 맞았을 때 
        //총알에 맞으면 맞은 방향으로 피 튀기기

        player_CurHealth -= damage;

        if (player_CurHealth <= 0 && !isDead)
            {
                Player_Die();
            }
    }

    public virtual void RestoreHp(float newHealth)
    {
        if (isDead) return;
            player_CurHealth += newHealth * Time.deltaTime;
    }

    public virtual void Player_Die()
    {
        isDead = true;
    }

    public virtual void Respawn(ETeam team, bool falling)
    {
    }
}
