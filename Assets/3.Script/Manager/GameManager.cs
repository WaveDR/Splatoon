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
    //현재 참가 중인 플레이어 값
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

    //============================================        ↑ CallBack   |   Nomal ↓        ========================================================
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

        deadLine.enabled = false; //데드라인 메쉬 비활성화
        deltaTime = startTimer; //시작 전 카운트  
    }
    
    public void FindPlayer()
    {
        //플레이어 색인
        players = null;
        players = FindObjectsOfType<PlayerController>();
    }

    public void Player_Dead_Check()
    {
        //사망한 플레이어 체크하여 UI 변경 연계
        for (int i = 0; i < players.Length; i++)
        {
            Player_Respawn_UI(i, players[i].isDead);
        }
    }
    public void Player_Respawn_UI(int i, bool isDead)
    {
        if (isDead)
        {
            //사망한 플레이어 UI 회색으로 변경
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
            //부활한 플레이어 UI 원래 색으로 복구
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
        //플레이어 색인 후 팀에 맞게 스폰 위치 배정
        FindPlayer();
        int positionNum_Yellow = 0;
        int positionNum_Blue = 0;

        foreach (PlayerController player in players)
        {
            //플레이어 ui 활성화 및 OnEnable을 이용한 정보 값 재적용 확인 절차
            player.UI_On_Off(true);
            player.gameObject.SetActive(false);
            player.gameObject.SetActive(true);
            //MapCam(false, player._player_shot.playerCam.cam_Obj);
        }

        //팀 UI 배정
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].player_Team.team == ETeam.Yellow)
            {
                //Yellow팀이 5명 이상으로 매칭될 경우 해당 플레이어의 팀 진영을 Blue팀으로 변경
                if (positionNum_Yellow >= 4)
                {
                    players[i].player_Team.team = ETeam.Blue;
                    i--;
                    continue;
                }

                //아이템에 따른 UI변경
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

                //positionNum_Yellow 변수에 따라 스폰 위치 및 해당 UI 위치 변경
                players[i].transform.position = team_Yellow_Spawn[positionNum_Yellow].position;
                players[i].transform.localRotation = Quaternion.identity;
                positionNum_Yellow++;
            }
            else
            {
                //5인 이상이 Blue팀일 경우 Yellow팀으로 변경
                if (positionNum_Blue >= 4)
                {
                    players[i].player_Team.team = ETeam.Yellow;
                    i--;
                    continue;
                }

                //아이템에 따른 UI변경
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

                //positionNum_Blue 변수에 따라 스폰 위치 및 해당 UI 위치 변경
                players[i].transform.position = team_Blue_Spawn[positionNum_Blue].position;
                players[i].transform.localRotation = Quaternion.identity;
                players[i].transform.localRotation = Quaternion.Euler(0,180,0);
                positionNum_Blue++;
            }
        }
    }


    //MVP Player를 가리기 위한 딕셔너리 정보값 추가
    public void List_In_Player(int score, PlayerController player_data)
    {
        player_Info[score] = new Player_Info(player_data.player_Team.team, player_data._player_shot.WeaponType,
            player_data.player_Input.player_Name, player_data._player_shot.player_ScoreSet);
    }

    //======================================================================  Time Late Method

    //게임 시작 전 입장한 플레이어 체크
    public void Player_Count()
    {
        FindPlayer();
        if (PhotonNetwork.InRoom)
        {
            if (photonView.IsMine)
                Photon_Manager.Instance.photonView.RPC("MaxPlayer", RpcTarget.AllBuffered, Photon_Manager.Instance.max_Player);
            //매칭 인원 전원 입장 시 자동시작

            if (PhotonNetwork.CurrentRoom.PlayerCount >= Photon_Manager.Instance.max_Player)
            {
                Photon_Manager.Instance.set_Manager.LoadingOff();
                Photon_Manager.Instance.matching_UI.transform.parent.gameObject.SetActive(false);
                isLobby = false;
                isStart = false;
                player_Check = true;
            }

            //부족한 인원 수 만큼 AI를 생성하여 시작
            if (skip_Start)
            {
                Photon_Manager.Instance.set_Manager.LoadingOff();
                int Ai_Count = Photon_Manager.Instance.max_Player - PhotonNetwork.CurrentRoom.PlayerCount;

                int yellow = 0;
                int blue = 0;
                int num = 0;

                for (int i = 0; i < players.Length; i++) //팀 당 필요한 인원 수 계산
                {
                    if (players[i].player_Team.team == ETeam.Blue) blue++;
                    else yellow++;
                }

                if (photonView.IsMine)
                {
                    while (true)
                    {
                        //AI 생성
                        GameObject ai = PhotonNetwork.Instantiate(AI_Prefab.name, Vector3.zero, Quaternion.identity);
                        PlayerTeams ai_Team = ai.GetComponent<PlayerTeams>();

                        //생성된 AI를 번갈아가며 팀 배정
                        if (blue >= yellow)
                        {
                            if (num % 2 == 0)
                            {
                                ai_Team.team = ETeam.Yellow; // Blue팀이 Yellow팀 보다 인원이 많을 경우 Yellow팀 먼저 시작
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
                                ai_Team.team = ETeam.Blue; // Yellow팀이 Blue팀 보다 인원이 많을 경우 Blue팀 먼저 시작
                            }
                            if (num % 2 == 1)
                            {
                                ai_Team.team = ETeam.Yellow;
                            }
                        }

                        num++;

                        if (num == Ai_Count) //적정 생성 완료 시 While문 종료
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
            //게임 시작 전 초기화
            ui_Anim = GameObject.FindGameObjectWithTag("TimeUI").GetComponent<Animator>();
            Photon_Manager.Instance.set_Manager.LoadingOff();
            PaintTarget.ClearAllPaint(); 
            SetPlayerPos();
            BGM_Manager.Instance.Stop_All_Sound_BGM();

            foreach(TeamZone teamZone in nodes)
            {
                teamZone.team = ETeam.Etc;
            }

            count_Image.gameObject.SetActive(true); //카운트 다운 이미지 켜기

            //스코어 게이지 초기화
            scoreGage_Blue.fillAmount = 0; 
            scoreGage_Yellow.fillAmount = 0;
            isStart = true;
        }

        deltaTime -= Time.deltaTime;

        //게임시작 전 플레이어 대기 및 카메라 초기화
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
            //플레이어 위치 재배정
            SetPlayerPos();
            
            //카운트 다운 종료
            ui_Anim.SetBool("Count", false);
            count_Image.gameObject.SetActive(false); 
            ui_Anim.SetTrigger("GameStart");

            //플레이어 세팅 완료
            foreach (PlayerController player in players)
            {
                player.gameObject.SetActive(false);
                player.gameObject.SetActive(true);

                player.isStop = false;

                if(player.photonView.IsMine && player._enemy == null)
                player._player_shot.name_UI.text = "영역을 잔뜩 확보해라!";
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

        //화면 상단 Time UI 메서드
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

        //남은 시간 1분 이하 시 시계 색 변경
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

        //10초 카운트
        else if (deltaTime <= 1)
        {
            count_Image.gameObject.SetActive(false);
            ui_Anim.SetBool("Count", false);
            timeText.text = "END";
        }
    }

    //카운트 다운 애니메이션 및 Sfx
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

    //게임 종료 후 27%까지 게이지 충전 연출
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
        //게임 시작 후 입장한 플레이어 재확인
        FindPlayer();
        BGM_Manager.Instance.Stop_All_Sound_BGM();
        BGM_Manager.Instance.Play_Sound_BGM("UI_Finish");

        //플레이어 동작정지
        foreach (PlayerController player in players)
        {
            player.player_Input.fire = false;
            player.isStop = true;
        }

        yield return new WaitForSeconds(5f); //페이드 인 / 아웃

        deadLine.enabled = true; 
        ui_Anim.SetBool("TimeOut", false);
        teamYellow_Anim.gameObject.SetActive(true);
        teamBlue_Anim.gameObject.SetActive(true);

        //Player 카메라 설정 
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

        //27% 연출 시작
        BGM_Manager.Instance.Play_Sound_BGM("UI_Gage");
        chargeCall = true;

        yield return new WaitForSeconds(3f); //승리 팀 계산

        //27% 연출 종료
        chargeCall = false;
        BGM_Manager.Instance.Stop_All_Sound_BGM();
        BGM_Manager.Instance.Play_Sound_BGM("UI_Splash");

        //각 팀 점수의 1% 계산
        float color_Count = (Check_Color(ETeam.Blue) + Check_Color(ETeam.Yellow)) / 100;

        //계산된 1%의 적용 후 곱한 값
        float yellow_Score = Mathf.Floor(((Check_Color(ETeam.Yellow) / color_Count)) * 10f) / 10f;
        float blue_Score = Mathf.Floor(((Check_Color(ETeam.Blue) / color_Count)) * 10f) / 10f;

        //UI 적용
        scoreGage_Yellow.fillAmount = yellow_Score / 100;
        scoreGage_Blue.fillAmount = blue_Score / 100;
        scoreCount_Yellow.text = $"{yellow_Score}" + "%";
        scoreCount_Blue.text = $"{blue_Score}" + "%";

        //승리 팀 선별
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

        //===========================================================================================
        //MVP Player 색인

        int[] player_Score = new int[players.Length];

        for (int i = 0; i < players.Length; i++)
        {
            List_In_Player(players[i]._player_shot.player_ScoreSet, players[i]);
            player_Score[i] = players[i]._player_shot.player_ScoreSet;
        }

        //가장 큰 스코어
        int mostScore = player_Score.Max();

        //가장 큰 스코어를 key값으로 딕셔너리 검색
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

        //=================================================== 끝내고 결과 Scene으로 이동

        yield return new WaitForSeconds(7f);
        ui_Anim.SetBool("FadeOut", true);
        yield return new WaitForSeconds(2f);
        Photon_Manager.Instance.DisConnect();
        SceneManager.LoadScene("ScoreLobby");
    }

    //======================================================================  ETC
    public float Check_Color(ETeam team)
    {
        //컬러 노드 갯수 확인 메서드
        int teamScore = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].team == team)
                teamScore++;
        }
        return teamScore;
    } 

    public void SetCursorState(bool newState) //커서 잠금
    {
       Cursor.lockState = newState ? CursorLockMode.Confined : CursorLockMode.None;
    }

    public void MapCam(bool camOn, GameObject playerCam)
    {
        //카메라 전환
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
    public void isLobby_Server()
    {
        isLobby = false;
    }

     public void Save_Data(PlayerController player)
     {
        if (photonView.IsMine)
        {
            PlayerPrefs.DeleteAll();
            //딕셔너리와 유사 키,값으로 저장하기에 키를 먼저 만들어줌
            PlayerPrefs.SetString("Name", player.player_Input.player_Name);
            PlayerPrefs.SetString("Score", player._player_shot.player_ScoreSet.ToString());
            PlayerPrefs.SetString("Weapon", player._player_shot.weapon.weaponType.ToString());
            PlayerPrefs.SetString("Team", player.player_Team.team.ToString());
        }
    }

}
