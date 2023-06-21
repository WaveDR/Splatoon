using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerStat player_Stat;
    [SerializeField] private GameObject raycast_Object;
    [SerializeField] private ParticleSystem player_MoveWave;

    public Team player_Team;

    private PlayerInput _player_Input; 
    private Rigidbody _player_rigid; 
    private PlayerShooter _player_shot;
    private Animator _player_Anim;

    public bool _isJump;

    private bool isTransmition = false;

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
        RaycastFloor();
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
            _isJump = true;
            _player_rigid.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }
    }

    #region Player Raycast

    private void RaycastFloor()
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
                isTransmition = true;

            }

            if (teamZone.team == player_Team) // 내 진영과 현재 바닥 진영이 같을 때
            {
                _player_shot.Reload_Ammo();
                isTransmition = true;

                if(_player_Input.move_Vec != Vector3.zero)
                {
                   player_MoveWave.Play();
                }
            }
            else //내 진영과 바닥 진영이 같지 않을 때
            {
                isTransmition = false;
            }
        }
        else
        {
            _isJump = true;
            raycast_Object = null;
        }
    }

    #endregion
}
