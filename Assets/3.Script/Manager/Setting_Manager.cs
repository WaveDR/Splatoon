using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Setting_Manager : MonoBehaviour
{
    public PlayerShooter player_shot;
    public PlayerTeams player_Team;
    [SerializeField] private GameObject[] playerSelect_Card;
    [SerializeField] private GameObject[] playerSelect_Camera;
    [SerializeField] private GameObject[] playerSelect_Weapon;
    [SerializeField] private GameObject[] stage_Lobby;
    [SerializeField] private GameObject loading_Page;
    [SerializeField] private ParticleSystem select_Effect;
    [SerializeField] private Text npc_Text;
    [SerializeField] private Text back_Text;
    [SerializeField] private Image[] images;
    [SerializeField] private int page_Num;

    public ETeam team;
    public EWeapon weapon;
    public string player_Name;

    public Color32 team_Yellow = new Color32(253, 242, 63, 255);
    public Color32 team_Blue = new Color32(129, 67, 255, 255);
    private void Awake()
    {
        player_shot = FindObjectOfType<PlayerShooter>();
        player_Team = FindObjectOfType<PlayerTeams>();
        reset_Obj(0);
        stage_Lobby[0].SetActive(true);

        stage_Lobby[1].SetActive(false);
        loading_Page.SetActive(false);
        player_shot.skill_UI_Obj.SetActive(false);
        page_Num = 0;
        back_Text.text = "로비실";
        foreach (GameObject obj in playerSelect_Weapon)
        {
            obj.SetActive(true);
        }
    }
    public void Select_Team(string team)
    {
        back_Text.text = "뒤로가기";
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
        back_Text.transform.parent.GetComponent<Button>().enabled = true; //뒤로가기 활성화
        page_Num++;
        reset_Obj(1);
    }
    public void Select_Weapon(string weapon)
    {
        back_Text.text = "뒤로가기";
        switch (weapon)
        {
            case "Brush":
                this.weapon = btn_weapon(0, EWeapon.Brush,false);
                break;
            case "Gun":
                this.weapon = btn_weapon(1, EWeapon.Gun, false);
                break;
            case "Bow":
                this.weapon = btn_weapon(2, EWeapon.Bow, false);
                break;
        }
        page_Num++;
        reset_Obj(2);
    }


    public void Write_Name(string name)
    {
        name = player_Name;
    }
    public void Back_Page()
    {
        switch (page_Num)
        {
            case 0:
                return;
            case 1:
                page_Num--;
                reset_Obj(0);
                back_Text.text = "로비실";
                npc_Text.text = "엥, 뭐야? 진영 바꾸게? \n 낙장불입인데~";
                back_Text.transform.parent.GetComponent<Button>().enabled = false; //뒤로가기 비활성화
                break;
            case 2:
                page_Num--;
                player_Name = null;
                btn_weapon(0, EWeapon.Gun, true);
                reset_Obj(1);
                npc_Text.text = "무기 바꾸려고? \n 거기서 거기라니까~?";
                break;
        }
    }
    private EWeapon btn_weapon(int i, EWeapon weapon, bool back)
    {
        select_Effect.transform.position = playerSelect_Weapon[i].transform.position;
        if(!back)
        playerSelect_Weapon[i].SetActive(false);
        else
        {
            for (int j = 0; j < playerSelect_Weapon.Length; j++)
            {
                playerSelect_Weapon[j].SetActive(true);
                Debug.Log(playerSelect_Weapon[j].name);
            }
        }
        select_Effect.Play();
        return weapon;
    }
    private void reset_Obj(int num)
    {
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
    }

    public void MoveScene()
    {
        //나중에 네트워크에 넣을 정보값들

        player_Team.team = team;
        player_shot.WeaponType = weapon;

        //로딩 UI 켜기
        loading_Page.SetActive(true);

        //플레이어 무기 재활성화
        player_shot.gameObject.SetActive(false);
        player_shot.gameObject.SetActive(true);

        //로비 오브젝트 비활성화, 대기실 오브젝트 활성화
        stage_Lobby[1].SetActive(true);
        stage_Lobby[0].SetActive(false);


        //1.5초 후에 로딩씬 끄기
        Invoke("LoadingOff",2f);

        //로비 UI 비활성화
        gameObject.SetActive(false);
    }

    private void LoadingOff()
    {
        loading_Page.SetActive(false);

        //플레이어 UI 켜기
        player_shot.skill_UI_Obj.SetActive(true);
    }
}
