using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team {Blue, Yellow}

public class TeamZone : MonoBehaviour
{
    public Team team;
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
        Debug.Log("물감 충돌" + other.name);

        //팀 판별 로직 구현할것
    }
}
