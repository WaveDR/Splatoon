using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public enum EWeapon { Brush, Gun, Bow }
public class PlayerShooter : MonoBehaviourPun
{
    [Header("Weapon")]
    [Header("============================================")]
    public GameObject skill_UI_Obj;
    public GameObject[] weapon_Obj;
    public EWeapon WeaponType;

    public int weaponNum;            // 총 번호
    public GameObject ammo_Back;     // 총알 통 애니메이션용

    //Limited UI 
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
    [Header("============================================")]
    public Player_Camera playerCam;
    [SerializeField] private GameObject[] weapon_Aim;

    [Header("Bow Effect")]
    [Header("============================================")]
    [SerializeField] private ParticleSystem _Charge_Effect;

    //근접 공격 콤보
    private bool _combo_Attack;
    private bool _combo_Start;
    private int _combo_Num;

    [Header("Player_Score")]
    [Header("============================================")]
    public int player_Score = 0;
    private int _player_ScoreSet;
    public int player_ScoreSet => _player_ScoreSet;

    [Header("Shot_UI")]
    [Header("============================================")]
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
    [Header("============================================")]
    [SerializeField] private float fireMaxTime;
    [SerializeField] private float fireRateTime;
    [SerializeField] private float combo_ResetTime;
    // [SerializeField] private bool fireReady;

    [Header("IK Transform")]
    [Header("============================================")]
    public Transform weapon_Pivot;
    public Transform[] left_HandMount;
    public Transform[] right_HandMount;


    [Header("Player Component")]
    [Header("============================================")]
    private PlayerController _Player_Con;
    private PlayerInput _player_Input;
    private Animator _player_Anim;
    public Shot_System weapon;
    public PlayerController Player_Con => _Player_Con;
    public PlayerInput player_Input => _player_Input;

    // Start is called before the first frame update

