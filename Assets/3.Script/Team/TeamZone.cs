using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public enum ETeam {Blue, Yellow, Etc, Static}

public class TeamZone : MonoBehaviourPun
{
    public ETeam team;
    private Bullet bullet;
    public PaintTarget paint;

    public bool isSide;
    private void Awake()
    {
        TryGetComponent(out paint);
    }

    private void OnEnable()
    {
        //In_NodeList();
    }
    public void In_NodeList()
    {
        GameManager.Instance.nodes.Add(gameObject.GetComponent<TeamZone>());
    }
    public void ChangeZone(Bullet bullet)
    {
        if (team != ETeam.Static)
            team = bullet.bulletType;
        //팀 판별 로직 구현할것

        if (!GameManager.Instance.isLobby)
            bullet.Score_Plus();
    }
    private void OnParticleCollision(GameObject other)
    {
        if(paint != null)
        {
            if (!paint.enabled)
                paint.enabled = true;
        }

        bullet = other.GetComponent<Bullet>();
        if (bullet.team.team == team) return;
        ChangeZone(bullet);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("AI"))
        {
            Enemy_Con ai = collision.gameObject.GetComponent<Enemy_Con>();
            team = ai.Player_Team.team;
        }
    }
}
