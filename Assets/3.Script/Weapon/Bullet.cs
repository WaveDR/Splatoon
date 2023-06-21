using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Team bulletType;

    public PlayerController player;
    // Start is called before the first frame update
    void Awake()
    {
        player = GetComponentInParent<PlayerController>();
        bulletType = player.player_Team;
    }
}
