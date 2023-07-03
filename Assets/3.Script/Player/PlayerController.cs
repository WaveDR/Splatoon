using UnityEngine;
using Photon.Pun;
public class PlayerController : Living_Entity, IPlayer, IPunObservable
{
    [Header("Player Stat")]
    public PlayerTeams player_Team;
    public bool _isJump;
    public bool isWall;

    [Header("Player Move")]
    [SerializeField] private GameObject raycast_Object;
    [SerializeField] private GameObject raycast_Wall_Object;
    [SerializeField] private ParticleSystem[] player_Wave;

    [Header("Squidform")]
    [SerializeField] private GameObject[] human_Object;
    [SerializeField] private GameObject squid_Object;

    public PlayerInput player_Input;
    private Rigidbody _player_rigid;
    public PlayerShooter _player_shot;
    private Animator _player_Anim;

    private float _player_Speed;
    private bool _Wall_RacastOn;
    private Bullet dmgBullet; //맞을 때 받을 데미지값 가져오기

    [SerializeField] private float hp;

    private float spawnTime = 5;
    private float plusTime;

    public Bullet deathEffect;

    public Sound_Manager ES_Manager;
    // Start is called before the first frame update
    void Awake()
    {
        TryGetComponent(out player_Input);
        TryGetComponent(out _player_rigid);
        TryGetComponent(out _player_Anim);
        TryGetComponent(out _player_shot);
        TryGetComponent(out player_Team);

        Player_StatReset();

        foreach (ParticleSystem start in player_Wave) start.Stop();

        for (int i = 1; i < hitEffect.Length; i++)
        {
            hitEffect[i] = hitEffect[0].transform.GetChild(i).GetComponent<ParticleSystem>();
        }
        deathEffect = transform.GetChild(transform.childCount - 1).GetComponent<Bullet>();

        ES_Manager = GetComponentInChildren<Sound_Manager>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        player_CurHealth = player_Stat.max_Heath;
    }

