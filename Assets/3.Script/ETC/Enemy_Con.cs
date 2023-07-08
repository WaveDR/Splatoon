using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

public class Enemy_Con : MonoBehaviour
{
    PlayerController player_Con;
    PlayerShooter player_Shot;
    PlayerInput player_Input;
    PlayerTeams player_Team;
    public PlayerTeams Player_Team => player_Team;
    Animator player_Anim;
    [SerializeField] private PlayerStat player_Stat;

    public NavMeshAgent nav;

    public PlayerController target;
    public Transform target_Pos;
    public TeamZone team_Floor;

    public bool isNull;

    public List<TeamZone> zone = new List<TeamZone>();
    // Start is called before the first frame update
    void Awake()
    {
        TryGetComponent(out player_Con);
        TryGetComponent(out player_Shot);
        TryGetComponent(out player_Input);
        TryGetComponent(out player_Team);
        TryGetComponent(out player_Anim);
        TryGetComponent(out nav);
        player_Con.isStop = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!player_Con.isStop)
        {
            if (player_Shot.weapon.weapon_CurAmmo > 90)
            {
                nav.isStopped = false;

                if (!isNull)
                {
                    team_Floor = null;
                    isNull = true;
                }

                if (!player_Con.isDead)
                {
                    nav.isStopped = false;

                    if (target != null)
                    {

                        if (!target.isDead)
                        {
                            target_Pos = target.transform;
                            player_Input.fire = true;
                        }

                        else
                        {
                            target = null;
                            player_Input.fire = false;
                        }
                    }
                    else
                    {
                        if (zone[0].team != player_Team.team)
                        {
                            Debug.DrawRay(transform.position, Vector3.down * player_Stat.detect_Range, Color.green);

                            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, player_Stat.detect_Range, player_Stat.floor_Layer))
                            {
                                GameObject raycast_Object = hit.collider.gameObject;

                                TeamZone teamZone = raycast_Object.GetComponent<TeamZone>();

                                if (zone[0] == teamZone)
                                {
                                    target_Pos = zone[0].transform;
                                    zone.RemoveAt(0);

                                    if (isNull && zone[0].team == player_Team.team)
                                    {
                                        team_Floor = teamZone;
                                    }
                                }
                                else
                                {
                                    player_Input.fire = true;
                                    target_Pos = zone[0].transform;
                                }
                            }
                        }

                        else
                        {
                            zone.RemoveAt(0);
                        }
                    }
                }
                else
                {
                    target = null;
                    nav.isStopped = true;
                }
            }
            else if (player_Shot.weapon.weapon_CurAmmo < 0)
            {
                player_Input.fire = false;
                if (!player_Con.isDead)
                {
                    if (team_Floor != null)
                        target_Pos = team_Floor.transform;


                    Debug.DrawRay(transform.position, Vector3.down * player_Stat.detect_Range, Color.green);
                    if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, player_Stat.detect_Range, player_Stat.floor_Layer))
                    {
                        GameObject raycast_Object = hit.collider.gameObject;

                        TeamZone teamZone = raycast_Object.GetComponent<TeamZone>();

                        if (team_Floor == teamZone)
                        {
                            team_Floor = null;
                            nav.isStopped = true;
                            isNull = false;

                        }
                    }
                    
                }
                else
                {
                    target = null;
                    nav.isStopped = true;
                }
            }
            AI_Anim();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!player_Con.isStop)
        {
            TeamZone team = other.GetComponent<TeamZone>();
            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                if (player.player_Team.team != player_Team.team)
                    target = player;
            }

            else
            {
                if (team == null) return;
                if (team.team == ETeam.Static || team.isSide) return;

                if (team.team != player_Team.team)
                    zone.Add(team);
                // target_Pos.position = team.transform.position;
            }
        }
    }
    void AI_Anim()
    {
        Vector3 player_Pos = transform.position;

        nav.destination = target_Pos.position;

        player_Anim.SetFloat(player_Input.Move_Hor_S, player_Pos.x / player_Pos.x);
        player_Anim.SetFloat(player_Input.Move_Ver_S, player_Pos.z / player_Pos.z);

        zone = zone.Distinct().ToList();
    }
}
