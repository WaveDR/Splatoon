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

    [Header("Don't NavMesh Target")]
    public bool isSide;
    private void Awake()
    {
        TryGetComponent(out paint);
    }
    public void In_NodeList()
    {
        //싱글톤으로 선언된 GameManager의 Node list에 Component 추가
        GameManager.Instance.nodes.Add(gameObject.GetComponent<TeamZone>());
    }
    public void ChangeZone(Bullet bullet)
    {
        //Score / Team Change
        if (team != ETeam.Static)
            team = bullet.bulletType;

        if (!GameManager.Instance.isLobby)
            bullet.Score_Plus();
    }

    //총알 Particle 충돌 시 영역 변경
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

    //AI의 자체 Collider와 충돌할 경우 해당 TeamZone의 영역을 변경
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("AI"))
        {
            Enemy_Con ai = collision.gameObject.GetComponent<Enemy_Con>();
            team = ai.Player_Team.team;
        }
    }
}
