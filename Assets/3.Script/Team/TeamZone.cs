using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ETeam {Blue, Yellow, Etc, Static}

[System.Serializable]
public class TeamZone : MonoBehaviour
{
    public ETeam team;
    private Bullet bullet;

    void Start()
    {
        
    }
    private void OnEnable()
    {
        ScoreManager.Instance.nodes.Add(gameObject.GetComponent<TeamZone>());
    }

    void Update()
    {
       
    }
    private void OnParticleCollision(GameObject other)
    {
        bullet = other.GetComponent<Bullet>();
        if(team != ETeam.Static)
        team = bullet.bulletType;
        //팀 판별 로직 구현할것

        if(team != bullet.team.team)
        bullet.player_Shot.player_Score += 1;
    }
}
