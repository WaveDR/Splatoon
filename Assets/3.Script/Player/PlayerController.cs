using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : Living_Entity, IPlayer, IPunObservable
{
    [Header("Player Stat")]
    [Header("===================================")]

    public PlayerTeams player_Team;
    public bool _isJump;
    public bool isWall;

    [Header("Player Move")]
    [Header("===================================")]

    [SerializeField] private GameObject raycast_Object;
    [SerializeField] private GameObject raycast_Wall_Object;
    [SerializeField] private ParticleSystem[] player_Wave;

    [Header("Squidform")]
    [Header("===================================")]

    [SerializeField] private GameObject[] human_Object;
    [SerializeField] private GameObject squid_Object;


    [Header("Player Component")]
    [Header("===================================")]
    private Rigidbody _player_rigid;
    private Animator _player_Anim;
    public PlayerInput player_Input;
    public PlayerShooter _player_shot;
    public Enemy_Con _enemy;

    //Player Data
    private float _player_Speed;
    private bool _Wall_RacastOn;
    private bool _isHuman;
    private bool _isFloor;
    public Bullet dmgBullet; //맞을 때 받을 데미지값 가져오기

    private float spawnTime = 5;
    private float plusTime;

    public Bullet deathEffect;
    public Sound_Manager ES_Manager;

    //Network Data
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private float network_Hp;
    private float network_Ammo;

    void Awake()
    {
        //GetComponent =====================================================================
        TryGetComponent(out player_Input);
        TryGetComponent(out _player_rigid);
        TryGetComponent(out _player_Anim);
        TryGetComponent(out _player_shot);
        TryGetComponent(out player_Team);
        TryGetComponent(out _enemy);
        ES_Manager = GetComponentInChildren<Sound_Manager>();
        deathEffect = transform.GetChild(transform.childCount - 1).GetComponent<Bullet>();
        for (int i = 1; i < hitEffect.Length; i++)
        {
            hitEffect[i] = hitEffect[0].transform.GetChild(i).GetComponent<ParticleSystem>();
        }

        //Player Info Reset ================================================================
        foreach (ParticleSystem start in player_Wave) start.Stop();
        _player_Speed = player_Stat.moveZone_Speed;
        player_CurHealth = player_Stat.max_Heath;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //캐릭터 이동 / 회전 / 체력 / 총알
        if (stream.IsWriting)
        {
            stream.SendNext(this.gameObject.transform.position);
            stream.SendNext(this.gameObject.transform.rotation);
            stream.SendNext(this.player_CurHealth);
            stream.SendNext(this._player_shot.weapon.weapon_CurAmmo);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            network_Hp = (float)stream.ReceiveNext();
            network_Ammo = (float)stream.ReceiveNext();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (_enemy == null)
        {
            //AI 이외 ui 적용
            _player_shot.UI_Set_Server();
        }

        //Team Color 적용
        player_Team.Player_ColorSet();
        //Weapon 적용
        _player_shot.WeaponSet(player_Team.team);
    }
    private void FixedUpdate()
    {
        //1초 당 10프레임 계산하여 오징어 상태 최적화
        RaycastFloor(player_Input.squid_Form);

        if (!player_Input.squid_Form)
        {
            _Wall_RacastOn = false;
            MoveWall(false, null);
        }
        else
        {
            RaycastWall(player_Input.squid_Form);
        }

        
        if (photonView.IsMine)
        {
            _player_rigid.MovePosition(_player_rigid.position + player_Input.move_Vec * _player_Speed * Time.deltaTime);
        }
        //이동 위치 동기화 차이가 많이 날 시 위치 강제 조정
        else if ((transform.position - networkPosition).sqrMagnitude >= 100)
        {
            transform.position = networkPosition;
            transform.rotation = networkRotation;
        }
        //타 플레이어 이동보간법 적용
        else
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 20f);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 20f);
        }

        //타 플레이어 총알 및 체력 적용
        if (!photonView.IsMine)
        {
            player_CurHealth = network_Hp;
            _player_shot.weapon.weapon_CurAmmo = network_Ammo;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            if (!isStop)
            {
                if (isFalling) Respawn(player_Team.team, true); //맵 밖으로 떨어질 때
                if (isDead) Respawn(player_Team.team, false);   //죽을 때 리스폰

                else //죽지 않았을 때
                {
                    Hit_UI();
                    Player_Movement();
                    Player_Jump();
                    Player_Animation();
                }
            }
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        //파티클 콜라이더로 데미지 처리
        dmgBullet = other.GetComponent<Bullet>();

        if (dmgBullet.team.team != player_Team.team && !isDead)
        {
            OnDamage(dmgBullet.dmg);
        }

    }
    private void OnCollisionEnter(Collision collision)
    {
        //콜라이더 엔터로 맵 바깥으로 떨어질 시 리스폰 처리
        if (!photonView.IsMine) return;
        if (collision.gameObject.CompareTag("DeadLine"))
        {
            spawnTime = 3;
            isFalling = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        //Stay로 벽 타겟 감지
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
        //벽에서 떨어질 시 자동적으로 타겟 null
        if (!photonView.IsMine) return;
        if (collision.gameObject.CompareTag("Wall")) // 건물 끄트머리에 도달할 시
        {
            MoveWall(false, null);
        }
    }

    //============================================        ↑ CallBack   |   Nomal ↓        ========================================================

    #region Player Basic
    [PunRPC]
    public void Player_Set(ETeam team, EWeapon weapon, string name)
    {
        //플레이어 네트워크 Setting
        player_Team.team = team;
        _player_shot.WeaponType = weapon;
        player_Input.player_Name = name;
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
        //오징어폼 회전
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
        Squid_Eular();
    }
    #endregion

    #region Override
    public override void OnDamage(float damage)
    {
        base.OnDamage(damage);
        //팀에 따른 Hit Effect Color 변경

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
            //플레이어 피격 사운드 / 애니메이션 / 이펙트
            ES_Manager.Play_SoundEffect("Player_Hit");
            _player_Anim.SetTrigger("Hit");
            hitEffect[0].Play();
        }

    }
    public override void RestoreHp(float newHealth)
    {
        base.RestoreHp(newHealth);

    }
    public override void Player_Die()
    {
        base.Player_Die();
        //사망 시 상태 변경
        Transform_Stat(0, 0, false, false);
        Transform_Mesh(false, false);

        //사망 이펙트
        deathEffect.particle.Play();
        ES_Manager.Play_SoundEffect("Player_Death");

        dmgBullet.Player_Kill(player_Input.player_Name); //상대에게 뜨는 킬로그

        GameManager.Instance.Player_Dead_Check(); //Player Dead UI 체크;

        if(_enemy == null) //사망 시 적 정보 출력
        StartCoroutine(_player_shot.Enemy_Data_UI(dmgBullet.player_Shot.player_Input.player_Name, dmgBullet.player_Shot.player_ScoreSet));
    }
    public override void Respawn(ETeam team, bool falling)
    {
        //지연시간
        plusTime += Time.deltaTime;

        if (plusTime >= 1)
        {
            if (_enemy == null)
                _player_shot.playerCam.cam_Obj.gameObject.SetActive(false);
        }
        if (_enemy == null)
            GameManager.Instance.MapCam(true, _player_shot.playerCam.cam_Obj.gameObject);

        if (!falling)
        {
            if (plusTime >= spawnTime)
            {
                //팀 구분
                if (team == ETeam.Yellow)
                {
                    transform.rotation = Quaternion.identity;
                    transform.position = GameManager.Instance.team_Yellow_Spawn[Random.Range(0, 3)].position;
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                    transform.position = GameManager.Instance.team_Blue_Spawn[Random.Range(0, 3)].position;
                }

                //카메라 전환
                if (_enemy == null)
                {
                    GameManager.Instance.MapCam(false, _player_shot.playerCam.cam_Obj.gameObject);
                    for (int i = 0; i < _player_shot.hit_UI.Length; i++)
                    {
                        _player_shot.hit_UI[i].SetActive(false);
                    }
                }

                //리스폰 시간 초기화
                spawnTime = 5f;
                plusTime = 0;

                //플레이어 스탯 초기화
                player_CurHealth = player_Stat.max_Heath;
                _player_shot.weapon.weapon_CurAmmo = _player_shot.weapon.weapon_MaxAmmo;
                Transform_Stat(0, player_Stat.moveZone_Speed, false, true);
                Transform_Mesh(false, true);
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
                    transform.position = GameManager.Instance.team_Yellow_Spawn[Random.Range(0, 3)].position;
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                    transform.position = GameManager.Instance.team_Blue_Spawn[Random.Range(0, 3)].position;
                }
                //카메라 전환
                GameManager.Instance.MapCam(false, _player_shot.playerCam.cam_Obj.gameObject);
                for (int i = 0; i < _player_shot.hit_UI.Length; i++)
                {
                    _player_shot.hit_UI[i].SetActive(false);
                }
                //리스폰 초기화, 리턴
                spawnTime = 5f;
                plusTime = 0;
                isFalling = false;
                return;
            }
        }
    }
    #endregion

    #region Player Info UI
    public void Hit_UI()
    {
        if(_enemy == null)
        {
            if (player_CurHealth > 0)
            {
                if (player_CurHealth < 50)
                {
                    _player_shot.hit_UI[0].SetActive(true);
                }
                else _player_shot.hit_UI[0].SetActive(false);

                if (player_CurHealth < 30)
                {
                    _player_shot.hit_UI[1].SetActive(true);
                }
                else _player_shot.hit_UI[1].SetActive(false);

                if (player_CurHealth < 10)
                {
                    _player_shot.hit_UI[2].SetActive(true);
                }
                else _player_shot.hit_UI[2].SetActive(false);
            }
        }
    }

    public void UI_On_Off(bool on)
    {
        _player_shot.skill_UI_Obj.SetActive(on);
    }

    #endregion

    #region Player Raycast
    private void RaycastFloor(bool SquidForm)
    {
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
                    if (_isFloor)
                    {
                        _isHuman = false;
                        _isFloor = false;
                    }

                    Transform_Stat(30, player_Stat.dashSpeed, false, false);
                    RestoreHp(recovery_Speed * 3);


                    if (!_isHuman)
                    {
                        Transform_Mesh(false, false);
                        _isHuman = true;
                    }

                }

                else //사람 형태
                {
                    Transform_Stat(20, player_Stat.moveZone_Speed, false, true);

                    RestoreHp(recovery_Speed);

                    if (_isHuman)
                    {
                        Transform_Mesh(false, true);
                        _isHuman = false;
                    }
                }
            }

            else if (teamZone.team == ETeam.Etc) //그 이외의 진영일 때
            {
                if (SquidForm)// 오징어 형태
                {
                    if (!_isFloor)
                    {
                        _isHuman = false;
                        _isFloor = true;
                    }

                    Transform_Stat(3, player_Stat.moveZone_Speed, true, false);
                    if (!_isHuman)
                    {
                        Transform_Mesh(true, false);
                        _isHuman = true;
                    }

                }

                else //사람 형태
                {
                    Transform_Stat(3, player_Stat.moveZone_Speed, false, true);
                    if (_isHuman)
                    {
                        Transform_Mesh(false, true);
                        _isHuman = false;
                    }
                    _isFloor = false;

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

                    if (!_isFloor)
                    {
                        _isHuman = false;
                        _isFloor = true;
                    }

                    Transform_Stat(0, player_Stat.enemyZone_Speed, true, false);

                    if (!_isHuman)
                    {
                        Transform_Mesh(true, false);
                        _isHuman = true;
                    }

                }

                else //사람 형태
                {
                    Transform_Stat(0, player_Stat.enemyZone_Speed, false, true);

                    if (_isHuman)
                    {
                        Transform_Mesh(false, true);
                        _isHuman = false;
                    }
                }
            }
        }
        else //공중에 있을 때
        {
            _isJump = true;

            raycast_Object = null;

            if (SquidForm)// 오징어 형태
            {
                if (!_isFloor)
                {
                    _isHuman = false;
                    _isFloor = true;
                }

                Transform_Stat(0, player_Stat.dashSpeed, true, false);

                if (!_isHuman)
                {
                    Transform_Mesh(true, false);
                    _isHuman = true;
                }
            }

            else //사람 형태
            {
                Transform_Stat(0, player_Stat.moveZone_Speed, false, true);
                if (_isHuman)
                {
                    Transform_Mesh(false, true);
                    _isHuman = false;
                }

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
    }                 //Ray로 벽 확인
    public void Wall_Vec(bool SquidForm, Vector3 dir)
    {
        if (!_Wall_RacastOn) { MoveWall(false, null); return; }

        if (Physics.Raycast(transform.position, dir, out RaycastHit forward_Hit, player_Stat.detect_Range * 3, player_Stat.floor_Layer))
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

                    RestoreHp(recovery_Speed * 3);

                    Transform_Stat(30, player_Stat.dashSpeed, false, false);

                    if (!_isHuman)
                    {
                        Transform_Mesh(false, false);
                        _isHuman = true;
                    }
                }

                else //사람 형태
                {
                    Transform_Stat(20, player_Stat.moveZone_Speed, false, true);

                    RestoreHp(recovery_Speed);

                    if (_isHuman)
                    {
                        Transform_Mesh(false, true);
                        _isHuman = false;
                    }
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

    public void Transform_Stat(int ammo, float speed, bool Squid, bool Human)
    {
        //이동속도 및 재장전 발걸음 이펙트 / 형태 변환 이펙트

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftShift)) && _enemy == null)
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
                _isFloor = false;
            }
            ES_Manager.Stop_All_Sound_Effect();
            ES_Manager.Play_SoundEffect("Player_Hide");

            player_Wave[2].Play(); //1회만 실행되는 소환 이펙트
        }

        _player_shot.Reload_Ammo(ammo); // 재장전

        //이동속도 증가
        _player_Speed = speed;
        if (_enemy != null)
            _enemy.ai_nav.speed = speed * 1.5f;

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


    //형태변환 메서드
    public void Transform_Mesh(bool squid, bool human)
    {
        photonView.RPC("TransSquid_Server", RpcTarget.All, squid, human);
    }
    [PunRPC]
    public void TransSquid_Server(bool squid, bool human)
    {
     
        foreach (GameObject obj in human_Object)
        {
            obj.SetActive(human);
        }
        squid_Object.SetActive(squid);
    }
    #endregion
}
