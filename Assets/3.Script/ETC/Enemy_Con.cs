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

    //���� ����� ������Ƽ
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
            //��ü ��� ���� �ν�
            if (!player_Con.isDead)
            {
                if (player_Shot.weapon.weapon_CurAmmo > 0)
                {
                    //��Ȱ���� �� ����
                    if (player_Shot.weapon.weapon_CurAmmo > 90) Reset_AI();

                    //���� �� ���� ����
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
                //�̵� ��ǥ �ν� �� �����ϱ� ���� �ڵ� ���κп� ��ġ
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

    //�� �� ���� ����
    private void OnTriggerEnter(Collider other)
    {
        if (!player_Con.isStop)
        {
            if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("AI"))
            {
                PlayerController player = other.GetComponent<PlayerController>();

                if (player != null)
                {
                    //�ֿ켱 ��ǥ ����
                    if (player.player_Team.team != player_Team.team)
                        target = player;
                }
            }

            else if (other.gameObject.CompareTag("Wall"))
            {
                //�� / �� �� ���� ����
                TeamZone team = other.GetComponent<TeamZone>();

                //����ó�� AI�� �� �� ���� ������ Returns
                if (team == null) return;
                if (team.team == ETeam.Static || team.isSide) return;
                //�Ʊ��� �ƴ� ���Ǹ� List�� �߰� (�߰��� �Ŀ� ������ ����� ���ǵ��� Update���� ����)
                if (team.team != player_Team.team)
                    zone.Add(team);
            }
        }
    }

    //�ν� ���� �� ����
    private void OnTriggerExit(Collider other)
    {
        //�νĹ��� �ٱ����� �Ѿ �� ��Ͽ��� ����

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
                //AI�� �������� ����ȭ�ϱ� ���� ����
                if (zone.Count <= 10) return;
                TeamZone team = other.GetComponent<TeamZone>();
                if (team.team != player_Team.team && zone.Contains(team))
                    zone.Remove(team);
                else return;
            }
        }
    }


    //============================================        �� �ݹ� �޼���   |  �Ϲ� �޼��� ��        ========================================================

    private void Attack_Target()
    {
        //Ÿ���� �����Ǿ��� ��
        if (target != null)
        {
            //�� ��� �� �����ϸ� Ÿ�ٿ��� �̵�
            if (!target.isDead)
            {
                target_Pos = target.transform;
                player_Input.fire = true;
            }

            //��� �� �ʱ�ȭ
            else
            {
                target = null;
                player_Input.fire = false;
            }
        }
        //�������� �ʾ��� ��
        else
        {
            //Trigger�� ������ �� ������ �̵��ϸ鼭 ����
            if (zone[0].team != player_Team.team)
            {
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, player_Stat.detect_Range, player_Stat.floor_Layer))
                {
                    GameObject raycast_Object = hit.collider.gameObject;
                    TeamZone target_Zone = raycast_Object.GetComponent<TeamZone>();
                    // Raycast�� �߻��Ͽ� �߹� ���� �Ǵ� �� ����

                    if (zone[0] == target_Zone)
                    {
                        //��ǥ���� �̵� �Ϸ� �� ���� ��ǥ�� �̵���ȯ
                        zone.RemoveAt(0);
                        target_Pos = zone[0].transform;

                        //������ ���� ����
                        if (isNull && zone[0].team == player_Team.team)
                        {
                            team_Floor = target_Zone;
                        }
                    }
                    else
                    {
                        //��ǥ �������� �����ϸ� �̵�
                        player_Input.fire = true;
                        target_Pos = zone[0].transform;
                    }
                }
            }

            //Target Zone List���� �Ʊ� ���� ����
            else
            {
                zone.RemoveAt(0);
            }
        }
    }

    //AI �̵� �ʱ�ȭ����
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

        //��ǥ�����ν�
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

        //Zone List���� �ߺ��Ǵ� ��� ����
        zone = zone.Distinct().ToList();
    }
}
