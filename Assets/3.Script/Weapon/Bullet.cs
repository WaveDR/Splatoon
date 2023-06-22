using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public ETeam bulletType;

    public PlayerTeams team;
    public ParticleSystem particle;
    public float dmg;
    void Awake()
    {
        particle = GetComponent<ParticleSystem>();
    }
    private void OnEnable()
    {
        team = GetComponentInParent<PlayerTeams>();
        bulletType = team.team;
    }
}
