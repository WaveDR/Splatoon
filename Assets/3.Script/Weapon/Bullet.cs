using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public ETeam bulletType;

    public PlayerTeams team;
    public ParticleSystem particle;
    public float dmg;
    public bool brush;
    public PlayerController[] players;
    void Awake()
    {
        particle = GetComponent<ParticleSystem>();
        players = FindObjectsOfType<PlayerController>();
    }

    private void OnEnable()
    {
    
        team = GetComponentInParent<PlayerTeams>();
        bulletType = team.team;

        if (!brush)
        {
            for (int i = 0; i < particle.trigger.colliderCount; i++)
            {
                particle.trigger.RemoveCollider(i);
            }


            for (int i = 0; i < players.Length; i++)
            {
                particle.trigger.AddCollider(players[i]);
            }
        }
    }
    private void OnDisable()
    {
        if (!brush)
        {
            for (int i = 0; i < particle.trigger.colliderCount; i++)
            {
                particle.trigger.RemoveCollider(i);
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        PlayerController enemy = other.GetComponent<PlayerController>();

        if(enemy != null)
        enemy.OnDamage(dmg);
    }
}
