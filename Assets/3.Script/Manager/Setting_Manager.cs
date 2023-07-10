using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class Setting_Manager : MonoBehaviour
{

    [Header("Player Select UI")]
    [SerializeField] private GameObject[] playerSelect_Card;
    [SerializeField] private GameObject[] playerSelect_Camera;
    [SerializeField] private GameObject[] playerSelect_Weapon;
    [SerializeField] private ParticleSystem select_Effect;

    [Header("Page Change")]
    [SerializeField] private GameObject loading_Page;
    [SerializeField] private GameObject[] change_Cam;
    [SerializeField] private GameObject Matching_Obj;

    [Header("Select Page UI")]
    [SerializeField] private Text npc_Text;
    [SerializeField] private Text back_Text;
    [SerializeField] private Image[] images;
    [SerializeField] private int page_Num;

    [Header("Select Player Data")]
    public GameObject player_Prefabs;
    private PlayerShooter player_shot;
    private PlayerTeams player_Team;
    private PlayerInput player_Input;
    public InputField player_Name;

    [SerializeField] private Color32 team_Yellow = new Color32(253, 242, 63, 255);
    [SerializeField] private Color32 team_Blue = new Color32(129, 67, 255, 255);
    public ETeam team;
    public EWeapon weapon;

    [Header("Photon Manager")]
    public Photon_Manager photon_Manager;

    private void Awake()
    {
        // GetComponent to Prefab
        player_shot = player_Prefabs.GetComponent<PlayerShooter>();
        player_Team = player_Prefabs.GetComponent<PlayerTeams>();
        player_Input = player_Prefabs.GetComponent<PlayerInput>();

        // GetComponent to PhotonManager
        photon_Manager = FindObjectOfType<Photon_Manager>();

        //Cursor Off
        GameManager.Instance.SetCursorState(false);


        #region UI 초기화 
        Reset_Obj(0);
        change_Cam[0].SetActive(true);
        change_Cam[1].SetActive(false);

        loading_Page.SetActive(false);
        Matching_Obj.SetActive(false);

        page_Num = 0;
        back_Text.text = "로비실";
        foreach (GameObject obj in playerSelect_Weapon)
        {
            obj.SetActive(true);
        }
        #endregion

    }
    public void Select_Team(string team)
    {
        //팀 정하는 버튼
        //모든 버튼 색을 정한 팀 색으로 변경
        if (team == "Blue")
        {
           foreach(Image img in images)
            {
                img.color = team_Blue;
            }
            this.team = ETeam.Blue;

        }
        else if (team == "Yellow")
        {
            foreach (Image img in images)
            {
                img.color = team_Yellow;
            }
            this.team = ETeam.Yellow;
        }

        //뒤로가기 활성화
        back_Text.transform.parent.GetComponent<Button>().enabled = true; 
        back_Text.text = "뒤로가기";

        //다음 페이지 변경
        page_Num++;
        Reset_Obj(1);
    }

    
    public void Select_Weapon(string weapon)
    {
        //매개변수에 따른 enum 메서드 호출 및 적용
        switch (weapon)
        {
            //매개변수 : 무기번호 / 무기 타입 / 뒤로가기 여부
            case "Brush":
                this.weapon = Select_weapon(0, EWeapon.Brush,false);
                break;
            case "Gun":
                this.weapon = Select_weapon(1, EWeapon.Gun, false);
                break;
            case "Bow":
                this.weapon = Select_weapon(2, EWeapon.Bow, false);
                break;
        }

        back_Text.text = "뒤로가기";

        //다음 페이지 변경
        page_Num++;
        Reset_Obj(2);
    }

    //뒤로가기 페이지
    public void Back_Page()
    {
        //진행된 page_Num에 따라 대사 및 페이지 변경
        switch (page_Num)
        {
            //팀 페이지 기준
            case 0:
                return;

            //무기 페이지 기준
            case 1:
                page_Num--;
                Reset_Obj(0);
                npc_Text.text = "엥, 뭐야? 진영 바꾸게? \n 낙장불입인데~";

                //뒤로가기 버튼 비활성화
                back_Text.text = "로비실";
                back_Text.transform.parent.GetComponent<Button>().enabled = false; 
                break;

            //이름 페이지 기준
            case 2:
                page_Num--;
                Reset_Obj(1);

                //닉네임 초기화
                player_Name = null;
                //무기 초기화
                Select_weapon(0, EWeapon.Gun, true);
                npc_Text.text = "무기 바꾸려고? \n 거기서 거기라니까~?";
                break;
        }
    }
    
    private EWeapon Select_weapon(int i, EWeapon weapon, bool back)
    {
        //무기 선택 이펙트
        select_Effect.transform.position = playerSelect_Weapon[i].transform.position;
        select_Effect.Play();

        //선택 무기 비활성화
        if (!back)
        playerSelect_Weapon[i].SetActive(false);

        //뒤로가기 선택 시 무기 전원 활성화
        else
        {
            for (int j = 0; j < playerSelect_Weapon.Length; j++)
            {
                playerSelect_Weapon[j].SetActive(true);
                Debug.Log(playerSelect_Weapon[j].name);
            }
        }

        //선택된 EWeapon값 반환
        return weapon;
    }

    private void Reset_Obj(int num)
    {
        //매개변수에 따른 페이지 및 카메라 변경

        for (int i = 0; i < playerSelect_Card.Length; i++)
        {
            playerSelect_Card[i].SetActive(true);
            playerSelect_Camera[i].SetActive(true);

            if (num != i)
            {
                playerSelect_Card[i].SetActive(false);
                playerSelect_Camera[i].SetActive(false);
            }
        }

        //페이지 번호에 따라 대사 변경
        switch (page_Num)
        {
            case 0:
                npc_Text.text = "어서와 햇병아리. \n 초짜 티 내지말고 오른쪽 카드에서 네가 속할 진영을 선택해.";
                break;
            case 1:
                npc_Text.text = "자, 이제 무기를 고르도록 해. \n 그냥 대충 골라~ 어차피 네 수준엔 거기서 거기니까.";
                break;
            case 2:
                npc_Text.text = "좋아. 마지막이야. \n 이제 저 단말기에 너의 이름을 적어.";
                break;
        }

        //클릭음
        //BGM_Manager.Instance.Play_Sound_BGM("UI_Click");
    }

    public void MoveScene()
    {
        //입력사항 선택 완료 버튼

        //닉네임 정하지 않을 시 No Name으로 지정 
        if (player_Name.text == null)
        {
            player_Name.text = "No Name";
            player_Input.player_Name = player_Name.text;
        }
        else if (player_Name.text == " " || player_Name.text == "")
        {
            player_Name.text = "No Name";
            player_Input.player_Name = player_Name.text;
        }
        else
        {
            player_Input.player_Name = player_Name.text;
        }

        //선택된 팀 / 무기를 Prefab에 적용
        player_Team.team = team;
        player_shot.WeaponType = weapon;

        //로딩 UI 켜기
        LoadingOn();
        //로비 오브젝트 비활성화, 대기실 오브젝트 활성화
        change_Cam[1].SetActive(true);
        change_Cam[0].SetActive(false);

        //1.5초 후에 로딩씬 끄기
        Invoke("Set_InGame_UI", 2f);

        //로비 UI 비활성화
        //BGM_Manager.Instance.Play_Sound_BGM("UI_Click");
        gameObject.SetActive(false);
    }

    void Set_InGame_UI()
    {
        //플레이어 기본 UI 활성화
        player_shot.skill_UI_Obj.SetActive(true);

        //매칭 UI 활성화
        Matching_Obj.SetActive(true);

        //로딩 씬 비활성화
        LoadingOff();
    }

    //============================================= Loading Page ========================================
    public void LoadingOff()
    {
        loading_Page.SetActive(false);
    }
    public void LoadingOn()
    {
        loading_Page.SetActive(true);
    }
}
