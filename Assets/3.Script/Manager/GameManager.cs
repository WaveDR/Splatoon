using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using Photon.Pun;
using Photon.Realtime;


[Serializable]
public class Player_Info
{
    public Player_Info(ETeam _team, EWeapon _weapon, string _name, int _score) { team = _team; weapon = _weapon; name = _name; score = _score; }
    public ETeam team;
    public EWeapon weapon;
    public string name;
    public int score;
}
public class GameManager : MonoBehaviourPunCallbacks
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
    public List<TeamZone> nodes = new List<TeamZone>();
    public GameObject map_Camera;

    [Header("Player")]
    public PlayerController[] players;
    public Player_Info player_Data;

    public Dictionary<int, Player_Info> player_Info = new Dictionary<int, Player_Info>();
    [SerializeField] private Player_MVP mvp_Model;

    [Header("UI")]
    [SerializeField] private Text timeText;
   //[SerializeField] private Text timeText_Mid;
   //[SerializeField] private Text timeText_Min;

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
    [SerializeField] private Animator ui_Anim;
    [SerializeField] private Animator manager_Anim;
    [SerializeField] private Animator mvp_Anim;
    [SerializeField] private WinAnim teamYellow_Anim;
    [SerializeField] private WinAnim teamBlue_Anim;


    [Header("Timer")]
    [SerializeField] private float startTimer = 10;
    [SerializeField] private float endTimer = 180;

    private int _Min;
    private int _Sec;
    private float _Time;
    private float _BeepTime;
    private float _Charging_Score;
    public float deltaTime
    {
        get { return _Time; }
        set { _Time = value;
            _Time = Mathf.Clamp(_Time, 0, endTimer);
        }
    }
    private bool chargeCall;
    private bool isStart;

    public bool isLobby;
    
    [Header("SpawnPos")]
    [SerializeField] private Vector3[] _team_Yellow_Spawn;
    [SerializeField] private Vector3[] _team_Blue_Spawn;

    public Vector3[] team_Yellow_Spawn => _team_Yellow_Spawn;
    public Vector3[] team_Blue_Spawn => _team_Blue_Spawn;
    public MeshRenderer deadLine;
    public bool gameStart;
    public bool gameEnd;

    [Header("Particle")]
    [SerializeField] private ParticleSystem yellow_WinEffect;
    [SerializeField] private ParticleSystem blue_WinEffect;

    private void Awake()
    {
        Manager_Server();
    }
    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;
        if (!isLobby)
        {
            if (Input.GetKey(KeyCode.Escape)) SetCursorState(false);

            else SetCursorState(true);

            if (!gameStart) StartCount();
            else if (!gameEnd) EndCount();

            EndScoreCharge(chargeCall);
        }
    }

    //============================================        ↑ CallBack   |   Nomal ↓        ========================================================

    [PunRPC]
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

        //Set Pos

        _team_Yellow_Spawn = new Vector3[4];
        _team_Blue_Spawn = new Vector3[4];

        _team_Yellow_Spawn[0] = new Vector3(-5.46f, 3.6f, -60);
        _team_Yellow_Spawn[1] = new Vector3(-1.46f, 3.6f, -60);
        _team_Yellow_Spawn[2] = new Vector3(2.3f, 3.6f, -60);
        _team_Yellow_Spawn[3] = new Vector3(6.04f, 3.6f, -60);

        _team_Blue_Spawn[0] = new Vector3(-6.34f, 3.6f, 60);
        _team_Blue_Spawn[1] = new Vector3(-2.34f, 3.6f, 60);
        _team_Blue_Spawn[2] = new Vector3(1.41f, 3.6f, 60);
        _team_Blue_Spawn[3] = new Vector3(5.16f, 3.6f, 60);

        deadLine.enabled = false; //데드라인 메쉬 비활성화
        deltaTime = startTimer; //시작 전 카운트  
    }
    
    public void FindPlayer()
    {
        players = null;
        players = FindObjectsOfType<PlayerController>();
    }

    public void Player_Dead_Check()
    {
        for (int i = 0; i < players.Length; i++)
        {
            Player_Respawn_UI(i, players[i].isDead);
        }
    }
    public void Player_Respawn_UI(int i, bool isDead)
    {
        if (isDead)
        {
            if (players[i].player_Team.team == ETeam.Yellow)
            {
                Image img = yellow_Player_UI[i].transform.parent.GetComponent<Image>();
                img.color = Color.gray;
            }
            else if (players[i].player_Team.team == ETeam.Blue)
            {
                Image img = blue_Player_UI[i].transform.parent.GetComponent<Image>();
                img.color = Color.gray;
            }
        }
        else
        {
            if (players[i].player_Team.team == ETeam.Yellow)
            {
                Image img = yellow_Player_UI[i].transform.parent.GetComponent<Image>();
                img.color = players[i].player_Team.team_Yellow;
            }
            else if (players[i].player_Team.team == ETeam.Blue)
            {
                Image img = blue_Player_UI[i].transform.parent.GetComponent<Image>();
                img.color = players[i].player_Team.team_Blue;
            }
        }
  
    }
    public void SetPlayerPos()
    {
        FindPlayer();
        int positionNum_Yellow = 0;
        int positionNum_Blue = 0;

        foreach (PlayerController player in players)
        {
            player.UI_OnOFf(true);
            //MapCam(false, player._player_shot.playerCam.cam_Obj);
        }

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].player_Team.team == ETeam.Yellow)
            {
                if (i >= 5)
                {
                    players[i].player_Team.team = ETeam.Blue;
                    i--;
                    continue;
                }

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
           

                players[i].transform.position = team_Yellow_Spawn[positionNum_Yellow];
                players[i].transform.localRotation = Quaternion.identity;
                positionNum_Yellow++;
            }
            else
            {
                if (i >= 5)
                {
                    players[i].player_Team.team = ETeam.Yellow;
                    i--;
                    continue;
                }

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
                players[i].transform.position = team_Blue_Spawn[positionNum_Blue];
                players[i].transform.localRotation = Quaternion.identity;
                players[i].transform.localRotation = Quaternion.Euler(0,180,0);
                positionNum_Blue++;
            }
        }
    }


    public void List_In_Player(int score, PlayerController player_data)
    {
        player_Info[score] = new Player_Info(player_data.player_Team.team, player_data._player_shot.WeaponType,
            player_data.player_Input.player_Name, player_data._player_shot.player_ScoreSet);
    }

    //======================================================================  Time Late Method

    public void StartCount() //Game Start CountDown
    {
        if (!isStart)
        {
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount >= Photon_Manager.Instance.max_Player)
                {
                    isLobby = false;
                }
            }

            ui_Anim = GameObject.FindGameObjectWithTag("TimeUI").GetComponent<Animator>();

            PaintTarget.ClearAllPaint();

            SetPlayerPos();
            photonView.RPC("UI_Out", RpcTarget.AllBuffered);
 
            BGM_Manager.Instance.Stop_All_Sound_BGM();

            foreach(TeamZone teamZone in nodes)
            {
                teamZone.team = ETeam.Etc;
            }

            count_Image.gameObject.SetActive(true); //카운트 다운 이미지 켜기
            scoreGage_Blue.fillAmount = 0; //스코어 게이지 초기화
            scoreGage_Yellow.fillAmount = 0;
            isStart = true;
        }

        deltaTime -= Time.deltaTime;

        foreach (PlayerController player in players)
        {
            player.player_Input.fire = false;
            player.isStop = true;
            player._player_shot.playerCam.SelectCamera();
            MapCam(false, player._player_shot.playerCam.cam_Obj.gameObject);
        }
        //Player Move Limit

        if (deltaTime <= 5 && deltaTime > 0) //CountDown Call
        {
            _BeepTime += Time.deltaTime;
            CountDown((int)deltaTime);
        }

        if (deltaTime <= 0) //GameStart Action
        {
            FindPlayer();
            ui_Anim.SetBool("Count", false);
            count_Image.gameObject.SetActive(false);
            ui_Anim.SetTrigger("GameStart");

            deltaTime = endTimer;

            foreach (PlayerController player in players)
            {
                player.isStop = false;
                player._player_shot.name_UI.text = "영역을 잔뜩 확보해라!";
            }

            BGM_Manager.Instance.Stop_All_Sound_BGM();
            BGM_Manager.Instance.Play_Sound_BGM("UI_Finish");
            BGM_Manager.Instance.Play_Sound_BGM("BGM_Game");
            gameStart = true;
        }
    }

    public void EndCount() //Game End CountDown
    {
        deltaTime -= Time.deltaTime;

        TimeSet();
        //TimeSet();

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
        _Sec = (int)deltaTime % 60;
        _Min = (int)deltaTime / 60;
        timeText.text = $"{ _Min}:{_Sec.ToString("D2")}";

        if (deltaTime <= 61 && deltaTime > 10)
        {
            ui_Anim.SetBool("One_Min", true);
            timeText.color = players[0].player_Team.team_Yellow;
        }   //1 Min Warning
        if (deltaTime <= 11 && deltaTime > 1)
        {
            ui_Anim.SetBool("One_Min", false);
            count_Image.gameObject.SetActive(true);

            _BeepTime += Time.deltaTime;
            CountDown((int)deltaTime - 1);
        }    //10 Sec. NumColor = Yellow
        else if (deltaTime <= 1)
        {
            count_Image.gameObject.SetActive(false);
            ui_Anim.SetBool("Count", false);
            timeText.text = "END";
        } //TimeSet Exit
    }

    
    private void CountDown(int count) //Count Down Image
    {
        if (count < 11 && count > -1)
        {
            ui_Anim.SetBool("Count", true);
            Count_Image_Server(count);
        }
    }
    [PunRPC]
    public void Count_Image_Server(int count)
    {
        count_Image.sprite = count_Sprite[count];

        if (_BeepTime >= 1)
        {
            BGM_Manager.Instance.Play_Sound_BGM("UI_Beep");
            _BeepTime = 0;
        }
    }
    private void EndScoreCharge(bool call) //27.7% Production
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
        FindPlayer();
        BGM_Manager.Instance.Stop_All_Sound_BGM();
        BGM_Manager.Instance.Play_Sound_BGM("UI_Finish");
        //화면 전환되는 시간
        foreach (PlayerController player in players)
        {
            player.player_Input.fire = false;
            player.isStop = true;
        }
        yield return new WaitForSeconds(5f); //맵 확인하는 시간

        deadLine.enabled = true;
        ui_Anim.SetBool("TimeOut", false);
        teamYellow_Anim.gameObject.SetActive(true);
        teamBlue_Anim.gameObject.SetActive(true);

        foreach (PlayerController player in players)
        {
            player.UI_OnOFf(false);
            MapCam(true, player._player_shot.playerCam.cam_Obj.gameObject);
        } //맵캠으로 변경 / 플레이어 ui 비활성화

        
        yield return new WaitForSeconds(3f); //UI Call
        ui_Anim.SetBool("Score", true);
        manager_Anim.SetBool("GameEnd", true);

        yield return new WaitForSeconds(1f); //Score Gage Charged
        BGM_Manager.Instance.Play_Sound_BGM("UI_Gage");
        chargeCall = true;
        

        yield return new WaitForSeconds(3f); //Result
        chargeCall = false;
        BGM_Manager.Instance.Stop_All_Sound_BGM();
        BGM_Manager.Instance.Play_Sound_BGM("UI_Splash");

        float color_Count = (Check_Color(ETeam.Blue) + Check_Color(ETeam.Yellow)) / 100;
        float yellow_Score = Mathf.Floor(((Check_Color(ETeam.Yellow) / color_Count)) * 10f) / 10f;
        float blue_Score = Mathf.Floor(((Check_Color(ETeam.Blue) / color_Count)) * 10f) / 10f;

        scoreGage_Yellow.fillAmount = yellow_Score / 100;
        scoreGage_Blue.fillAmount = blue_Score / 100;
        scoreCount_Yellow.text = $"{yellow_Score}" + "%";
        scoreCount_Blue.text = $"{blue_Score}" + "%";

        //점수 확인
        if ( Check_Color(ETeam.Yellow) > Check_Color(ETeam.Blue))
        {
            teamYellow_Anim.Win();
            teamBlue_Anim.Lose();
            yellow_WinEffect.Play();
        }
        else //블루 팀 승리
        {
            teamBlue_Anim.Win();
            teamYellow_Anim.Lose();
            blue_WinEffect.Play();
        }

        yield return new WaitForSeconds(0.5f);
        ui_Anim.SetBool("Score", false);
        BGM_Manager.Instance.Play_Sound_BGM("BGM_Victory");
        //Player Data Setup
        int[] player_Score = new int[players.Length];

        for (int i = 0; i < players.Length; i++)
        {
            List_In_Player(players[i]._player_shot.player_ScoreSet, players[i]);
            player_Score[i] = players[i]._player_shot.player_ScoreSet;
        }

        int mostScore = player_Score.Max();

        Debug.Log(mostScore);

        Debug.Log(player_Info[mostScore].team);
        Debug.Log(player_Info[mostScore].weapon);
        Debug.Log(player_Info[mostScore].name);
        Debug.Log(player_Info[mostScore].score);
        // Debug.Log(mostScore);

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
                mvp_Data[2].text = "호쿠사이";
                break;   
            case EWeapon.Gun:
                mvp_Data[2].text = "새싹 슈터";
                break;   
            case EWeapon.Bow:
                mvp_Data[2].text = "트라이 스트링거";
                break;
        }
        mvp_Data[3].text = player_Data.score.ToString("D4");

        yield return new WaitForSeconds(2f);

        manager_Anim.SetBool("GameEnd", false);
        ui_Anim.SetBool("isMVP", true);
        mvp_Anim.SetBool("Dance", true);
    }

    //======================================================================  ETC
    public float Check_Color(ETeam team)
    {
        int teamScore = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].team == team)
                teamScore++;
        }
        return teamScore;
    } //컬러 노드 갯수 확인 메서드

    public void SetCursorState(bool newState) //커서 잠금
    {
       //Cursor.lockState = newState ? CursorLockMode.Confined : CursorLockMode.None;
    }

    public void MapCam(bool camOn, GameObject playerCam)
    {
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
    } //카메라 메서드

    [PunRPC]
    public void UI_Out()
    {
        Photon_Manager.Instance.matching_UI.transform.parent.gameObject.SetActive(false);
    }
}
