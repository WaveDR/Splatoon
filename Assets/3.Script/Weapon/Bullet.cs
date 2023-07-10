using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourPun
{

    [Header("Player Component")]
    [Header("============================================")]
    public ETeam bulletType;
    public PlayerTeams team;
    public PlayerShooter player_Shot;
    public PlayerController[] players;

    [Header("Bullet Particle")]
    [Header("============================================")]
    public ParticleSystem particle;
    public ParticleSystem[] color;

    [Header("Weapon ETC")]
    [Header("============================================")]
    public float dmg;
    public bool brush;
    public bool deathEffect;
    void Awake()
    {
        particle = GetComponent<ParticleSystem>();
        players = FindObjectsOfType<PlayerController>();
        color = GetComponentsInChildren<ParticleSystem>();
        player_Shot = GetComponentInParent<PlayerShooter>();
    }

    public void Bullet_Set(PlayerTeams team)
    {
        //�Ѿ� Team �� Color ����

        this.team = team;
        if (deathEffect)
        {
            //��� ����Ʈ�� ��� ������ ���ݿ� ���� ����Ǳ⿡ �ݴ�Ǵ� Team���� ����
            if (team.team == ETeam.Blue) bulletType = ETeam.Yellow;
            else bulletType = ETeam.Blue;
        }
        else
            bulletType = team.team;


        //����� ETeam�� ���� Color ����
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
    public void Paint_Play()
    {
        //���� �� Particle ���
        particle.Play();
    }
    
    public void Score_Plus()
    {
        player_Shot.player_Score++;
    }

    public void Player_Kill(string name)
    {
        player_Shot.KillLog(name);
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
    }
}
