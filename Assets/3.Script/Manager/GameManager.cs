using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using Photon.Pun;
using UnityEngine.SceneManagement;

[Serializable]
public class Player_Info
{
    //���� ���� ���� �÷��̾� ��
    public Player_Info(ETeam _team, EWeapon _weapon, string _name, int _score) { team = _team; weapon = _weapon; name = _name; score = _score; }
    public ETeam team;
    public EWeapon weapon;
    public string name;
    public int score;
}
public class GameManager : MonoBehaviourPun
{
    public static GameManager _instance = null;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }

            return _instance;
        }
        set { _instance = value; }
    }

    [Header("Play Zone")]
    public List<TeamZone> nodes = new List<TeamZone>();

    [Header("Player")]
    [Header("===================================")]
    public PlayerController[] players;
    public GameObject AI_Prefab;
    public Player_Info player_Data;
    public Dictionary<int, Player_Info> player_Info = new Dictionary<int, Player_Info>();
    [SerializeField] private Player_MVP mvp_Model;


    [Header("UI")]
    [Header("===================================")]

    [SerializeField] private Text timeText; 

    [SerializeField] private Sprite[] count_Sprite;
    [SerializeField] private Image count_Image;

    [SerializeField] private Image scoreGage_Yellow;
    [SerializeField] private Image scoreGage_Blue;

    [SerializeField] private Text scoreCount_Yellow;
    [SerializeField] private Text scoreCount_Blue;

    [SerializeField] private Text[] mvp_Data;

    [SerializeField] private Image[] yellow_Player_UI;
    [SerializeField] private Image[] blue_Player_UI;

    [SerializeField] private Sprite[] weapon_UI;

    [Header("Character Anim")]
    [Header("===================================")]
    [SerializeField] private Animator ui_Anim;
    [SerializeField] private Animator manager_Anim;
    [SerializeField] private Animator mvp_Anim;
    [SerializeField] private WinAnim teamYellow_Anim;
    [SerializeField] private WinAnim teamBlue_Anim;


    [Header("Timer")]
    [Header("===================================")]
    [SerializeField] private float startTimer = 10;
    [SerializeField] private float endTimer = 180;
    private int _Min;
    private int _Sec;
    private float _Time;
    private float _BeepTime;
    private float _Charging_Score;
    public float time;
    public float deltaTime
    {
        get { return _Time; }
        set { _Time = value;
            _Time = Mathf.Clamp(_Time, 0, endTimer);
        }
    }

    [Header("SpawnPos")]
    [Header("===================================")]
    [SerializeField] private Transform[] _team_Yellow_Spawn;
    [SerializeField] private Transform[] _team_Blue_Spawn;
    public Transform[] team_Yellow_Spawn => _team_Yellow_Spawn;
    public Transform[] team_Blue_Spawn => _team_Blue_Spawn;


    [Header("EndGame Production")]
    [Header("===================================")]
    public MeshRenderer deadLine;
    public GameObject map_Camera;
    public bool gameStart;
    public bool gameEnd;
    public bool skip_Start;

    [Header("Particle")]
    [SerializeField] private ParticleSystem yellow_WinEffect;
    [SerializeField] private ParticleSystem blue_WinEffect;

    //Etc Bool
    private bool chargeCall;
    private bool isStart;
    private bool player_Check;
    public bool isLobby;

    private void Awake()
    {
        if (PhotonNetwork.InRoom)
        {
            Manager_Server();
            isStart = true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!player_Check)
        {
            Player_Count();
        }
        if (!isLobby)
        {
            if (Input.GetKey(KeyCode.Escape)) SetCursorState(false);

            else SetCursorState(true);

            if (!gameStart) StartCount();
            else if (!gameEnd) EndCount();

            EndScoreCharge(chargeCall);
        }
        time = deltaTime;
    }

    //============================================        �� CallBack   |   Nomal ��        ========================================================
    public void Manager_Server()
    {
        //GetComponent
        deadLine = GameObject.FindGameObjectWithTag("DeadLine").GetComponent<MeshRenderer>();
        map_Camera = GameObject.FindGameObjectWithTag("MapCamera");
        mvp_Model = transform.GetComponentInChildren<Player_MVP>();

        //Animator
        TryGetComponent(out manager_Anim);
        teamYellow_Anim = transform.GetChild(0).GetComponent<WinAnim>();
        teamBlue_Anim = transform.GetChild(1).GetComponent<WinAnim>();
        mvp_Anim = transform.GetChild(2).GetComponent<Animator>();
        teamYellow_Anim.gameObject.SetActive(false);
        teamBlue_Anim.gameObject.SetActive(false);

        deadLine.enabled = false; //������� �޽� ��Ȱ��ȭ
        deltaTime = startTimer; //���� �� ī��Ʈ  
    }
    
    public void FindPlayer()
    {
        //�÷��̾� ����
        players = null;
        players = FindObjectsOfType<PlayerController>();
    }

    public void Player_Dead_Check()
    {
        //����� �÷��̾� üũ�Ͽ� UI ���� ����
        for (int i = 0; i < players.Length; i++)
        {
            Player_Respawn_UI(i, players[i].isDead);
        }
    }
    public void Player_Respawn_UI(int i, bool isDead)
    {
        if (isDead)
        {
            //����� �÷��̾� UI ȸ������ ����
            if (players[i].player_Team.team == ETeam.Yellow)
            {
                Image img = yellow_Player_UI[i / 2].transform.parent.GetComponent<Image>();
                img.color = Color.gray;
            }
            else if (players[i].player_Team.team == ETeam.Blue)
            {
                Image img = blue_Player_UI[i / 2].transform.parent.GetComponent<Image>();
                img.color = Color.gray;
            }
        }
        else
        {
            //��Ȱ�� �÷��̾� UI ���� ������ ����
            if (players[i].player_Team.team == ETeam.Yellow)
            {
                Image img = yellow_Player_UI[i / 2].transform.parent.GetComponent<Image>();
                img.color = players[i].player_Team.team_Yellow;
            }
            else if (players[i].player_Team.team == ETeam.Blue)
            {
                Image img = blue_Player_UI[i / 2].transform.parent.GetComponent<Image>();
                img.color = players[i].player_Team.team_Blue;
            }
        }
    }

    public void SetPlayerPos()
    {
        //�÷��̾� ���� �� ���� �°� ���� ��ġ ����
        FindPlayer();
        int positionNum_Yellow = 0;
        int positionNum_Blue = 0;

        foreach (PlayerController player in players)
        {
            //�÷��̾� ui Ȱ��ȭ �� OnEnable�� �̿��� ���� �� ������ Ȯ�� ����
            player.UI_On_Off(true);
            player.gameObject.SetActive(false);
            player.gameObject.SetActive(true);
            //MapCam(false, player._player_shot.playerCam.cam_Obj);
        }

        //�� UI ����
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].player_Team.team == ETeam.Yellow)
            {
                //Yellow���� 5�� �̻����� ��Ī�� ��� �ش� �÷��̾��� �� ������ Blue������ ����
                if (positionNum_Yellow >= 4)
                {
                    players[i].player_Team.team = ETeam.Blue;
                    i--;
                    continue;
                }

                //�����ۿ� ���� UI����
                if (!isLobby)
                {
                    switch (players[i]._player_shot.WeaponType)
                    {
                        case EWeapon.Brush:
                            yellow_Player_UI[positionNum_Yellow].sprite = weapon_UI[0];
                            break;
                        case EWeapon.Gun:
                            yellow_Player_UI[positionNum_Yellow].sprite = weapon_UI[1];
                            break;
                        case EWeapon.Bow:
                            yellow_Player_UI[positionNum_Yellow].sprite = weapon_UI[2];
                            break;
                    }
                }

                //positionNum_Yellow ������ ���� ���� ��ġ �� �ش� UI ��ġ ����
                players[i].transform.position = team_Yellow_Spawn[positionNum_Yellow].position;
                players[i].transform.localRotation = Quaternion.identity;
                positionNum_Yellow++;
            }
            else
            {
                //5�� �̻��� Blue���� ��� Yellow������ ����
                if (positionNum_Blue >= 4)
                {
                    players[i].player_Team.team = ETeam.Yellow;
                    i--;
                    continue;
                }

                //�����ۿ� ���� UI����
                if (!isLobby)
                {
                    switch (players[i]._player_shot.WeaponType)
                    {
                        case EWeapon.Brush:
                            blue_Player_UI[positionNum_Blue].sprite = weapon_UI[0];
                            break;
                        case EWeapon.Gun:
                            blue_Player_UI[positionNum_Blue].sprite = weapon_UI[1];
                            break;
                        case EWeapon.Bow:
                            blue_Player_UI[positionNum_Blue].sprite = weapon_UI[2];
                            break;
                    }
                }

                //positionNum_Blue ������ ���� ���� ��ġ �� �ش� UI ��ġ ����
                players[i].transform.position = team_Blue_Spawn[positionNum_Blue].position;
                players[i].transform.localRotation = Quaternion.identity;
                players[i].transform.localRotation = Quaternion.Euler(0,180,0);
                positionNum_Blue++;
            }
        }
    }


    //MVP Player�� ������ ���� ��ųʸ� ������ �߰�
    public void List_In_Player(int score, PlayerController player_data)
    {
        player_Info[score] = new Player_Info(player_data.player_Team.team, player_data._player_shot.WeaponType,
            player_data.player_Input.player_Name, player_data._player_shot.player_ScoreSet);
    }

    //======================================================================  Time Late Method

    //���� ���� �� ������ �÷��̾� üũ
    public void Player_Count()
    {
        FindPlayer();
        if (PhotonNetwork.InRoom)
        {
            if (photonView.IsMine)
                Photon_Manager.Instance.photonView.RPC("MaxPlayer", RpcTarget.AllBuffered, Photon_Manager.Instance.max_Player);
            //��Ī �ο� ���� ���� �� �ڵ�����

            if (PhotonNetwork.CurrentRoom.PlayerCount >= Photon_Manager.Instance.max_Player)
            {
                Photon_Manager.Instance.set_Manager.LoadingOff();
                Photon_Manager.Instance.matching_UI.transform.parent.gameObject.SetActive(false);
                isLobby = false;
                isStart = false;
                player_Check = true;
            }

            //������ �ο� �� ��ŭ AI�� �����Ͽ� ����
            if (skip_Start)
            {
                Photon_Manager.Instance.set_Manager.LoadingOff();
                int Ai_Count = Photon_Manager.Instance.max_Player - PhotonNetwork.CurrentRoom.PlayerCount;

                int yellow = 0;
                int blue = 0;
                int num = 0;

                for (int i = 0; i < players.Length; i++) //�� �� �ʿ��� �ο� �� ���
                {
                    if (players[i].player_Team.team == ETeam.Blue) blue++;
                    else yellow++;
                }

                if (photonView.IsMine)
                {
                    while (true)
                    {
                        //AI ����
                        GameObject ai = PhotonNetwork.Instantiate(AI_Prefab.name, Vector3.zero, Quaternion.identity);
                        PlayerTeams ai_Team = ai.GetComponent<PlayerTeams>();

                        //������ AI�� �����ư��� �� ����
                        if (blue >= yellow)
                        {
                            if (num % 2 == 0)
                            {
                                ai_Team.team = ETeam.Yellow; // Blue���� Yellow�� ���� �ο��� ���� ��� Yellow�� ���� ����
                            }
                            if (num % 2 == 1)
                            {
                                ai_Team.team = ETeam.Blue;
                            }
                        }
                        else
                        {
                            if (num % 2 == 0)
                            {
                                ai_Team.team = ETeam.Blue; // Yellow���� Blue�� ���� �ο��� ���� ��� Blue�� ���� ����
                            }
                            if (num % 2 == 1)
                            {
                                ai_Team.team = ETeam.Yellow;
                            }
                        }

                        num++;

                        if (num == Ai_Count) //���� ���� �Ϸ� �� While�� ����
                        {
                            photonView.RPC("isLobby_Server", RpcTarget.AllBuffered);
                            skip_Start = false;
                            isStart = false;
                            player_Check = true;
                            return;
                        }
                    }
                }
            }
        }
    }
    public void StartCount() //Game Start CountDown
    {
        if (!isStart)
        {
            //���� ���� �� �ʱ�ȭ
            ui_Anim = GameObject.FindGameObjectWithTag("TimeUI").GetComponent<Animator>();
            Photon_Manager.Instance.set_Manager.LoadingOff();
            PaintTarget.ClearAllPaint(); 
            SetPlayerPos();
            BGM_Manager.Instance.Stop_All_Sound_BGM();

            foreach(TeamZone teamZone in nodes)
            {
                teamZone.team = ETeam.Etc;
            }

            count_Image.gameObject.SetActive(true); //ī��Ʈ �ٿ� �̹��� �ѱ�

            //���ھ� ������ �ʱ�ȭ
            scoreGage_Blue.fillAmount = 0; 
            scoreGage_Yellow.fillAmount = 0;
            isStart = true;
        }

        deltaTime -= Time.deltaTime;

        //���ӽ��� �� �÷��̾� ��� �� ī�޶� �ʱ�ȭ
        foreach (PlayerController player in players)
        {
            player.player_Input.fire = false;
            player.isStop = true;
            player._player_shot.playerCam.SelectCamera();

            if(player.photonView.IsMine && player._enemy == null)
            MapCam(false, player._player_shot.playerCam.cam_Obj.gameObject);
        }

        if (deltaTime <= 5 && deltaTime > 0) //CountDown Call
        {
            _BeepTime += Time.deltaTime;
            CountDown((int)deltaTime);
        }

        if (deltaTime <= 0) //GameStart Action
        {
            //�÷��̾� ��ġ �����
            SetPlayerPos();
            
            //ī��Ʈ �ٿ� ����
            ui_Anim.SetBool("Count", false);
            count_Image.gameObject.SetActive(false); 
            ui_Anim.SetTrigger("GameStart");

            //�÷��̾� ���� �Ϸ�
            foreach (PlayerController player in players)
            {
                player.gameObject.SetActive(false);
                player.gameObject.SetActive(true);

                player.isStop = false;

                if(player.photonView.IsMine && player._enemy == null)
                player._player_shot.name_UI.text = "������ �ܶ� Ȯ���ض�!";
            }

            BGM_Manager.Instance.Stop_All_Sound_BGM();
            BGM_Manager.Instance.Play_Sound_BGM("UI_Finish");
            BGM_Manager.Instance.Play_Sound_BGM("BGM_Game");

            deltaTime = endTimer;
            gameStart = true;
        }
    }

    //Game Ending
    public void EndCount() //Game End CountDown
    {
        deltaTime -= Time.deltaTime;

        //ȭ�� ��� Time UI �޼���
        TimeSet();


        if (deltaTime <= 0)
        {
            ui_Anim.SetBool("Count", false);
            ui_Anim.SetBool("TimeOut", true);
            StartCoroutine(GameStop());
            gameEnd = true;
        }
    }
    private void TimeSet() //Time UI Setting
    {
        // n : nn
        _Sec = (int)deltaTime % 60;
        _Min = (int)deltaTime / 60;
        timeText.text = $"{ _Min}:{_Sec.ToString("D2")}";

        //���� �ð� 1�� ���� �� �ð� �� ����
        if (deltaTime <= 61 && deltaTime > 10)
        {
            ui_Anim.SetBool("One_Min", true);
            timeText.color = players[0].player_Team.team_Yellow;
        }   
        
        //1 Min Warning
        if (deltaTime <= 11 && deltaTime > 1)
        {
            ui_Anim.SetBool("One_Min", false);
            count_Image.gameObject.SetActive(true);

            _BeepTime += Time.deltaTime;
            CountDown((int)deltaTime - 1);
        }    

        //10�� ī��Ʈ
        else if (deltaTime <= 1)
        {
            count_Image.gameObject.SetActive(false);
            ui_Anim.SetBool("Count", false);
            timeText.text = "END";
        }
    }

    //ī��Ʈ �ٿ� �ִϸ��̼� �� Sfx
    private void CountDown(int count) //Count Down Image
    {
        if (count < 11 && count > -1)
        {
            ui_Anim.SetBool("Count", true);
            Count_Image_Server(count);
        }
    }

    public void Count_Image_Server(int count)
    {
        count_Image.sprite = count_Sprite[count];

        if (_BeepTime >= 1)
        {
            BGM_Manager.Instance.Play_Sound_BGM("UI_Beep");
            _BeepTime = 0;
        }
    }

    //���� ���� �� 27%���� ������ ���� ����
    private void EndScoreCharge(bool call) 
    {
        if (call && _Charging_Score <= 27.3f)
        {
            _Charging_Score += Time.deltaTime * 35;

            scoreGage_Yellow.fillAmount = _Charging_Score / 100;
            scoreGage_Blue.fillAmount = _Charging_Score / 100;
            scoreCount_Yellow.text = $"{Mathf.Floor(_Charging_Score * 10f) / 10f}" + "%";
            scoreCount_Blue.text = $"{Mathf.Floor(_Charging_Score * 10f) / 10f}" + "%";
        }
        else return;
    }

    //======================================================================  Ending Method
    public IEnumerator GameStop()
    {
        //���� ���� �� ������ �÷��̾� ��Ȯ��
        FindPlayer();
        BGM_Manager.Instance.Stop_All_Sound_BGM();
        BGM_Manager.Instance.Play_Sound_BGM("UI_Finish");

        //�÷��̾� ��������
        foreach (PlayerController player in players)
        {
            player.player_Input.fire = false;
            player.isStop = true;
        }

        yield return new WaitForSeconds(5f); //���̵� �� / �ƿ�

        deadLine.enabled = true; 
        ui_Anim.SetBool("TimeOut", false);
        teamYellow_Anim.gameObject.SetActive(true);
        teamBlue_Anim.gameObject.SetActive(true);

        //Player ī�޶� ���� 
        foreach (PlayerController player in players)
        {
            player.UI_On_Off(false);

            if(player._enemy == null )
            {
                if (player.photonView.IsMine)
                {
                    MapCam(true, player._player_shot.playerCam.cam_Obj.gameObject);
                    Save_Data(player);
                }
            }
        } 

        
        yield return new WaitForSeconds(3f); //UI Call
        ui_Anim.SetBool("Score", true);
        manager_Anim.SetBool("GameEnd", true);

        yield return new WaitForSeconds(1f); //Score Gage Charged

        //27% ���� ����
        BGM_Manager.Instance.Play_Sound_BGM("UI_Gage");
        chargeCall = true;

        yield return new WaitForSeconds(3f); //�¸� �� ���

        //27% ���� ����
        chargeCall = false;
        BGM_Manager.Instance.Stop_All_Sound_BGM();
        BGM_Manager.Instance.Play_Sound_BGM("UI_Splash");

        //�� �� ������ 1% ���
        float color_Count = (Check_Color(ETeam.Blue) + Check_Color(ETeam.Yellow)) / 100;

        //���� 1%�� ���� �� ���� ��
        float yellow_Score = Mathf.Floor(((Check_Color(ETeam.Yellow) / color_Count)) * 10f) / 10f;
        float blue_Score = Mathf.Floor(((Check_Color(ETeam.Blue) / color_Count)) * 10f) / 10f;

        //UI ����
        scoreGage_Yellow.fillAmount = yellow_Score / 100;
        scoreGage_Blue.fillAmount = blue_Score / 100;
        scoreCount_Yellow.text = $"{yellow_Score}" + "%";
        scoreCount_Blue.text = $"{blue_Score}" + "%";

        //�¸� �� ����
        if ( Check_Color(ETeam.Yellow) > Check_Color(ETeam.Blue))
        {
            teamYellow_Anim.Win();
            teamBlue_Anim.Lose();
            yellow_WinEffect.Play();
        }
        else //��� �� �¸�
        {
            teamBlue_Anim.Win();
            teamYellow_Anim.Lose();
            blue_WinEffect.Play();
        }

        yield return new WaitForSeconds(0.5f);
        ui_Anim.SetBool("Score", false);
        BGM_Manager.Instance.Play_Sound_BGM("BGM_Victory");

        //===========================================================================================
        //MVP Player ����

        int[] player_Score = new int[players.Length];

        for (int i = 0; i < players.Length; i++)
        {
            List_In_Player(players[i]._player_shot.player_ScoreSet, players[i]);
            player_Score[i] = players[i]._player_shot.player_ScoreSet;
        }

        //���� ū ���ھ�
        int mostScore = player_Score.Max();

        //���� ū ���ھ key������ ��ųʸ� �˻�
        player_Data = player_Info[mostScore];

        mvp_Data[0].text = player_Data.name;
        
        if(player_Data.team == ETeam.Yellow)
        {
            mvp_Data[1].text = "Yellow";
            mvp_Data[1].color = players[0].player_Team.team_Yellow;
            mvp_Model.TeamChange(ETeam.Yellow);
        }
        else
        {
            mvp_Data[1].text = "Blue";
            mvp_Data[1].color = players[0].player_Team.team_Blue;
            mvp_Model.TeamChange(ETeam.Blue);
        }
        switch (player_Data.weapon)
        {
            case EWeapon.Brush:
                mvp_Data[2].text = "ȣ�����";
                break;   
            case EWeapon.Gun:
                mvp_Data[2].text = "���� ����";
                break;   
            case EWeapon.Bow:
                mvp_Data[2].text = "Ʈ���� ��Ʈ����";
                break;
        }
        mvp_Data[3].text = player_Data.score.ToString("D4");

        yield return new WaitForSeconds(2f);

        manager_Anim.SetBool("GameEnd", false);
        ui_Anim.SetBool("isMVP", true);
        mvp_Anim.SetBool("Dance", true);

        //=================================================== ������ ��� Scene���� �̵�

        yield return new WaitForSeconds(7f);
        ui_Anim.SetBool("FadeOut", true);
        yield return new WaitForSeconds(2f);
        Photon_Manager.Instance.DisConnect();
        SceneManager.LoadScene("ScoreLobby");
    }

    //======================================================================  ETC
    public float Check_Color(ETeam team)
    {
        //�÷� ��� ���� Ȯ�� �޼���
        int teamScore = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].team == team)
                teamScore++;
        }
        return teamScore;
    } 

    public void SetCursorState(bool newState) //Ŀ�� ���
    {
       Cursor.lockState = newState ? CursorLockMode.Confined : CursorLockMode.None;
    }

    public void MapCam(bool camOn, GameObject playerCam)
    {
        //ī�޶� ��ȯ
        if (camOn)
        {
            playerCam.SetActive(false);
            map_Camera.SetActive(true);
        }
        else
        {
            map_Camera.SetActive(false);
            playerCam.SetActive(true);

        }
    } //ī�޶� �޼���

    [PunRPC]
    public void isLobby_Server()
    {
        isLobby = false;
    }

     public void Save_Data(PlayerController player)
     {
        if (photonView.IsMine)
        {
            PlayerPrefs.DeleteAll();
            //��ųʸ��� ���� Ű,������ �����ϱ⿡ Ű�� ���� �������
            PlayerPrefs.SetString("Name", player.player_Input.player_Name);
            PlayerPrefs.SetString("Score", player._player_shot.player_ScoreSet.ToString());
            PlayerPrefs.SetString("Weapon", player._player_shot.weapon.weaponType.ToString());
            PlayerPrefs.SetString("Team", player.player_Team.team.ToString());
        }
    }

}
