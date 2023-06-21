using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{

    [SerializeField] private string move_Hor_S;
    [SerializeField] private string move_Ver_S;

    [SerializeField] private float move_Hor = 0;
    [SerializeField] private float move_Ver = 0;
 

    public Vector3 move_Vec;
    
    public float Move_Hor => move_Hor;
    public float Move_Ver => move_Ver;

    public bool jDown;
    public bool fire;
    public bool squid_Form = false;

    public string Move_Hor_S => move_Hor_S;
    public string Move_Ver_S => move_Ver_S;

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


    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        move_Hor = Input.GetAxis(move_Hor_S);
        move_Ver = Input.GetAxis(move_Ver_S);

        jDown = Input.GetButtonDown("Jump");
        fire = Input.GetButton("Fire1");
        squid_Form = Input.GetButton("Run");


        move_Vec.x = move_Hor;
        move_Vec.z = move_Ver;
        move_Vec = new Vector3(Move_Hor, 0, Move_Ver);
        Squid_Euler(Move_Hor, Move_Ver);

    }

    void Squid_Euler(float h, float v)
    {

        if (h < 0 ) //좌
            squid_FinalRot = -90f;
        if (h > 0 ) //우
            squid_FinalRot = 90f;
        if (v > 0 ) //상
            squid_FinalRot = 0.1f;
        if (v < 0 ) //하
            squid_FinalRot = 179.9f;

        if (h > 0 && v > 0) //우측 상단
            squid_FinalRot = 45f;

        if (h < 0 && v > 0) //좌측 상단
            squid_FinalRot = -45f;

        if (h > 0 && v < 0) //우측 하단
            squid_FinalRot = 135f;

        if (h < 0 && v < 0) //좌측 하단
            squid_FinalRot = -135f;
    }
}
