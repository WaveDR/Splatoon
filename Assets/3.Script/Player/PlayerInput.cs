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

        //ĳ���� �̵�
        move_Hor = Input.GetAxis(move_Hor_S);
        move_Ver = Input.GetAxis(move_Ver_S);
        //ĳ���� ����
        jDown = Input.GetButtonDown("Jump");

        if (!_player_Con.isDead)
        {
            //ĳ���� ���� �� ���º�ȯ ����
            fire = Input.GetButton("Fire1");
            fUp = Input.GetButtonUp("Fire1");
            fDown = Input.GetButtonDown("Fire1");
            squid_Form = Input.GetButton("Run");
        }

        if (!isWall) //���� �ƴϸ� ���� �̵�
            move_Vec = new Vector3(Move_Hor, 0, Move_Ver);

        else //���̸� �� Ÿ��
        {
            if (isWall_Hor) // Ⱦ�̵�
            {
                if (!isWall_Left) //���� ���� ������ �� �̵� ���� ��ȯ
                    move_Vec = new Vector3(0, Move_Hor, Move_Ver);
                else
                    move_Vec = new Vector3(0, -Move_Hor, Move_Ver);
            }
            else //���̵�
            {
                move_Vec = new Vector3(Move_Hor, Move_Ver, 0);
            }
        }

        //��¡���� ȸ����
        Squid_Euler(Move_Hor, Move_Ver);
    }

    void Squid_Euler(float h, float v)
    {

        if (h < 0) //��
            squid_FinalRot = -89.99f;
        if (h > 0) //��
            squid_FinalRot = 89.99f;
        if (v > 0) //��
            squid_FinalRot = 0.1f;
        if (v < 0) //��
            squid_FinalRot = 179.9f;

        if (h > 0 && v > 0) //���� ���
            squid_FinalRot = 44.99f;

        if (h < 0 && v > 0) //���� ���
            squid_FinalRot = -44.99f;

        if (h > 0 && v < 0) //���� �ϴ�
            squid_FinalRot = 134.99f;

        if (h < 0 && v < 0) //���� �ϴ�
            squid_FinalRot = -134.99f;
    }

}
