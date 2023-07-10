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


        #region UI �ʱ�ȭ 
        Reset_Obj(0);
        change_Cam[0].SetActive(true);
        change_Cam[1].SetActive(false);

        loading_Page.SetActive(false);
        Matching_Obj.SetActive(false);

        page_Num = 0;
        back_Text.text = "�κ��";
        foreach (GameObject obj in playerSelect_Weapon)
        {
            obj.SetActive(true);
        }
        #endregion

    }
    public void Select_Team(string team)
    {
        //�� ���ϴ� ��ư
        //��� ��ư ���� ���� �� ������ ����
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

        //�ڷΰ��� Ȱ��ȭ
        back_Text.transform.parent.GetComponent<Button>().enabled = true; 
        back_Text.text = "�ڷΰ���";

        //���� ������ ����
        page_Num++;
        Reset_Obj(1);
    }

    
    public void Select_Weapon(string weapon)
    {
        //�Ű������� ���� enum �޼��� ȣ�� �� ����
        switch (weapon)
        {
            //�Ű����� : �����ȣ / ���� Ÿ�� / �ڷΰ��� ����
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

        back_Text.text = "�ڷΰ���";

        //���� ������ ����
        page_Num++;
        Reset_Obj(2);
    }

    //�ڷΰ��� ������
    public void Back_Page()
    {
        //����� page_Num�� ���� ��� �� ������ ����
        switch (page_Num)
        {
            //�� ������ ����
            case 0:
                return;

            //���� ������ ����
            case 1:
                page_Num--;
                Reset_Obj(0);
                npc_Text.text = "��, ����? ���� �ٲٰ�? \n ��������ε�~";

                //�ڷΰ��� ��ư ��Ȱ��ȭ
                back_Text.text = "�κ��";
                back_Text.transform.parent.GetComponent<Button>().enabled = false; 
                break;

            //�̸� ������ ����
            case 2:
                page_Num--;
                Reset_Obj(1);

                //�г��� �ʱ�ȭ
                player_Name = null;
                //���� �ʱ�ȭ
                Select_weapon(0, EWeapon.Gun, true);
                npc_Text.text = "���� �ٲٷ���? \n �ű⼭ �ű��ϱ�~?";
                break;
        }
    }
    
    private EWeapon Select_weapon(int i, EWeapon weapon, bool back)
    {
        //���� ���� ����Ʈ
        select_Effect.transform.position = playerSelect_Weapon[i].transform.position;
        select_Effect.Play();

        //���� ���� ��Ȱ��ȭ
        if (!back)
        playerSelect_Weapon[i].SetActive(false);

        //�ڷΰ��� ���� �� ���� ���� Ȱ��ȭ
        else
        {
            for (int j = 0; j < playerSelect_Weapon.Length; j++)
            {
                playerSelect_Weapon[j].SetActive(true);
                Debug.Log(playerSelect_Weapon[j].name);
            }
        }

        //���õ� EWeapon�� ��ȯ
        return weapon;
    }

    private void Reset_Obj(int num)
    {
        //�Ű������� ���� ������ �� ī�޶� ����

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

        //������ ��ȣ�� ���� ��� ����
        switch (page_Num)
        {
            case 0:
                npc_Text.text = "��� �޺��Ƹ�. \n ��¥ Ƽ �������� ������ ī�忡�� �װ� ���� ������ ������.";
                break;
            case 1:
                npc_Text.text = "��, ���� ���⸦ ������ ��. \n �׳� ���� ���~ ������ �� ���ؿ� �ű⼭ �ű�ϱ�.";
                break;
            case 2:
                npc_Text.text = "����. �������̾�. \n ���� �� �ܸ��⿡ ���� �̸��� ����.";
                break;
        }

        //Ŭ����
        //BGM_Manager.Instance.Play_Sound_BGM("UI_Click");
    }

    public void MoveScene()
    {
        //�Է»��� ���� �Ϸ� ��ư

        //�г��� ������ ���� �� No Name���� ���� 
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

        //���õ� �� / ���⸦ Prefab�� ����
        player_Team.team = team;
        player_shot.WeaponType = weapon;

        //�ε� UI �ѱ�
        LoadingOn();
        //�κ� ������Ʈ ��Ȱ��ȭ, ���� ������Ʈ Ȱ��ȭ
        change_Cam[1].SetActive(true);
        change_Cam[0].SetActive(false);

        //1.5�� �Ŀ� �ε��� ����
        Invoke("Set_InGame_UI", 2f);

        //�κ� UI ��Ȱ��ȭ
        //BGM_Manager.Instance.Play_Sound_BGM("UI_Click");
        gameObject.SetActive(false);
    }

    void Set_InGame_UI()
    {
        //�÷��̾� �⺻ UI Ȱ��ȭ
        player_shot.skill_UI_Obj.SetActive(true);

        //��Ī UI Ȱ��ȭ
        Matching_Obj.SetActive(true);

        //�ε� �� ��Ȱ��ȭ
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
