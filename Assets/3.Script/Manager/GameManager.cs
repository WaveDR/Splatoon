using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;
    public List<TeamZone> nodes = new List<TeamZone>();
    public static GameManager Instance
    {
        get
        {
            if(_instance == null)
            {
                GameObject obj = new GameObject("GameManager");
                obj.AddComponent<GameManager>();
                _instance = obj.GetComponent<GameManager>();
            }
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
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
            GameManager.Instance.SetCursorState(true);
        }
    }
    public void SetCursorState(bool newState)
    {
       //Cursor.lockState = newState ? CursorLockMode.Confined : CursorLockMode.None;
    }
}
