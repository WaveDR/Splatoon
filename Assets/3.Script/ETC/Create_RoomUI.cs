using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Create_RoomUI : MonoBehaviour
{
    [Header("UI GameObject")]
    public GameObject create_Room;
    public GameObject close_Match_Btn;

    [Header("UI Animator")]
    private Animator ui_Anim;

    [Header("Max Player Text")]
    public Text maxPlayer;
    public int num;
    // Start is called before the first frame update
    void Awake()
    {
        TryGetComponent(out ui_Anim);
    }

    //방 생성 UI 선택 시 다음 UI 정보
    public void NextUI()
    {
        BGM_Manager.Instance.Play_Sound_BGM("UI_Click");
        num++;
        //최대 인원 텍스트
        int maxPlayer_Count = ((num + 1) * 2);
        maxPlayer_Count = Mathf.Clamp(maxPlayer_Count, 0, 8);

        ui_Anim.SetInteger("CreateRoom", num);

        if (num >= 5)
        { 
            num = 0;
            maxPlayer_Count = 2;
        }
        maxPlayer.text = maxPlayer_Count.ToString();
    }
    
    // Create Room UI 활성화
    public void Create_Room_Open()
    {
        BGM_Manager.Instance.Play_Sound_BGM("UI_Click");

        create_Room.SetActive(true);
    }

    // Create Room UI 비활성화
    public void Create_Room_Close()
    {
        BGM_Manager.Instance.Play_Sound_BGM("UI_Click");

        create_Room.SetActive(false);
    }

    //매칭 버튼
    public void Matching(bool isMatch)
    {
        BGM_Manager.Instance.Play_Sound_BGM("UI_Click");
        Photon_Manager.Instance.set_Manager.LoadingOn();
        ui_Anim.SetBool("isMatch", isMatch);
        if (!isMatch) close_Match_Btn.SetActive(false);
    }

    //즉시 시작 버튼
    public void GameStart()
    {
        GameManager.Instance.skip_Start = true;
        Photon_Manager.Instance.set_Manager.LoadingOff();
    }
}
