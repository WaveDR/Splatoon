using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public ETeam bulletType;

    public PlayerController player;
    public ParticlePainter paint;
    // Start is called before the first frame update
    void Awake()
    {
        player = GetComponentInParent<PlayerController>();
        bulletType = player.player_Team;
    }
}
