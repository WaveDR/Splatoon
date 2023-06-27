using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager _instance = null;
    public static ScoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ScoreManager>();
            }
            return _instance;
        }
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
    [SerializeField] private Animator ui_Anim;

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
            _Time = Mathf.Clamp(_Time, 0, 180);
        }
    }
    
    [Header("SpawnPos")]
    public Vector3 yellowSpawn = new Vector3(0,3.6f,-60f);
    public Vector3 blueSpawn = new Vector3(0, 3.6f, 60f);

    public bool gameStart;

    private void Awake()
    {
        map_Camera = GameObject.FindGameObjectWithTag("MapCamera");
        ui_Anim = GameObject.FindGameObjectWithTag("TimeUI").GetComponent<Animator>();
        players = FindObjectsOfType<PlayerController>();
    }
    private void OnEnable()
    {
        deltaTime = startTimer;
        count_Image.gameObject.SetActive(true);
    }
    private void Start()
    {
        foreach(PlayerController player in players)
        {
            MapCam(false, player._player_shot.playerCam.cam_Obj);
        }
    }
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
        else
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

        if(deltaTime <= 61)
        {
            ui_Anim.SetBool("One_Min",true);
            timeText_Min.color = players[0].player_Team.team_Yellow;
            timeText_Mid.color = players[0].player_Team.team_Yellow;
            timeText_Sec.color = players[0].player_Team.team_Yellow;
        }
        if (deltaTime <= 10 && deltaTime > 0)
        {
            ui_Anim.SetBool("One_Min", false);
            CountDown((int)deltaTime);
        }
        if(deltaTime <= 0)
        {
            ui_Anim.SetBool("Count", false);
            ui_Anim.SetBool("TimeOut", true);
        }
    }

    private void CountDown(int count)
    {
        ui_Anim.SetBool("Count",true);
        count_Image.sprite = count_Sprite[count];
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
            StartCoroutine(GameStop());
        }
    }
    public IEnumerator GameStop()
    {
        foreach (PlayerController player in players)
        {
            player.isStop = true;
        }
        yield return new WaitForSeconds(5f);

        foreach (PlayerController player in players)
        {
            MapCam(true, player._player_shot.playerCam.cam_Obj);
        }

        if( Check_Color(ETeam.Yellow) > Check_Color(ETeam.Blue))
        {
            StartCoroutine(Winner_Team(ETeam.Yellow));
        }
        else //ºí·ç ÆÀ ½Â¸®
        {
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
        Debug.Log($"{team}ÆÀ ½Â¸®!");
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
