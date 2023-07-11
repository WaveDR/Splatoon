using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

public class Enemy_Con : MonoBehaviour
{

    //GetComponent
    [SerializeField] private PlayerStat player_Stat;
    private PlayerController player_Con;
    private PlayerShooter player_Shot;
    private PlayerInput player_Input;
    private PlayerTeams player_Team;
    private Animator player_Anim;
    public NavMeshAgent ai_nav;

    //발판 변경용 프로퍼티
    public PlayerTeams Player_Team => player_Team;
    public bool isNull;


    [Header("Player Target")]
    public PlayerController target;

    [Header("Target Position")]
    public Transform target_Pos;

    [Header("Reload Position")]
    public TeamZone team_Floor;
    [Header("Target Floor")]
    public List<TeamZone> zone = new List<TeamZone>();
    // Start is called before the first frame update
    void Awake()
    {
        TryGetComponent(out player_Con);
        TryGetComponent(out player_Shot);
        TryGetComponent(out player_Input);
        TryGetComponent(out player_Team);
        TryGetComponent(out player_Anim);
        TryGetComponent(out ai_nav);
        player_Con.isStop = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!player_Con.isStop)
        {
            //객체 사망 상태 인식
            if (!player_Con.isDead)
            {
                if (player_Shot.weapon.weapon_CurAmmo > 0)
                {
                    //부활했을 때 실행
                    if (player_Shot.weapon.weapon_CurAmmo > 90) Reset_AI();

                    //공격 및 점령 로직
                    Attack_Target();
                }

                else if (player_Shot.weapon.weapon_CurAmmo < 0)
                {
                    player_Input.fire = false;

                    if (team_Floor != null)
                        target_Pos = team_Floor.transform;

                    if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, player_Stat.detect_Range, player_Stat.floor_Layer))
                    {
                        GameObject raycast_Object = hit.collider.gameObject;

                        TeamZone teamZone = raycast_Object.GetComponent<TeamZone>();

                        if (team_Floor == teamZone)
                        {
                            team_Floor = null;
                            ai_nav.isStopped = true;
                            isNull = false;
                        }
                    }
                }
                //이동 목표 인식 후 적용하기 위해 코드 끝부분에 위치
                AI_Anim();
            }
            else
            {
                target = null;
                ai_nav.isStopped = true;
                player_Input.fire = false;
            }

        }
    }

    //적 및 발판 갱신
    private void OnTriggerEnter(Collider other)
    {
        if (!player_Con.isStop)
        {
            if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("AI"))
            {
                PlayerController player = other.GetComponent<PlayerController>();

                if (player != null)
                {
                    //최우선 목표 갱신
                    if (player.player_Team.team != player_Team.team)
                        target = player;
                }
            }

            else if (other.gameObject.CompareTag("Wall"))
            {
                //적 / 그 외 발판 감지
                TeamZone team = other.GetComponent<TeamZone>();

                //예외처리 AI가 갈 수 없는 발판은 Returns
                if (team == null) return;
                if (team.team == ETeam.Static || team.isSide) return;
                //아군이 아닌 발판만 List에 추가 (추가된 후에 진영이 변경된 발판들은 Update에서 삭제)
                if (team.team != player_Team.team)
                    zone.Add(team);
            }
        }
    }

    //인식 범위 외 삭제
    private void OnTriggerExit(Collider other)
    {
        //인식범위 바깥으로 넘어갈 시 목록에서 제거

        if (!player_Con.isStop)
        {
            if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("AI"))
            {
                PlayerController player = other.GetComponent<PlayerController>();

                if (player != null)
                {
                    if (player.player_Team.team != player_Team.team)
                    {
                        target = null;
                        player_Input.fire = false;
                    }
                }
            }
            if (other.gameObject.CompareTag("Wall"))
            {
                //AI의 정보량을 최적화하기 위한 로직
                if (zone.Count <= 10) return;
                TeamZone team = other.GetComponent<TeamZone>();
                if (team.team != player_Team.team && zone.Contains(team))
                    zone.Remove(team);
                else return;
            }
        }
    }


    //============================================        ↑ 콜백 메서드   |  일반 메서드 ↓        ========================================================

    private void Attack_Target()
    {
        //타겟이 감지되었을 때
        if (target != null)
        {
            //미 사살 시 공격하며 타겟에게 이동
            if (!target.isDead)
            {
                target_Pos = target.transform;
                player_Input.fire = true;
            }

            //사살 시 초기화
            else
            {
                target = null;
                player_Input.fire = false;
            }
        }
        //감지되지 않았을 때
        else
        {
            //Trigger로 감지된 적 발판을 이동하면서 점령
            if (zone[0].team != player_Team.team)
            {
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, player_Stat.detect_Range, player_Stat.floor_Layer))
                {
                    GameObject raycast_Object = hit.collider.gameObject;
                    TeamZone target_Zone = raycast_Object.GetComponent<TeamZone>();
                    // Raycast를 발사하여 발밑 발판 판단 및 감지

                    if (zone[0] == target_Zone)
                    {
                        //목표지점 이동 완료 시 다음 목표로 이동전환
                        zone.RemoveAt(0);
                        target_Pos = zone[0].transform;

                        //재장전 발판 갱신
                        if (isNull && zone[0].team == player_Team.team)
                        {
                            team_Floor = target_Zone;
                        }
                    }
                    else
                    {
                        //목표 지점까지 점령하며 이동
                        player_Input.fire = true;
                        target_Pos = zone[0].transform;
                    }
                }
            }

            //Target Zone List에서 아군 발판 제거
            else
            {
                zone.RemoveAt(0);
            }
        }
    }

    //AI 이동 초기화로직
    private void Reset_AI()
    {
        ai_nav.isStopped = false;

        if (!isNull)
        {
            team_Floor = null;
            isNull = true;
        }
    }
 
    void AI_Anim()
    {
        Vector3 player_Pos = transform.position;

        //목표지점인식
        ai_nav.destination = target_Pos.position;

        if (!ai_nav.isStopped)
        {
            player_Anim.SetFloat(player_Input.Move_Hor_S, player_Pos.x / player_Pos.x);
            player_Anim.SetFloat(player_Input.Move_Ver_S, player_Pos.z / player_Pos.z);
        }
        else
        {
            player_Anim.SetFloat(player_Input.Move_Hor_S, 0);
            player_Anim.SetFloat(player_Input.Move_Ver_S, 0);
        }

        //Zone List에서 중복되는 요소 제거
        zone = zone.Distinct().ToList();
    }
}
