using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public enum EWeapon { Brush, Gun, Bow }
public class PlayerShooter : MonoBehaviourPun
{
    [Header("Weapon")]

    public GameObject skill_UI_Obj;
    public GameObject[] weapon_Obj;

    public EWeapon WeaponType;
    public Shot_System weapon;

    public int weaponNum;            // ÃÑ ¹øÈ£
    public GameObject ammo_Back;     // ÃÑ¾Ë Åë ¾Ö´Ï¸ÞÀÌ¼Ç¿ë

    private bool _isFire;
    private bool _isCharge;
    private bool _isCharge_Sfx;
    private float _jumpRot;
    private float weapon_JumpRot
    {
        get { return _jumpRot; }

        set
        {

            _jumpRot = value;
            _jumpRot = Mathf.Clamp(_jumpRot, -90, 0);
        }
    }

    [Header("Player Aim")]
    public Player_Camera playerCam;
    [SerializeField] private GameObject[] weapon_Aim;

    [Header("Bow Effect")]
    [SerializeField] private ParticleSystem _Charge_Effect;

    //±ÙÁ¢ °ø°Ý ÄÞº¸
    private bool _combo_Attack;
    private bool _combo_Start;
    private int _combo_Num;

    [Header("Player_Score")]
    public int player_Score = 0;
    private int _player_ScoreSet;
    public int player_ScoreSet => _player_ScoreSet;

    [Header("Shot_UI")]
    public Shot_UI shot_UI;
    [SerializeField] private Image bowAim_UI;
    [SerializeField] private Text killLog_UI;
    [SerializeField] private GameObject killLog_Obj;
    [SerializeField] private GameObject enemyData_Obj;
    [SerializeField] private Text enemyName_UI;
    [SerializeField] private Text enemyScore_UI;

    public Image ammoBack_UI;
    public Image ammoNot_UI;
    public Text name_UI;
    public Text score_UI;

    public GameObject[] hit_UI;

    [Header("Attack Rate")]

    [SerializeField] private float fireMaxTime;
    [SerializeField] private float fireRateTime;
    [SerializeField] private float combo_ResetTime;
    // [SerializeField] private bool fireReady;

    [Header("IK Transform")]
    public Transform weapon_Pivot;
    public Transform[] left_HandMount;
    public Transform[] right_HandMount;


    [Header("Player Component")]
    private PlayerController _Player_Con;
    public PlayerController Player_Con => _Player_Con;
    private PlayerInput _player_Input;
    private Animator _player_Anim;

    public PlayerInput player_Input => _player_Input;

    // Start is called before the first frame update
    public void UI_Set_Server()
    {
        shot_UI = FindObjectOfType<Shot_UI>();

        if (!photonView.IsMine) return;
        weapon_Aim[0] = shot_UI.weapon_Aim[0];
        weapon_Aim[1] = shot_UI.weapon_Aim[1];

        bowAim_UI = shot_UI.bowAim_UI;
        killLog_UI = shot_UI.killLog_UI;
        killLog_Obj = shot_UI.killLog_Obj;
        enemyData_Obj = shot_UI.enemyData_Obj;
        enemyName_UI = shot_UI.enemyName_UI;
        enemyScore_UI = shot_UI.enemyScore_UI;

        ammoBack_UI = shot_UI.ammoBack_UI;
        ammoNot_UI = shot_UI.ammoNot_UI;
        name_UI = shot_UI.name_UI;
        score_UI = shot_UI.score_UI;

        skill_UI_Obj = shot_UI.gameObject;

        hit_UI[0] = shot_UI.hit_UI[0];
        hit_UI[1] = shot_UI.hit_UI[1];
        hit_UI[2] = shot_UI.hit_UI[2];

        for (int i = 0; i < hit_UI.Length; i++)
        {
            hit_UI[i].SetActive(false);
        }
    }

