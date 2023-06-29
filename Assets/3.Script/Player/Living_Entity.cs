using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Living_Entity : MonoBehaviour, IDamage
{
    [SerializeField] protected PlayerStat player_Stat;

    private float _player_CurHealth;
    public ParticleSystem[] hitEffect;
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
    public event Action onDeath;

    protected virtual void OnEnable()
    {
        isDead = false;
    }
    public virtual void OnDamage(float damage)
    {
            //플레이어의 총알에 맞았을 때 
            //총알에 맞으면 맞은 방향으로 피 튀기기
            player_CurHealth -= damage;

        if(player_CurHealth <= 0 && !isDead)
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
        if(onDeath != null)
        {
            onDeath();
        }


        isDead = true;
    }
}
