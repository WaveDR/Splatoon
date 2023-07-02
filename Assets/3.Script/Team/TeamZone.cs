using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public enum ETeam {Blue, Yellow, Etc, Static}

public class TeamZone : MonoBehaviourPun,IPunObservable
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
            //팀 판별 로직 구현할것

            if (!GameManager.Instance.isLobby)
                bullet.player_Shot.player_Score++;
           
    }
    private void OnParticleCollision(GameObject other)
    {
        bullet = other.GetComponent<Bullet>();
        ChangeZone(bullet);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(team);
        }
        else
        {
            team = (ETeam) stream.ReceiveNext();
        }
    }
}
