using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerStat stat;
    [SerializeField] GameObject raycast_Object;

    private PlayerInput _player_Input; 
    private Rigidbody _player_rigid; 
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
        _player_CurHp = stat.maxHeath;
        _player_Speed = stat.moveZone_Speed;
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
        Debug.DrawRay(transform.position, Vector3.down * stat.detectRange, Color.green); ;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, stat.detectRange, stat.floor_Layer))
        {
            raycast_Object = hit.collider.gameObject;


            TeamZone teamZone = raycast_Object.GetComponent<TeamZone>();


            if (teamZone.team == Team.Blue || teamZone.team == Team.Yellow)
            {  
                _isJump = false;
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
