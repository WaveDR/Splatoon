using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EWeapon { Brush, Gun, Bow }
public class PlayerShooter : MonoBehaviour
{
    [Header("Weapon")]

    public GameObject skill_UI_Obj;
    
    public EWeapon WeaponType;
    public GameObject[] weapon_Obj;
    public Shot_System weapon;
    
    public int weaponNum;            // 총 번호
    public GameObject ammo_Back;     // 총알 통 애니메이션용

    private bool _isFire;
    private bool _isCharge;
    private float JumpRot;
    private float weapon_JumpRot
    {
        get { return JumpRot; }

        set
        {

            JumpRot = value;
            JumpRot = Mathf.Clamp(JumpRot, -90, 0);
        }
    }


    [SerializeField] private Player_Camera _playerCam;
    [SerializeField] private ParticleSystem _Charge_Effect;
    [SerializeField] private bool _combo_Attack;
    [SerializeField] private bool _combo_Start;
    [SerializeField] private int _combo_Num;
    [SerializeField] private Image bowAim;
    [SerializeField] private GameObject[] weapon_Aim;

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
    private PlayerInput _player_Input;
    private Animator _player_Anim;

    
    // Start is called before the first frame update
    void Awake()
    {
        TryGetComponent(out _player_Input);
        TryGetComponent(out _Player_Con);
        TryGetComponent(out _player_Anim);
        TryGetComponent(out _playerCam);
    }

    private void OnEnable()
    {
        //선택에 따라 활성화 disable도 동일한 방식

        switch (WeaponType)
        {
            case EWeapon.Brush:
                weaponNum = 0;
                for (int i = 0; i < weapon_Aim.Length; i++)
                {
                    weapon_Aim[i].SetActive(false);
                }
                break;

            case EWeapon.Gun:
                weaponNum = 1;
                weapon_Aim[0].SetActive(true);
                weapon_Aim[1].SetActive(false);
                break;

            case EWeapon.Bow:
                weaponNum = 2;
                weapon_Aim[1].SetActive(true);
                weapon_Aim[0].SetActive(false);
                break;
        }

        for (int i = 0; i < weapon_Obj.Length; i++)
        {
            weapon_Obj[i].SetActive(true);
            if (weaponNum != i)
                weapon_Obj[i].SetActive(false);
        }
        _player_Anim.SetInteger("WeaponNum", weaponNum);

        weapon = GetComponentInChildren<Shot_System>();
        fireMaxTime = weapon.weapon_Stat.fire_Rate;
        _playerCam.weapon_DirY = GetComponentInChildren<Shot_System>();
    }

    void Update()
    {
        //공격로직
        Fire_Paint();
    }

    private void OnDisable()
    {
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

    private void Fire_Paint()
    {
        switch (WeaponType)
        {
            case EWeapon.Gun:

                if (_player_Input.fire && !_player_Input.squid_Form)
                {
            
                    fireRateTime += Time.deltaTime;
                    ammo_Back.transform.localScale =
                    new Vector3(ammo_Back.transform.localScale.x, weapon.weapon_CurAmmo * 0.0018f, ammo_Back.transform.localScale.z);
                    _isFire = true;

                    _player_Anim.SetTrigger("Reload_Bow");

                    if (fireRateTime >= fireMaxTime && weapon.weapon_CurAmmo > 0)
                    {
                        _player_Anim.SetBool("isFire", true);
                        weapon.Shot();
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

                if (_Player_Con._isJump && weapon_JumpRot < 0) // 점프 O
                {
                    weapon.transform.localEulerAngles = new Vector3(0, 0, weapon_JumpRot += 540 * Time.deltaTime);
                }
                else if (!_Player_Con._isJump && weapon_JumpRot > -90) //점프 X
                {
                    weapon.transform.localEulerAngles = new Vector3(0, 0, weapon_JumpRot -= 540 * Time.deltaTime);
                }

                if (_player_Input.fire)
                {
                    fireRateTime += Time.deltaTime;
                    bowAim.fillAmount = fireRateTime;
                    ammo_Back.transform.localScale =
                    new Vector3(ammo_Back.transform.localScale.x, weapon.weapon_CurAmmo * 0.0018f, ammo_Back.transform.localScale.z);
                    _isFire = true;
                    _player_Anim.SetTrigger("Reload_Bow");
                }
                if (fireRateTime >= fireMaxTime && weapon.weapon_CurAmmo > 0)
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
                        weapon.Shot();
                        _isCharge = false;
                        fireRateTime = 0;
                    }
                }
                else if ((fireRateTime < fireMaxTime || weapon.weapon_CurAmmo <= 0) && _player_Input.fUp)
                {
                    fireRateTime = 0;
                    bowAim.fillAmount = fireRateTime;
                    _player_Anim.SetBool("isFire", false);
                    _isFire = false;
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
                        ammo_Back.transform.localScale =
                            new Vector3(ammo_Back.transform.localScale.x, weapon.weapon_CurAmmo * 0.0018f, ammo_Back.transform.localScale.z);
                        _combo_Num++;
                        combo_ResetTime = 0;
                        _player_Anim.SetInteger("Brush_Combo", _combo_Num);
                        _combo_Attack = false;
                    }


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
    }
    public void shot()
    {
        weapon.Shot();
    }
    public void Reload_Ammo(int speed)
    {
        if (weapon.weapon_CurAmmo <= weapon.weapon_MaxAmmo && !_isFire)
        {
            weapon.weapon_CurAmmo += Time.deltaTime * speed;

            ammo_Back.transform.localScale =
           new Vector3(ammo_Back.transform.localScale.x, weapon.weapon_CurAmmo * 0.0018f, ammo_Back.transform.localScale.z);
        }

    }
}