    public void WeaponSet(ETeam team)
    {
        TryGetComponent(out _player_Input);
        TryGetComponent(out _Player_Con);
        TryGetComponent(out playerCam);

        switch (WeaponType)
        {
            case EWeapon.Brush:
                weaponNum = 0;

                break;

            case EWeapon.Gun:
                weaponNum = 1;

                break;

            case EWeapon.Bow:
                weaponNum = 2;
                break;
        }
        TryGetComponent(out _player_Anim);

        _player_Anim.SetInteger("WeaponNum", weaponNum);
        for (int i = 0; i < weapon_Obj.Length; i++)
        {
            weapon_Obj[i].SetActive(true);
            if (weaponNum != i)
                weapon_Obj[i].SetActive(false);
        }

        weapon = GetComponentInChildren<Shot_System>();

        fireMaxTime = weapon.weapon_Stat.fire_Rate;

        weapon.Weapon_Color_Change(team);
        if(_Player_Con._enemy == null)
        {
            if (photonView.IsMine)
            {
                playerCam.weapon_DirY = weapon;

                killLog_Obj.SetActive(false);
                enemyData_Obj.SetActive(false);
                ammoNot_UI.gameObject.SetActive(false);
                ammoBack_UI.transform.parent.gameObject.SetActive(false);
                name_UI.text = _player_Input.player_Name;

                switch (WeaponType)
                {
                    case EWeapon.Brush:
                        for (int i = 0; i < weapon_Aim.Length; i++)
                        {
                            weapon_Aim[i].SetActive(false);
                        }
                        break;

                    case EWeapon.Gun:
                        weapon_Aim[0].SetActive(true);
                        weapon_Aim[1].SetActive(false);
                        break;

                    case EWeapon.Bow:
                        weapon_Aim[1].SetActive(true);
                        weapon_Aim[0].SetActive(false);
                        break;
                }
            }
        }
    }

    void Update()
    {

        if (!Player_Con.isStop || !_Player_Con.isDead)
        {
            Fire_Paint();

            if (player_Score >= 2)
            {
                Get_Score_Server();
            }
            if (photonView.IsMine && _Player_Con._enemy == null)
            {

                WarningAmmo();
                score_UI.text = player_ScoreSet.ToString("D4");
            }
            //°ø°Ý·ÎÁ÷
        }
    }
    public void Get_Score_Server()
    {
        _player_ScoreSet++;
        player_Score = 0;
    }
    public void KillLog(string name)
    {
        if (_Player_Con._enemy == null)
        {
            if (killLog_Obj.activeSelf) return;

            killLog_Obj.SetActive(true);
            killLog_UI.text = $"{name}(À»)¸¦ ¾²·¯¶ß·È´Ù!";
            Invoke("KillLog_Out",1.5f);
        }
    }

    public void KillLog_Out()
    {
        killLog_Obj.SetActive(false);
    }

    public IEnumerator Enemy_Score(string name, int score)
    {
        if (_Player_Con._enemy == null)
        {
            enemyData_Obj.SetActive(true);
            enemyName_UI.text = name;
            enemyScore_UI.text = score.ToString("D4");
            Debug.Log("»ç¸Á!");
            yield return new WaitForSeconds(5f);

            enemyData_Obj.SetActive(false);
        }
    }

