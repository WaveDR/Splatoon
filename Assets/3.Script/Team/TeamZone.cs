using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public enum ETeam {Blue, Yellow, Etc, Static}

public class TeamZone : MonoBehaviourPun
{
    public ETeam team;
    private Bullet bullet;
    void Start()
    {
        
    }
    private void OnEnable()
    {
        In_NodeList();
    }
    public void In_NodeList()
    {
        GameManager.Instance.nodes.Add(gameObject.GetComponent<TeamZone>());
    }
    public void ChangeZone(Bullet bullet)
    {
            if (team != ETeam.Static)
                team = bullet.bulletType;
        //�� �Ǻ� ���� �����Ұ�

        if (!GameManager.Instance.isLobby)
            bullet.photonView.RPC("Score_Plus", RpcTarget.AllBuffered);
    }
    private void OnParticleCollision(GameObject other)
    {
        bullet = other.GetComponent<Bullet>();
        ChangeZone(bullet);
    }


}
