using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ETeam {Blue, Yellow, Etc, Static}

public class TeamZone : MonoBehaviour
{
    public ETeam team;
    private Bullet bullet;

    void Start()
    {
        
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
    }
}
