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
        //�̱������� ����� GameManager�� Node list�� Component �߰�
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

    //�Ѿ� Particle �浹 �� ���� ����
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

    //AI�� ��ü Collider�� �浹�� ��� �ش� TeamZone�� ������ ����
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("AI"))
        {
            Enemy_Con ai = collision.gameObject.GetComponent<Enemy_Con>();
            team = ai.Player_Team.team;
        }
    }
}
