using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Con : MonoBehaviour
{
    PlayerController player_Con;
    PlayerShooter player_Shot;
    PlayerInput player_input;
    PlayerTeams player_Team;

    public NavMeshAgent nav;

    public CircleCollider2D circle;

    public LayerMask layer;
    public GameObject target;
    // Start is called before the first frame update
    void Awake()
    {
        TryGetComponent(out player_Con);
        TryGetComponent(out player_Shot);
        TryGetComponent(out player_input);
        TryGetComponent(out player_Team);
        TryGetComponent(out nav);
    }

    // Update is called once per frame
    void Update()
    {
        //transform.Translate()
        
        
    }

    
}
