using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Living_Entity : MonoBehaviour, IDamage
{
    [SerializeField] protected PlayerStat player_Stat;

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
    public bool isDead;

    public event Action onDeath;

    protected virtual void OnEnable()
    {
        isDead = false;
    }
    public virtual void OnDamage(float damage)
    {
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
