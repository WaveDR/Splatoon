using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class Photon_Manager : MonoBehaviourPunCallbacks
{
    public static Photon_Manager Instance = null;
    public Setting_Manager set_Manager;
    private readonly string game_Version = "1";

    public int max_Player;

    public bool isCreateRoom;
    public ServerSettings setting = null;
    public GameObject matching_UI;

    //Player Prefabs

    public GameObject playerPrefabs;

    public Text stateUI;
    public Text cur_Player;

    public Text server_Ip;
    public Text server_Port;

    private bool isReady;
    public GameObject start_Btn;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        set_Manager.GetComponentInChildren<Setting_Manager>();
    }
    private void Start()
    {
        //Connect();
        PhotonNetwork.LogLevel = PunLogLevel.Informational;
    }
    private void OnApplicationQuit()
    {
        DisConnect();
    }
    #region 서버 관련 CallBack Method
    //Connect Tto Master -

    public void SetRoom_MaxPlayer(int i)
    {
        max_Player = i;
        isCreateRoom = true;
        matching_UI.SetActive(false);
        stateUI.text = "방 생성";
        Debug.Log("Created Room");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = max_Player });
    }

    [PunRPC]
    public void MaxPlayer(int i)
    {
        max_Player = i;
    
    }
    public IEnumerator GameStart_Skip()
    {
        matching_UI.transform.parent.gameObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        set_Manager.LoadingOff();
        photonView.RPC("GameStart_Network", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void GameStart_Network()
    {
        GameManager.Instance.skip_Start = true;
    }
    public void Matching_Room()
    {
        Connect();
    }
    public void Connect()
    {
        PhotonNetwork.GameVersion = game_Version;

        //Master 서버 연결

        PhotonNetwork.ConnectToMaster(setting.AppSettings.Server, setting.AppSettings.Port,"");

        Debug.Log("Connect to Master Server...");
        stateUI.text = "서버에 연결합니다...";
    }

    public void DisConnect()
    {
        PhotonNetwork.Disconnect(); //Photon Server DisConnect
    }

    //CallBack Methord

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connect To Master Server Join.");
        stateUI.text = "서버에 연결됐습니다.";
        PhotonNetwork.JoinLobby();
        Debug.Log("Connected to Master Server");
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Join Lobby Zone");
        stateUI.text = "로비에 연결됐습니다.";

        base.OnJoinedLobby();
        PhotonNetwork.JoinRandomRoom();

    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("Not Empty Room...");
        stateUI.text = "1 vs 1 매치!";
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        PhotonNetwork.LoadLevel("InGame");
        Debug.Log("Room Join Success");
        stateUI.text = "방에 입장합니다.";

        if(PhotonNetwork.IsMasterClient)
        start_Btn.SetActive(true);
        //나중에 입장한 플레이어 모으기 && 다 모이면 게임 시작 누를 수 있도록 수정예정
        StartCoroutine(Player_Spawn());
    }
    
    IEnumerator Player_Spawn()
    {
        yield return new WaitForSeconds(1f);
        GameObject player = PhotonNetwork.Instantiate(playerPrefabs.name, Vector3.zero, Quaternion.identity);
        PlayerController player_Con = player.GetComponent<PlayerController>();
       
        player_Con.photonView.RPC("Player_Set", RpcTarget.AllBuffered, 
            player_Con.player_Team.team,player_Con._player_shot.WeaponType,player_Con.player_Input.player_Name);
  
        GameManager.Instance.SetPlayerPos();
    }
    #endregion

    private void Update()
    {
        if(PhotonNetwork.InRoom)
        cur_Player.text = $"{PhotonNetwork.CurrentRoom.PlayerCount}/{max_Player}";
    }
}
