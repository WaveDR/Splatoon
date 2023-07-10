using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInput : MonoBehaviourPun
{
    [Header("Player Move Index")]
    [Header("===================================")]
    [SerializeField] private string move_Hor_S;
    [SerializeField] private string move_Ver_S;
    public Vector3 move_Vec;
    [SerializeField] private float move_Hor = 0;
    [SerializeField] private float move_Ver = 0;

    
    public float Move_Hor => move_Hor;
    public float Move_Ver => move_Ver;
    public string Move_Hor_S => move_Hor_S;
    public string Move_Ver_S => move_Ver_S;

    [Header("Player Interaction")]
    [Header("===================================")]
    public string player_Name;

    public bool jDown;
    public bool fDown;
    public bool fUp;
    public bool fire;
    public bool squid_Form = false;
    public bool isWall;
    public bool isWall_Hor;
    public bool isWall_Left;

    public bool isClick;

    [Header("Player Squid")]
    [Header("===================================")]
    public float squid_FinalRot;
    private float squidRot;
    public float player_SquidRot
    {
        get { return squidRot; }
        set
        {
            squidRot = value;

            if(squid_FinalRot > 0 && squid_FinalRot < 180)
                squidRot = Mathf.Clamp(squidRot, 0, squid_FinalRot);

            if (squid_FinalRot < 0 && squid_FinalRot > -180)
                squidRot = Mathf.Clamp(squidRot, squid_FinalRot, 0);
        }
    }

    private PlayerController _player_Con;

    // Start is called before the first frame update
    private void Awake()
    {
        TryGetComponent(out _player_Con);
    }
    void Update()
    {
        if (!photonView.IsMine) return;

        if (_player_Con.isStop)
        {
            move_Hor = 0;
            move_Ver = 0;
            move_Vec = Vector3.zero;
            return;
        }

        //캐릭터 이동
        move_Hor = Input.GetAxis(move_Hor_S);
        move_Ver = Input.GetAxis(move_Ver_S);
        //캐릭터 점프
        jDown = Input.GetButtonDown("Jump");

        if (!_player_Con.isDead)
        {
            //캐릭터 공격 및 형태변환 로직
            fire = Input.GetButton("Fire1");
            fUp = Input.GetButtonUp("Fire1");
            fDown = Input.GetButtonDown("Fire1");
            squid_Form = Input.GetButton("Run");
        }

        if (!isWall) //벽이 아니면 지상 이동
            move_Vec = new Vector3(Move_Hor, 0, Move_Ver);

        else //벽이면 벽 타기
        {
            if (isWall_Hor) // 횡이동
            {
                if (!isWall_Left) //왼쪽 벽과 오른쪽 벽 이동 기준 변환
                    move_Vec = new Vector3(0, Move_Hor, Move_Ver);
                else
                    move_Vec = new Vector3(0, -Move_Hor, Move_Ver);
            }
            else //종이동
            {
                move_Vec = new Vector3(Move_Hor, Move_Ver, 0);
            }
        }

        //오징어폼 회전값
        Squid_Euler(Move_Hor, Move_Ver);
    }

    void Squid_Euler(float h, float v)
    {

        if (h < 0) //좌
            squid_FinalRot = -89.99f;
        if (h > 0) //우
            squid_FinalRot = 89.99f;
        if (v > 0) //상
            squid_FinalRot = 0.1f;
        if (v < 0) //하
            squid_FinalRot = 179.9f;

        if (h > 0 && v > 0) //우측 상단
            squid_FinalRot = 44.99f;

        if (h < 0 && v > 0) //좌측 상단
            squid_FinalRot = -44.99f;

        if (h > 0 && v < 0) //우측 하단
            squid_FinalRot = 134.99f;

        if (h < 0 && v < 0) //좌측 하단
            squid_FinalRot = -134.99f;
    }

}
