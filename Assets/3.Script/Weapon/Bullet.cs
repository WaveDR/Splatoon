using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourPun
{
    public ETeam bulletType;

    public PlayerTeams team;
    public PlayerShooter player_Shot;
    public ParticleSystem particle;
    public ParticleSystem[] color;
    public float dmg;
    public bool brush;
    public bool deathEffect;
    public PlayerController[] players;
    void Awake()
    {
        particle = GetComponent<ParticleSystem>();
        players = FindObjectsOfType<PlayerController>();
        color = GetComponentsInChildren<ParticleSystem>();
    }

    private void OnEnable()
    {
        if (!photonView.IsMine) return;
        player_Shot = GetComponentInParent<PlayerShooter>();
        team = GetComponentInParent<PlayerTeams>();

        if(deathEffect)
        {
            if (team.team == ETeam.Blue) bulletType = ETeam.Yellow;
            else bulletType = ETeam.Blue;
        }
        else
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

        if (color != null)
        {
            foreach (ParticleSystem par in color)
            {
                var bullet = par.main;
                var main = par.main;
                if (bulletType == ETeam.Blue)
                {
                    bullet.startColor = (Color)team.team_Blue;
                    main.startColor = (Color)team.team_Blue;
                }
                else
                {
                    bullet.startColor = (Color)team.team_Yellow;
                    main.startColor = (Color)team.team_Yellow;
                }
            }
        }
    }
    [PunRPC]
    public void Score_Plus()
    {
        player_Shot.player_Score++;
    }

    public void Player_Kill(string name)
    {
        StartCoroutine(player_Shot.KillLog(name));
        player_Shot.Player_Con.ES_Manager.Play_SoundEffect("Player_Kill");
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

        //if(enemy != null)
        //enemy.OnDamage(dmg);
    }
}
