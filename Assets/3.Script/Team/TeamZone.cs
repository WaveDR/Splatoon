using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team {Blue, Yellow, Etc}

public class TeamZone : MonoBehaviour
{
    public Team team;
    private Bullet bullet;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    private void OnParticleCollision(GameObject other)
    {
        bullet = other.GetComponent<Bullet>();
        team = bullet.bulletType;
        //팀 판별 로직 구현할것
    }
}
