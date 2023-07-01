using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Create_RoomUI : MonoBehaviour
{
    public GameObject create_Room;
    public GameObject notMatch;
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
        BGM_Manager.Instance.Play_Sound_BGM("UI_Click");

        if (num >= 5) num = 0;
    }
    
    public void Create_Room_Open()
    {
        BGM_Manager.Instance.Play_Sound_BGM("UI_Click");

        create_Room.SetActive(true);
    }
    public void Create_Room_Close()
    {
        BGM_Manager.Instance.Play_Sound_BGM("UI_Click");

        create_Room.SetActive(false);
    }
    public void Matching(bool isMatch)
    {
        BGM_Manager.Instance.Play_Sound_BGM("UI_Click");

        ui_Anim.SetBool("isMatch", isMatch);

        if (!isMatch) notMatch.SetActive(false);
    }
}
