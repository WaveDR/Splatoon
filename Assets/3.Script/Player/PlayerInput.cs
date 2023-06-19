using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{

    [SerializeField] private string move_Hor_S;
    [SerializeField] private string move_Ver_S;

    [SerializeField] private float move_Hor = 0;
    [SerializeField] private float move_Ver = 0;
    [SerializeField] private float move_speed = 0;
 

    public Vector3 move_Vec;

    public float Move_Hor => move_Hor;
    public float Move_Ver => move_Ver;
    public bool jDown;

    public float Move_Speed => move_speed;

    public string Move_Hor_S => move_Hor_S;
    public string Move_Ver_S => move_Ver_S;

    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        move_Hor = Input.GetAxis(move_Hor_S);
        move_Ver = Input.GetAxis(move_Ver_S);
        jDown = Input.GetButtonDown("Jump");
        move_Vec.x = move_Hor;
        move_Vec.z = move_Ver;

        move_Vec = new Vector3(move_Hor, 0, move_Ver);

    }
}
