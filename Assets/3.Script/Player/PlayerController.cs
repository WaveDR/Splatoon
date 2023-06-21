using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IPlayer
{
    [Header("Player Stat")]
    public Team player_Team;
    [SerializeField] private PlayerStat player_Stat;

    [Header("Player Move")]
    [SerializeField] private GameObject raycast_Object;
    [SerializeField] private ParticleSystem[] player_Wave;

    [Header("Squidform")]
    [SerializeField] private GameObject[] human_Object;
    [SerializeField] private GameObject squid_Object;



    private PlayerInput _player_Input;
    private Rigidbody _player_rigid;
    private PlayerShooter _player_shot;
    private Animator _player_Anim;

    public bool _isJump;
    private bool _isSquid;

    private int _player_CurHp;
    private float _player_Speed;


    // Start is called before the first frame update
    void Awake()
    {
        TryGetComponent(out _player_Input);
        TryGetComponent(out _player_rigid);
        TryGetComponent(out _player_Anim);
        TryGetComponent(out _player_shot);
        Player_StatReset();

        foreach (ParticleSystem start in player_Wave) start.Stop();
    }
    private void FixedUpdate()
    {
        _player_rigid.MovePosition(_player_rigid.position + _player_Input.move_Vec * _player_Speed * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        Player_Movement();
        Player_Jump();
        RaycastFloor(_player_Input.squid_Form);
        Player_Animation();
    }

    private void Player_Animation()
    {
        _player_Anim.SetFloat(_player_Input.Move_Hor_S, _player_Input.Move_Hor);
        _player_Anim.SetFloat(_player_Input.Move_Ver_S, _player_Input.Move_Ver);
        _player_Anim.SetBool("isDown", !_isJump);
    }

    private void Player_Movement()
    {
        if (_player_Input.move_Vec != Vector3.zero)
        {
            transform.Translate(_player_Input.move_Vec * _player_Speed * Time.deltaTime);
        }
        Squid_Eular();
    }


    private void Player_StatReset()
    {
        _player_CurHp = player_Stat.max_Heath;
        _player_Speed = player_Stat.moveZone_Speed;
    }

    private void Player_Jump()
    {
        if (_player_Input.jDown && !_isJump)
        {
            _player_Anim.SetTrigger("isJump");
            //_isJump = true;
            _player_rigid.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }
    }
    private void Squid_Eular()
    {
        if (squid_Object.activeSelf)
        {
            if(_player_Input.squid_FinalRot > 0)
                squid_Object.transform.localEulerAngles = new Vector3(0, _player_Input.player_SquidRot += 900 * Time.deltaTime, 0);
            if (_player_Input.squid_FinalRot < 0)
                squid_Object.transform.localEulerAngles = new Vector3(0, _player_Input.player_SquidRot -= 900 * Time.deltaTime, 0);
        }
    }

    #region Player Raycast

    private void RaycastFloor(bool SquidForm)
    {
        Debug.DrawRay(transform.position, Vector3.down * player_Stat.detect_Range, Color.green); ;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, player_Stat.detect_Range, player_Stat.floor_Layer))
        {
            raycast_Object = hit.collider.gameObject;


            TeamZone teamZone = raycast_Object.GetComponent<TeamZone>();
            Debug.Log(teamZone.team);

            if (teamZone.team == player_Team || teamZone.team != player_Team)
            {
                _isJump = false;

            }

            if (teamZone.team == player_Team) // 내 진영과 현재 바닥 진영이 같을 때
            {
                if (_player_Input.squid_Form)// 오징어 형태
                {
                    Transform_Stat(30, player_Stat.dashSpeed, false, false);
                }

                else //사람 형태
                {
                    Transform_Stat(20, player_Stat.moveZone_Speed, false, true);
                }
            }

            else if (teamZone.team == Team.Etc) //그 이외의 진영일 때
            {
                if (_player_Input.squid_Form)// 오징어 형태
                {
                    Transform_Stat(0, player_Stat.moveZone_Speed, true, false);
                }

                else //사람 형태
                {
                    Transform_Stat(0, player_Stat.moveZone_Speed, false, true);
                }

                for (int i = 0; i < player_Wave.Length - 1; i++)
                {
                    player_Wave[i].Stop();
                }
            }

            else //적 진영에 있을 때
            {
                if (_player_Input.squid_Form)// 오징어 형태
                {
                    Transform_Stat(0, player_Stat.enemyZone_Speed, true, false);
                }

                else //사람 형태
                {
                    Transform_Stat(0, player_Stat.enemyZone_Speed, false, true);
                }
            }
        }
        else //공중에 있을 때
        {
            _isJump = true;
            raycast_Object = null;

            if (_player_Input.squid_Form)// 오징어 형태
            {
                Transform_Stat(0, player_Stat.dashSpeed, true, false);
            }

            else //사람 형태
            {
                Transform_Stat(0, player_Stat.moveZone_Speed, false, true);
            }
        }

        if (_player_Input.move_Vec == Vector3.zero || _isJump)
        {
            for (int i = 0; i < player_Wave.Length - 1; i++)
            {
                player_Wave[i].Stop();
            }
        }
    }

    public void Transform_Stat(int ammo, float speed, bool Squid, bool Human)
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftShift))
        {
            player_Wave[2].Play(); //1회만 실행되는 소환 이펙트
        }

        _player_shot.Reload_Ammo(ammo); // 재장전
        _player_Speed = speed;

        Transform_Mesh(Squid, Human);   //형태 변형

        if (Human)
        {
            player_Wave[1].Stop();
            player_Wave[0].Play(); // 인간 발걸음
        }
        else
        {

            player_Wave[0].Stop();
            player_Wave[1].Play(); // 오징어 발걸음
        }

        void Transform_Mesh(bool squid, bool human)
        {
            foreach (GameObject obj in human_Object)
            {
                obj.SetActive(human);
            }
            squid_Object.SetActive(squid);
        }
    }

    #endregion
}
