using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Create_RoomUI : MonoBehaviour
{
    public GameObject create_Room;
    private Animator ui_Anim;
    public int num;
    // Start is called before the first frame update
    void Awake()
    {
        TryGetComponent(out ui_Anim);
    }

    public void NextUI()
    {
        num++;
        ui_Anim.SetInteger("CreateRoom", num);

        if (num >= 5) num = 0;
    }
    public void Create_Room_Open()
    {
        create_Room.SetActive(true);
    }
    public void Create_Room_Close()
    {
        create_Room.SetActive(true);
    }
}