    #region Player Data
    public void UI_Set_Server()
    {
        //UI Component 가져오기
        shot_UI = FindObjectOfType<Shot_UI>();

        if (!photonView.IsMine) return;
        //에임 UI
        weapon_Aim[0] = shot_UI.weapon_Aim[0];
        weapon_Aim[1] = shot_UI.weapon_Aim[1];
        bowAim_UI = shot_UI.bowAim_UI;

        //킬로그 UI
        killLog_UI = shot_UI.killLog_UI;
        killLog_Obj = shot_UI.killLog_Obj;

        //사망 UI
        enemyData_Obj = shot_UI.enemyData_Obj;
        enemyName_UI = shot_UI.enemyName_UI;
        enemyScore_UI = shot_UI.enemyScore_UI;

        //Ammo / Nam / Score UI
        ammoBack_UI = shot_UI.ammoBack_UI;
        ammoNot_UI = shot_UI.ammoNot_UI;
        name_UI = shot_UI.name_UI;
        score_UI = shot_UI.score_UI;

        //UI 자체 오브젝트
        skill_UI_Obj = shot_UI.gameObject;

        //Hit UI
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
        TryGetComponent(out _player_Anim);

        //적용된 EWeapon에 따른 WeaponNum 동기화
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

        //WeaponNum에 따라 공격 애니메이션 및 오브젝트 차별화
        _player_Anim.SetInteger("WeaponNum", weaponNum);

        for (int i = 0; i < weapon_Obj.Length; i++)
        {
            //선택된 오브젝트 외 전부 비활성화
            weapon_Obj[i].SetActive(true);
            if (weaponNum != i)
                weapon_Obj[i].SetActive(false);
        }

        //유일하게 활성화된 오브젝트의 Shot_System Component 가져오기
        weapon = GetComponentInChildren<Shot_System>();

        //Weapon Stat 정보값 적용
        fireMaxTime = weapon.weapon_Stat.fire_Rate;
        weapon.Weapon_Color_Change(team);

        if(_Player_Con._enemy == null)
        {
            if (photonView.IsMine)
            {
                //카메라 에임에 따른 Y축 회전 값 추가
                playerCam.weapon_DirY = weapon;

                //UI 리셋
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
    #endregion

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
            // 총의 기준점을 3D 모델 오른쪽 팔꿈치로 이동
            weapon_Pivot.position = _player_Anim.GetIKHintPosition(AvatarIKHint.RightElbow) + new Vector3(-0.05f, -0.1f, 0.07f);

            //왼쪽팔
            _player_Anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
            _player_Anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

            _player_Anim.SetIKPosition(AvatarIKGoal.LeftHand, left_HandMount[weaponNum].position);
            _player_Anim.SetIKRotation(AvatarIKGoal.LeftHand, left_HandMount[weaponNum].rotation);

            //오른팔

            _player_Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
            _player_Anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

            _player_Anim.SetIKPosition(AvatarIKGoal.RightHand, right_HandMount[weaponNum].position);
            _player_Anim.SetIKRotation(AvatarIKGoal.RightHand, right_HandMount[weaponNum].rotation);
        }
    }

    //============================================        ↑ CallBack   |   Nomal ↓        ========================================================


    #region Kill & Score
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
            killLog_UI.text = $"{name}(을)를 쓰러뜨렸다!";
            Invoke("KillLog_Out", 1.5f);
        }
    }
    public void KillLog_Out()
    {
        killLog_Obj.SetActive(false);
    }
    public IEnumerator Enemy_Data_UI(string name, int score)
    {
        //플레이어 사망 시 적 정보 값을 가져와 출력
        if (_Player_Con._enemy == null)
        {
            enemyData_Obj.SetActive(true);
            enemyName_UI.text = name;
            enemyScore_UI.text = score.ToString("D4");
            yield return new WaitForSeconds(5f);
            enemyData_Obj.SetActive(false);
        }
    }
    #endregion

    #region Attack Method
    private void Fire_Paint()
    {
        switch (WeaponType)
        {
            case EWeapon.Gun:

                if (_player_Input.fire && !_player_Input.squid_Form)
                {
                    //사람 형태로 공격 시
                    fireRateTime += Time.deltaTime;
                    _isFire = true;
                   
                    if (fireRateTime >= fireMaxTime && weapon.weapon_CurAmmo > 0) //총알 보유시 공격 가능
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

                if (Player_Con._isJump && weapon_JumpRot < 0) // 점프 O
                {
                    weapon.transform.localEulerAngles = new Vector3(0, 0, weapon_JumpRot += 540 * Time.deltaTime);
                }
                else if (!Player_Con._isJump && weapon_JumpRot > -90) //점프 X
                {
                    weapon.transform.localEulerAngles = new Vector3(0, 0, weapon_JumpRot -= 540 * Time.deltaTime);
                }

                if (_player_Input.fire) //공격 차지
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
                if (fireRateTime >= fireMaxTime && weapon.weapon_CurAmmo > 0) //공격 조건 충족 시 fUp으로 공격
                {
                    if (!_isCharge)
                    {
                        _Charge_Effect.Play();
                        _isCharge = true;
                    }

                    //차지 중
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
                else if ((fireRateTime < fireMaxTime || weapon.weapon_CurAmmo <= 0) && _player_Input.fUp) //완충 전에 공격 시 실패
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
                    //Combo Start가 시작되면 근접공격 콤보 실행
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

                    //공격 중 2초 이상 공격을 멈출 경우 초기화
                    if (combo_ResetTime >= 2f && _combo_Attack)
                    {
                        _combo_Start = false;
                    }

                    if (fireRateTime >= 0.3f) _combo_Attack = true; //콤보 가능 시간
                    if (_combo_Num >= 4) _combo_Num = -1;           //콤보 초기화
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

        //남은 총알 UI & Obj
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
#endregion

    #region Ammo Method
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

    private void WarningAmmo()
    {
        //총알 부족 시 호출되는 경고 UI
        if (_Player_Con._enemy == null)
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
    #endregion
}