    private void WarningAmmo()
    {
        if(_Player_Con._enemy == null)
        {

             if (weapon.weapon_CurAmmo <= 50)
             {
                 ammoBack_UI.transform.parent.gameObject.SetActive(true);
             }
             if (weapon.weapon_CurAmmo <= 10)
             {
                 ammoNot_UI.gameObject.SetActive(true);
             }
             else
             {
                 ammoNot_UI.gameObject.SetActive(false);
             }
        }
    }
    private void OnDisable()
    {
        if (!photonView.IsMine) return;
        weapon_Obj[weaponNum].SetActive(false);
    }
    private void OnAnimatorIK(int layerIndex)
    {

        if (WeaponType != EWeapon.Brush)
        {
            // ÃÑÀÇ ±âÁØÁ¡À» 3D ¸ðµ¨ ¿À¸¥ÂÊ ÆÈ²ÞÄ¡·Î ÀÌµ¿
            weapon_Pivot.position = _player_Anim.GetIKHintPosition(AvatarIKHint.RightElbow) + new Vector3(-0.05f, -0.1f, 0.07f);

            //¿ÞÂÊÆÈ
            _player_Anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
            _player_Anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

            _player_Anim.SetIKPosition(AvatarIKGoal.LeftHand, left_HandMount[weaponNum].position);
            _player_Anim.SetIKRotation(AvatarIKGoal.LeftHand, left_HandMount[weaponNum].rotation);

            //¿À¸¥ÆÈ

            _player_Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
            _player_Anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

            _player_Anim.SetIKPosition(AvatarIKGoal.RightHand, right_HandMount[weaponNum].position);
            _player_Anim.SetIKRotation(AvatarIKGoal.RightHand, right_HandMount[weaponNum].rotation);
        }
    }
    private void Fire_Paint()
    {
        switch (WeaponType)
        {
            case EWeapon.Gun:

                if (_player_Input.fire && !_player_Input.squid_Form)
                {

                    fireRateTime += Time.deltaTime;

                    _isFire = true;

                    _player_Anim.SetTrigger("Reload_Bow");

                    if (fireRateTime >= fireMaxTime && weapon.weapon_CurAmmo > 0)
                    {

                        _Player_Con.ES_Manager.Play_SoundEffect("Weapon_Gun");
                        _Player_Con.ES_Manager.Stop_Sound_Effect("Floor_Hit");
                        _Player_Con.ES_Manager.Play_SoundEffect("Floor_Hit");
                        _player_Anim.SetBool("isFire", true);
                        weapon.Shot();

                        //weapon.photonView.RPC("Shot", RpcTarget.AllBuffered);
                        fireRateTime = 0;
                    }

                    else
                    {
                        _player_Anim.SetBool("isFire", false);
                    }
                }
                else
                {
                    fireRateTime = 0;
                    _player_Anim.SetBool("isFire", false);

                    _isFire = false;
                }
                break;

            case EWeapon.Bow:

                if (Player_Con._isJump && weapon_JumpRot < 0) // Á¡ÇÁ O
                {
                    weapon.transform.localEulerAngles = new Vector3(0, 0, weapon_JumpRot += 540 * Time.deltaTime);
                }
                else if (!Player_Con._isJump && weapon_JumpRot > -90) //Á¡ÇÁ X
                {
                    weapon.transform.localEulerAngles = new Vector3(0, 0, weapon_JumpRot -= 540 * Time.deltaTime);
                }

                if (_player_Input.fire)
                {
                    fireRateTime += Time.deltaTime;

                    if(photonView.IsMine)
                    bowAim_UI.fillAmount = fireRateTime;

                    _isFire = true;
                    _player_Anim.SetTrigger("Reload_Bow");

                    if (!_isCharge_Sfx)
                    {
                        _Player_Con.ES_Manager.Stop_Sound_Effect("Weapon_Charge");
                        _Player_Con.ES_Manager.Play_SoundEffect("Weapon_Charge");
                        _isCharge_Sfx = true;
                    }

                }
                if (fireRateTime >= fireMaxTime && weapon.weapon_CurAmmo > 0)
                {
                    if (!_isCharge)
                    {
                        _Charge_Effect.Play();
                        _isCharge = true;
                    }

                    //Â÷Áö Áß
                    if (_player_Input.fUp)
                    {
                        _player_Anim.SetBool("isFire", true);
                        _Player_Con.ES_Manager.Stop_Sound_Effect("Weapon_Bow");
                        _Player_Con.ES_Manager.Play_SoundEffect("Weapon_Bow");
                        _Player_Con.ES_Manager.Stop_Sound_Effect("Floor_Hit");
                        _Player_Con.ES_Manager.Play_SoundEffect("Floor_Hit");
                        fireRateTime = 0;

                        if(photonView.IsMine)
                        bowAim_UI.fillAmount = fireRateTime;

                        weapon.Shot();
                        //weapon.photonView.RPC("Shot", RpcTarget.AllBuffered);

                        _isCharge = false;
                        _isCharge_Sfx = false;
                    }
                }
                else if ((fireRateTime < fireMaxTime || weapon.weapon_CurAmmo <= 0) && _player_Input.fUp)
                {
                    fireRateTime = 0;

                    if (photonView.IsMine)
                        bowAim_UI.fillAmount = fireRateTime;

                    _player_Anim.SetBool("isFire", false);

                    _Player_Con.ES_Manager.Stop_Sound_Effect("Weapon_Charge");
                    _Player_Con.ES_Manager.Stop_Sound_Effect("Weapon_Not_Ammo");
                    _Player_Con.ES_Manager.Play_SoundEffect("Weapon_Not_Ammo");

                    _isFire = false;
                    _isCharge_Sfx = false;
                }
                else
                {
                    _player_Anim.SetBool("isFire", false);

                }

                if (_player_Input.squid_Form)
                {
                    fireRateTime = 0;
                    _player_Anim.SetBool("isFire", false);
                    _isFire = false;
                    _isCharge_Sfx = false;
                }

                break;
            case EWeapon.Brush:

                if (_player_Input.fDown) _combo_Start = true;

                if (_combo_Start)
                {
                    fireRateTime += Time.deltaTime;
                    combo_ResetTime += Time.deltaTime;

                    if (_player_Input.fDown && _combo_Attack && weapon.weapon_CurAmmo > 0)
                    {
                        _isFire = true;
                        _combo_Num++;
                        combo_ResetTime = 0;
                        _player_Anim.SetInteger("Brush_Combo", _combo_Num);
                        _Player_Con.ES_Manager.Stop_Sound_Effect("Weapon_Brush");
                        _Player_Con.ES_Manager.Play_SoundEffect("Weapon_Brush");
                        _combo_Attack = false;
                    }


                    if (combo_ResetTime >= 2f && _combo_Attack)
                    {
                        _combo_Start = false;
                    }

                    if (fireRateTime >= 0.3f) _combo_Attack = true; //ÄÞº¸ °¡´É ½Ã°£
                    if (_combo_Num >= 4) _combo_Num = -1;           //ÄÞº¸ ÃÊ±âÈ­
                }
                else
                {
                    _combo_Num = 0;
                    fireRateTime = 0;
                    combo_ResetTime = 0;
                    _isFire = false;
                    _combo_Attack = false;
                    _player_Anim.SetInteger("Brush_Combo", 0);

                }

                break;
        }

        if (photonView.IsMine && _Player_Con._enemy == null)
        {
            if(ammoBack_UI.gameObject.activeSelf)
            ammoBack_UI.fillAmount = weapon.weapon_CurAmmo * 0.01f;

            ammo_Back.transform.localScale = new Vector3(ammo_Back.transform.localScale.x, weapon.weapon_CurAmmo * 0.0018f, ammo_Back.transform.localScale.z);
        }
    }
    public void shot()
    {
         weapon.Shot();
        //weapon.photonView.RPC("Shot", RpcTarget.AllBuffered);
    }

    public void Reload_Ammo(int speed)
    {
        if (weapon.weapon_CurAmmo <= weapon.weapon_MaxAmmo && !_isFire)
        {
            Reload_Server(speed);

            ammo_Back.transform.localScale =
           new Vector3(ammo_Back.transform.localScale.x, weapon.weapon_CurAmmo * 0.0018f, ammo_Back.transform.localScale.z);
        }
    }

    [PunRPC]
    public void Reload_Server(int speed)
    {
        weapon.weapon_CurAmmo += Time.deltaTime * speed;
    }
}