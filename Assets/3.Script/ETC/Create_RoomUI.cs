using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Create_RoomUI : MonoBehaviour
{
    public GameObject create_Room;
    public GameObject notMatch;
    private Animator ui_Anim;
    public Text maxPlayer;
 

    public int num;
    // Start is called before the first frame update
    void Awake()
    {
        TryGetComponent(out ui_Anim);
        num = Mathf.Clamp(num, 0, 8);
    }
    public void NextUI()
    {
        num++;
        int maxPlayer_Count = ((num + 1) * 2);
        maxPlayer_Count = Mathf.Clamp(maxPlayer_Count, 0, 8);

        ui_Anim.SetInteger("CreateRoom", num);
        BGM_Manager.Instance.Play_Sound_BGM("UI_Click");


        if (num >= 5) 
        { 
            num = 0;
            maxPlayer_Count = 2;
        }
        maxPlayer.text = maxPlayer_Count.ToString();
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
        Photon_Manager.Instance.set_Manager.LoadingOn();
        ui_Anim.SetBool("isMatch", isMatch);
        if (!isMatch) notMatch.SetActive(false);
    }

    public void GameStart()
    {
        GameManager.Instance.skip_Start = true;
        Photon_Manager.Instance.set_Manager.LoadingOff();

    }
}
