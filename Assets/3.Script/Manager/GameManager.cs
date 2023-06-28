using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
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
    public PlayerController[] players;

    [Header("UI")]
    [SerializeField] private Text timeText_Sec;
    [SerializeField] private Text timeText_Mid;
    [SerializeField] private Text timeText_Min;

    [SerializeField] private Sprite[] count_Sprite;
    [SerializeField] private Image count_Image;

    [SerializeField] private Image scoreGage_Yellow;
    [SerializeField] private Image scoreGage_Blue;


    [Header("Character Anim")]
    [SerializeField] private Animator ui_Anim;
    [SerializeField] private Animator manager_Anim;
    [SerializeField] private WinAnim teamYellow_Anim;
    [SerializeField] private WinAnim teamBlue_Anim;


    [Header("Timer")]
    [SerializeField] private float startTimer = 10;
    [SerializeField] private float endTimer = 180;

    private int _Min;
    private int _Sec;
    private float _Time;
    private float deltaTime
    {
        get { return _Time; }
        set { _Time = value;
            _Time = Mathf.Clamp(_Time, 0, endTimer);
        }
    }
    
    [Header("SpawnPos")]
    public Vector3 yellowSpawn = new Vector3(0,3.6f,-60f);
    public Vector3 blueSpawn = new Vector3(0, 3.6f, 60f);
    public MeshRenderer deadLine;
    public bool gameStart;
    public bool gameEnd;

    private void Awake()
    {
        //GetComponent
        deadLine = GameObject.FindGameObjectWithTag("DeadLine").GetComponent<MeshRenderer>();
        map_Camera = GameObject.FindGameObjectWithTag("MapCamera");
        ui_Anim = GameObject.FindGameObjectWithTag("TimeUI").GetComponent<Animator>();
        players = FindObjectsOfType<PlayerController>();

        //Animator
        TryGetComponent(out manager_Anim);
        teamYellow_Anim = transform.GetChild(0).GetComponent<WinAnim>();
        teamBlue_Anim = transform.GetChild(1).GetComponent<WinAnim>();
        teamYellow_Anim.gameObject.SetActive(false);
        teamBlue_Anim.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        deltaTime = startTimer; //시작 전 카운트 
        deadLine.enabled = false; //데드라인 메쉬 비활성화
        count_Image.gameObject.SetActive(true); //카운트 다운 이미지 켜기

        scoreGage_Blue.fillAmount = 0; //스코어 게이지 초기화
        scoreGage_Yellow.fillAmount = 0;
    }

    private void Start()
    {
        foreach(PlayerController player in players)
        {
            player.UI_OnOFf(true);
            MapCam(false, player._player_shot.playerCam.cam_Obj);
        }
    } //Cam 초기화

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            SetCursorState(false);
        }
        else
        {
            SetCursorState(true);
        }

        if (!gameStart)
        {
            StartCount();
        }
        else if (!gameEnd)
        {
            EndCount();
        }
    }

    private void TimeSet()
    {
        _Sec = (int)deltaTime % 60;
        _Min = (int)deltaTime / 60;

        timeText_Sec.text = _Sec.ToString("D2");
        timeText_Min.text = _Min.ToString();

        if(deltaTime <= 61 && deltaTime >10)
        {
            ui_Anim.SetBool("One_Min",true);
            timeText_Min.color = players[0].player_Team.team_Yellow;
            timeText_Mid.color = players[0].player_Team.team_Yellow;
            timeText_Sec.color = players[0].player_Team.team_Yellow;
        }
        if (deltaTime <= 11 && deltaTime > 1)
        {
            ui_Anim.SetBool("One_Min", false);
            count_Image.gameObject.SetActive(true);
            CountDown((int)deltaTime -1);
        }
        else if(deltaTime <= 1)
        {
            count_Image.gameObject.SetActive(false);
            ui_Anim.SetBool("Count", false); 
        }
    }

    private void CountDown(int count)
    {
        if(count <11 && count > -1)
        {
            ui_Anim.SetBool("Count", true);
            count_Image.sprite = count_Sprite[count];
        }
    }

    public void StartCount()
    {
        deltaTime -= Time.deltaTime;
        
        foreach (PlayerController player in players)
        {
            player.isStop = true;
        }

        if (deltaTime <= 10 && deltaTime > 0)
        {
            CountDown((int)deltaTime);
        }

        if (deltaTime <= 0)
        {
            ui_Anim.SetBool("Count", false);
            count_Image.gameObject.SetActive(false);
            ui_Anim.SetTrigger("GameStart");
            deltaTime = endTimer;
            gameStart = true;
            foreach (PlayerController player in players)
            {
                player.isStop = false;
            }
        }
    }
    public void EndCount()
    {
        deltaTime -= Time.deltaTime;
        TimeSet();

        if (deltaTime <= 0)
        {
            ui_Anim.SetBool("Count", false);
            ui_Anim.SetBool("TimeOut", true);

            StartCoroutine(GameStop());
            gameEnd = true;
        }
    }

    public IEnumerator GameStop()
    {
        foreach (PlayerController player in players)
        {
            player.isStop = true;
        }
        yield return new WaitForSeconds(5f); //화면 전환되는 시간

        deadLine.enabled = true;
        ui_Anim.SetBool("TimeOut", false);

        teamYellow_Anim.gameObject.SetActive(true);
        teamBlue_Anim.gameObject.SetActive(true);


        foreach (PlayerController player in players)
        {
            player.UI_OnOFf(false);
            MapCam(true, player._player_shot.playerCam.cam_Obj);
        } //맵캠으로 변경

        yield return new WaitForSeconds(3f); //맵 확인하는 시간

        ui_Anim.SetBool("Score", true);
        manager_Anim.SetBool("GameEnd", true);

        yield return new WaitForSeconds(3f); //스코어 게이지 충전...
        //점수 확인
        if ( Check_Color(ETeam.Yellow) > Check_Color(ETeam.Blue))
        {
            teamYellow_Anim.Win();
            teamBlue_Anim.Lose();
            StartCoroutine(Winner_Team(ETeam.Yellow));
        }
        else //블루 팀 승리
        {
            teamBlue_Anim.Win();
            teamYellow_Anim.Lose();
            StartCoroutine(Winner_Team(ETeam.Blue));
        }
    }
    public int Check_Color(ETeam team)
    {
        int teamScore = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].team == team)
                teamScore++;
        }
        return teamScore;
    }

    public IEnumerator Winner_Team(ETeam team)
    {
        Debug.Log($"{team}팀 승리!");
        yield return null;

    }
    public void SetCursorState(bool newState)
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
    }
}