    private void FixedUpdate()
    {
        _player_rigid.MovePosition(_player_rigid.position + player_Input.move_Vec * _player_Speed * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        if (!isStop)
        {
            if (isFalling) Respawn(player_Team.team, true); //맵 밖으로 떨어질 때
            if (isDead) Respawn(player_Team.team, false);   //죽을 때 리스폰

            else //죽지 않았을 때
            {
                Player_Movement();
                Player_Jump();
                RaycastFloor(player_Input.squid_Form);
                Player_Animation();

                if (!player_Input.squid_Form)
                {
                    _Wall_RacastOn = false;
                    MoveWall(false, null);
                }
                else
                {
                    RaycastWall(player_Input.squid_Form);
                }
            }
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            dmgBullet = other.GetComponent<Bullet>();

            if (dmgBullet.team.team != player_Team.team && !isDead)
            {
                OnDamage(dmgBullet.dmg);
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;
        if (collision.gameObject.CompareTag("DeadLine"))
        {
            spawnTime = 3;
            isFalling = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!photonView.IsMine) return;
        if (collision.gameObject.CompareTag("Wall"))
        {
            //_isJump = false;
            //raycast_Wall_Object = collision.gameObject;

            if (player_Input.squid_Form)
            {
                _Wall_RacastOn = true;
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (!photonView.IsMine) return;
        if (collision.gameObject.CompareTag("Wall")) // 건물 끄트머리에 도달할 시
        {
            MoveWall(false, null);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) 
    {
        if (stream.IsWriting)
        {
             stream.SendNext(player_Team.team);
             stream.SendNext(_player_shot.WeaponType);
             stream.SendNext(player_Input.player_Name);
        }
        else
        {
            player_Team.team = (ETeam)stream.ReceiveNext();
            _player_shot.WeaponType = (EWeapon)stream.ReceiveNext();
            player_Input.player_Name = (string)stream.ReceiveNext();
        }
    }


    //============================================        ↑ 콜백 메서드   |  일반 메서드 ↓        ========================================================

    [PunRPC]
    public void Player_Set(ETeam team, EWeapon weapon, string name)
    {
        player_Team.team = team;
        _player_shot.WeaponType = weapon;
        player_Input.player_Name = name;

        player_Team.Player_ColorSet();
        _player_shot.WeaponSet();

        player_Team.photonView.RPC("Player_ColorSet", RpcTarget.AllBuffered);
        _player_shot.photonView.RPC("WeaponSet", RpcTarget.AllBuffered);
    }


    private void Player_Jump()
    {
        if (player_Input.jDown && !_isJump)
        {
            _player_Anim.SetTrigger("isJump");
            //_isJump = true;
            _player_rigid.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }
    }

    private void Squid_Eular()
    {
        if (squid_Object.activeSelf)
        {
            if (player_Input.squid_FinalRot > 0)
                squid_Object.transform.localEulerAngles = new Vector3(0, player_Input.player_SquidRot += 900 * Time.deltaTime, 0);
            if (player_Input.squid_FinalRot < 0)
                squid_Object.transform.localEulerAngles = new Vector3(0, player_Input.player_SquidRot -= 900 * Time.deltaTime, 0);
        }
    }
    private void Player_Animation()
    {
        _player_Anim.SetFloat(player_Input.Move_Hor_S, player_Input.Move_Hor);
        _player_Anim.SetFloat(player_Input.Move_Ver_S, player_Input.Move_Ver);
        _player_Anim.SetBool("isDown", !_isJump);
    }
    private void Player_Movement()
    {
        if (player_Input.move_Vec != Vector3.zero)
        {
            transform.Translate(player_Input.move_Vec * _player_Speed * Time.deltaTime);
        }
        hp = player_CurHealth;
        Squid_Eular();
    }
    private void Player_StatReset()
    {
        _player_Speed = player_Stat.moveZone_Speed;
    }

    #region Override

    public override void OnDamage(float damage)
    {
        base.OnDamage(damage);

        if (player_Team.team == ETeam.Blue)
        {
            foreach (ParticleSystem par in hitEffect)
            {
                var hit = par.main;
                hit.startColor = (Color)player_Team.team_Yellow;
            }
        }
        else
        {
            foreach (ParticleSystem par in hitEffect)
            {
                var hit = par.main;
                hit.startColor = (Color)player_Team.team_Blue;
            }
        }

        if (!isDead)
        {
            ES_Manager.Play_SoundEffect("Player_Hit");
            _player_Anim.SetTrigger("Hit");
            hitEffect[0].Play();
        }

    }

    [PunRPC]
    public override void RestoreHp(float newHealth)
    {
        base.RestoreHp(newHealth);

    }
    public override void Player_Die()
    {
        base.Player_Die();
        Transform_Stat(0, 0, false, false);
        photonView.RPC("Transform_Stat", RpcTarget.All, 0, 0, false, false);
        deathEffect.particle.Play();
        dmgBullet.Player_Kill(player_Input.player_Name);
        GameManager.Instance.Player_Dead_Check(); //Player Dead UI;
        ES_Manager.Play_SoundEffect("Player_Death");

        StartCoroutine(_player_shot.Enemy_Score(dmgBullet.player_Shot.player_Input.player_Name, dmgBullet.player_Shot.player_ScoreSet));
    }
    public void Respawn(ETeam team, bool falling)
    {
        if (photonView.IsMine)
        {


            plusTime += Time.deltaTime;
            if (plusTime >= 1)
            {
                _player_shot.playerCam.cam_Obj.gameObject.SetActive(false);
            }
            GameManager.Instance.MapCam(true, _player_shot.playerCam.cam_Obj.gameObject);
            if (!falling)
            {
                if (plusTime >= spawnTime)
                {
                    //팀 구분
                    if (team == ETeam.Yellow)
                    {
                        transform.rotation = Quaternion.identity;
                        transform.position = GameManager.Instance.team_Yellow_Spawn[Random.Range(0, 3)];
                    }
                    else
                    {
                        transform.rotation = Quaternion.Euler(0, 180, 0);
                        transform.position = GameManager.Instance.team_Blue_Spawn[Random.Range(0, 3)];
                    }

                    //카메라 전환
                    GameManager.Instance.MapCam(false, _player_shot.playerCam.cam_Obj.gameObject);

                    //리스폰 시간 초기화
                    spawnTime = 5f;
                    plusTime = 0;

                    //플레이어 스탯 초기화
                    player_CurHealth = player_Stat.max_Heath;
                    _player_shot.weapon.weapon_CurAmmo = _player_shot.weapon.weapon_MaxAmmo;
                    Transform_Stat(0, player_Stat.moveZone_Speed, false, true);
                    //리턴
                    isDead = false;
                    GameManager.Instance.Player_Dead_Check(); //Player Dead UI;

                    return;
                }
            }
            else //맵 밖으로 떨어질 때
            {
                if (plusTime >= spawnTime)
                {
                    //팀 구분
                    if (player_Team.team == ETeam.Yellow)
                    {
                        transform.rotation = Quaternion.identity;
                        transform.position = GameManager.Instance.team_Yellow_Spawn[Random.Range(0, 3)];
                    }
                    else
                    {
                        transform.rotation = Quaternion.Euler(0, 180, 0);
                        transform.position = GameManager.Instance.team_Blue_Spawn[Random.Range(0, 3)];
                    }
                    //카메라 전환
                    GameManager.Instance.MapCam(false, _player_shot.playerCam.cam_Obj.gameObject);
                    //리스폰 초기화, 리턴
                    spawnTime = 5f;
                    plusTime = 0;
                    isFalling = false;
                    return;
                }
            }
        }
    } //리스폰
    #endregion
    public void Player_Data_SetUp(ETeam team, EWeapon weapon, string name)
    {
        player_Team.team = team;
        _player_shot.WeaponType = weapon;
        player_Input.player_Name = name;
    }

    public void UI_OnOFf(bool on)
    {
        _player_shot.skill_UI_Obj.SetActive(on);
    }

    #region Player Raycast
    private void RaycastFloor(bool SquidForm)
    {
        Debug.DrawRay(transform.position, Vector3.down * player_Stat.detect_Range, Color.green); ;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, player_Stat.detect_Range, player_Stat.floor_Layer))
        {
            raycast_Object = hit.collider.gameObject;

            TeamZone teamZone = raycast_Object.GetComponent<TeamZone>();

            if (teamZone != null)
                _isJump = false;

            if (teamZone.team == player_Team.team) // 내 진영과 현재 바닥 진영이 같을 때
            {
                if (SquidForm)// 오징어 형태
                {
                    Transform_Stat(30, player_Stat.dashSpeed, false, false);
                    photonView.RPC("Transform_Stat", RpcTarget.All, 30, player_Stat.dashSpeed, false, false);

                    RestoreHp(recovery_Speed * 3);
                    photonView.RPC("RestoreHp", RpcTarget.All, recovery_Speed * 3);
                }

                else //사람 형태
                {
                    Transform_Stat(20, player_Stat.moveZone_Speed, false, true);
                    photonView.RPC("Transform_Stat", RpcTarget.All, 20, player_Stat.moveZone_Speed, false, true);
                    RestoreHp(recovery_Speed);
                    photonView.RPC("RestoreHp", RpcTarget.All, recovery_Speed);
                }
            }

            else if (teamZone.team == ETeam.Etc) //그 이외의 진영일 때
            {
                if (SquidForm)// 오징어 형태
                {
                    Transform_Stat(3, player_Stat.moveZone_Speed, true, false);
                    photonView.RPC("Transform_Stat", RpcTarget.All, 3, player_Stat.moveZone_Speed, true, false);

                }

                else //사람 형태
                {
                    Transform_Stat(3, player_Stat.moveZone_Speed, false, true);
                    photonView.RPC("Transform_Stat", RpcTarget.All, 3, player_Stat.moveZone_Speed, false, true);

                }

                for (int i = 0; i < player_Wave.Length - 1; i++)
                {
                    player_Wave[i].Stop();
                }
            }

            else //적 진영에 있을 때
            {
                if (SquidForm)// 오징어 형태
                {
                    Transform_Stat(0, player_Stat.enemyZone_Speed, true, false);
                    photonView.RPC("Transform_Stat", RpcTarget.All,0, player_Stat.enemyZone_Speed, true, false);

                }

                else //사람 형태
                {
                    Transform_Stat(0, player_Stat.enemyZone_Speed, false, true);
                    photonView.RPC("Transform_Stat", RpcTarget.All,0, player_Stat.enemyZone_Speed, false, true);

                }
            }
        }
        else //공중에 있을 때
        {
            _isJump = true;
            raycast_Object = null;

            if (SquidForm)// 오징어 형태
            {
                Transform_Stat(0, player_Stat.dashSpeed, true, false);
                photonView.RPC("Transform_Stat", RpcTarget.All, 0, player_Stat.dashSpeed, true, false);

            }

            else //사람 형태
            {
                Transform_Stat(0, player_Stat.moveZone_Speed, false, true);
                photonView.RPC("Transform_Stat", RpcTarget.All, 0, player_Stat.moveZone_Speed, false, true);

            }
        }

        if (player_Input.move_Vec == Vector3.zero || _isJump)
        {
            for (int i = 0; i < player_Wave.Length - 1; i++)
            {
                player_Wave[i].Stop();
            }
        }
    }                //Ray로 바닥 확인
    private void RaycastWall(bool SquidForm)
    {
        if (player_Input.move_Vec == transform.forward)
        {
            player_Input.isWall_Hor = false;
            player_Input.isWall_Left = false;
        }

        Wall_Vec(SquidForm, transform.forward);

        //else if (player_Input.move_Vec == -transform.right)
        //{
        //    player_Input.isWall_Hor = true;
        //    player_Input.isWall_Left = true;
        //    Wall_Vec(SquidForm, -transform.right);
        //}
        //else if (player_Input.move_Vec == transform.right )
        //{
        //    player_Input.isWall_Hor = true;
        //    player_Input.isWall_Left = false;
        //    Wall_Vec(SquidForm, transform.right);
        //}
    }                 //Ray로 벽 확인
    public void Wall_Vec(bool SquidForm, Vector3 dir)
    {
        if (!_Wall_RacastOn) { MoveWall(false, null); return; }

        Debug.DrawRay(transform.position, dir * player_Stat.detect_Range * 10, Color.green);

        if (Physics.Raycast(transform.position, dir, out RaycastHit forward_Hit, player_Stat.detect_Range * 10, player_Stat.floor_Layer))
        {
            MoveWall(true, forward_Hit.transform.gameObject);
            TeamZone teamZone = raycast_Wall_Object.GetComponent<TeamZone>();
            player_Input.isWall_Hor = false;
            if (teamZone.team != player_Team.team)
            {
                MoveWall(false, null);
                return;
            }

            else // 내 진영일 때
            {
                if (SquidForm)// 오징어 형태
                {
                    Transform_Stat(30, player_Stat.dashSpeed, false, false);
                    photonView.RPC("Transform_Stat", RpcTarget.All, 30, player_Stat.dashSpeed, false, false);
                    RestoreHp(recovery_Speed * 3);
                    photonView.RPC("RestoreHp", RpcTarget.All, recovery_Speed * 3);

                }

                else //사람 형태
                {
                    Transform_Stat(20, player_Stat.moveZone_Speed, false, true);
                    photonView.RPC("Transform_Stat", RpcTarget.All, 20, player_Stat.moveZone_Speed, false, true);
                    RestoreHp(recovery_Speed);
                }
            }

            if (player_Input.Move_Ver < 0) // 벽탈 때 뒤로 이동 시
            {
                _Wall_RacastOn = false;
                MoveWall(false, null);
            }
        }
        else // 레이캐스트 감지 안될 때
        {
            MoveWall(false, null);
        }
    }        //벽 위치 확인
    public void MoveWall(bool moveWall, GameObject wallObj)
    {
        player_Input.isWall = moveWall;
        _player_rigid.isKinematic = moveWall;
        _player_rigid.useGravity = !moveWall;
        raycast_Wall_Object = wallObj;
    }  //벽에 닿았을 시 상태 변환

    [PunRPC]
    public void Transform_Stat(int ammo, float speed, bool Squid, bool Human)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftShift))
            {
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    _player_shot.ammoBack_UI.transform.parent.gameObject.SetActive(true);
                    hitEffect[0].transform.localPosition = new Vector3(0, 0.4f, 0.46f);
                }

                if (Input.GetKeyUp(KeyCode.LeftShift))
                {
                    _player_shot.ammoBack_UI.transform.parent.gameObject.SetActive(false);
                    hitEffect[0].transform.localPosition = new Vector3(0, 1.5f, 0.46f);

                }
                ES_Manager.Stop_All_Sound_Effect();
                ES_Manager.Play_SoundEffect("Player_Hide");

                player_Wave[2].Play(); //1회만 실행되는 소환 이펙트
            }

            _player_shot.Reload_Ammo(ammo); // 재장전
            _player_shot.photonView.RPC("Reload_Ammo",RpcTarget.All, ammo); // 재장전
            _player_Speed = speed;

            Transform_Mesh(Squid, Human);   //형태 변형
            photonView.RPC("Transform_Mesh", RpcTarget.All, Squid, Human);   //형태 변형

            if (Human)
            {
                player_Wave[1].Stop();
                player_Wave[0].Play(); // 인간 발걸음
            }
            else
            {

                player_Wave[0].Stop();
                player_Wave[1].Play(); // 오징어 발걸음
            }

 
        }
      
    } //기본 상태 변환
    [PunRPC]
    public void Transform_Mesh(bool squid, bool human)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (GameObject obj in human_Object)
            {
                obj.SetActive(human);
            }
            squid_Object.SetActive(squid);
        }
  
    }
    #endregion

}
