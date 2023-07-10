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

    //�� ���� UI ���� �� ���� UI ����
    public void NextUI()
    {
        BGM_Manager.Instance.Play_Sound_BGM("UI_Click");
        num++;
        //�ִ� �ο� �ؽ�Ʈ
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
    
    // Create Room UI Ȱ��ȭ
    public void Create_Room_Open()
    {
        BGM_Manager.Instance.Play_Sound_BGM("UI_Click");

        create_Room.SetActive(true);
    }

    // Create Room UI ��Ȱ��ȭ
    public void Create_Room_Close()
    {
        BGM_Manager.Instance.Play_Sound_BGM("UI_Click");

        create_Room.SetActive(false);
    }

    //��Ī ��ư
    public void Matching(bool isMatch)
    {
        BGM_Manager.Instance.Play_Sound_BGM("UI_Click");
        Photon_Manager.Instance.set_Manager.LoadingOn();
        ui_Anim.SetBool("isMatch", isMatch);
        if (!isMatch) close_Match_Btn.SetActive(false);
    }

    //��� ���� ��ư
    public void GameStart()
    {
        GameManager.Instance.skip_Start = true;
        Photon_Manager.Instance.set_Manager.LoadingOff();
    }
}
