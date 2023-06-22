using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public ETeam bulletType;

    public PlayerTeams team;
    public ParticlePainter paint;

    void Awake()
    {
        paint = GetComponent<ParticlePainter>();

    }
    private void OnEnable()
    {
        team = GetComponentInParent<PlayerTeams>();
        bulletType = team.team;
    }
}
