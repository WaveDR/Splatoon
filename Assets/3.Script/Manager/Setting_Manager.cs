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
        back_Text.text = "�κ��";
        foreach (GameObject obj in playerSelect_Weapon)
        {
            obj.SetActive(true);
        }
    }
    public void Select_Team(string team)
    {
        back_Text.text = "�ڷΰ���";
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
        back_Text.transform.parent.GetComponent<Button>().enabled = true; //�ڷΰ��� Ȱ��ȭ
        page_Num++;
        reset_Obj(1);
    }
    public void Select_Weapon(string weapon)
    {
        back_Text.text = "�ڷΰ���";
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
                back_Text.text = "�κ��";
                npc_Text.text = "��, ����? ���� �ٲٰ�? \n ��������ε�~";
                back_Text.transform.parent.GetComponent<Button>().enabled = false; //�ڷΰ��� ��Ȱ��ȭ
                break;
            case 2:
                page_Num--;
                player_Name = null;
                btn_weapon(0, EWeapon.Gun, true);
                reset_Obj(1);
                npc_Text.text = "���� �ٲٷ���? \n �ű⼭ �ű��ϱ�~?";
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
                npc_Text.text = "��� �޺��Ƹ�. \n ��¥ Ƽ �������� ������ ī�忡�� �װ� ���� ������ ������.";
                break;
            case 1:
                npc_Text.text = "��, ���� ���⸦ �������� ��. \n �׳� ���� ���~ ������ �� ���ؿ� �ű⼭ �ű�ϱ�.";
                break;
            case 2:
                npc_Text.text = "����. �������̾�. \n ���� �� �ܸ��⿡ ���� �̸��� ����.";
                break;
        }
    }

    public void MoveScene()
    {
        //���߿� ��Ʈ��ũ�� ���� ��������

        player_Team.team = team;
        player_shot.WeaponType = weapon;

        //�ε� UI �ѱ�
        loading_Page.SetActive(true);

        //�÷��̾� ���� ��Ȱ��ȭ
        player_shot.gameObject.SetActive(false);
        player_shot.gameObject.SetActive(true);

        //�κ� ������Ʈ ��Ȱ��ȭ, ���� ������Ʈ Ȱ��ȭ
        stage_Lobby[1].SetActive(true);
        stage_Lobby[0].SetActive(false);


        //1.5�� �Ŀ� �ε��� ����
        Invoke("LoadingOff",2f);

        //�κ� UI ��Ȱ��ȭ
        gameObject.SetActive(false);
    }

    private void LoadingOff()
    {
        loading_Page.SetActive(false);

        //�÷��̾� UI �ѱ�
        player_shot.skill_UI_Obj.SetActive(true);
    }
}