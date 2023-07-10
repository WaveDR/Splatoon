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
    public Bullet dmgBullet; //���� �� ���� �������� ��������

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
        //ĳ���� �̵� / ȸ�� / ü�� / �Ѿ�
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
            //AI �̿� ui ����
            _player_shot.UI_Set_Server();
        }

        //Team Color ����
        player_Team.Player_ColorSet();
        //Weapon ����
        _player_shot.WeaponSet(player_Team.team);
    }
    private void FixedUpdate()
    {
        //1�� �� 10������ ����Ͽ� ��¡�� ���� ����ȭ
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
        //�̵� ��ġ ����ȭ ���̰� ���� �� �� ��ġ ���� ����
        else if ((transform.position - networkPosition).sqrMagnitude >= 100)
        {
            transform.position = networkPosition;
            transform.rotation = networkRotation;
        }
        //Ÿ �÷��̾� �̵������� ����
        else
        {
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 20f);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 20f);
        }

        //Ÿ �÷��̾� �Ѿ� �� ü�� ����
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
                if (isFalling) Respawn(player_Team.team, true); //�� ������ ������ ��
                if (isDead) Respawn(player_Team.team, false);   //���� �� ������

                else //���� �ʾ��� ��
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
        //��ƼŬ �ݶ��̴��� ������ ó��
        dmgBullet = other.GetComponent<Bullet>();

        if (dmgBullet.team.team != player_Team.team && !isDead)
        {
            OnDamage(dmgBullet.dmg);
        }

    }
    private void OnCollisionEnter(Collision collision)
    {
        //�ݶ��̴� ���ͷ� �� �ٱ����� ������ �� ������ ó��
        if (!photonView.IsMine) return;
        if (collision.gameObject.CompareTag("DeadLine"))
        {
            spawnTime = 3;
            isFalling = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        //Stay�� �� Ÿ�� ����
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
        //������ ������ �� �ڵ������� Ÿ�� null
        if (!photonView.IsMine) return;
        if (collision.gameObject.CompareTag("Wall")) // �ǹ� ��Ʈ�Ӹ��� ������ ��
        {
            MoveWall(false, null);
        }
    }

    //============================================        �� CallBack   |   Nomal ��        ========================================================

    #region Player Basic
    [PunRPC]
    public void Player_Set(ETeam team, EWeapon weapon, string name)
    {
        //�÷��̾� ��Ʈ��ũ Setting
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
        //��¡���� ȸ��
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
        //���� ���� Hit Effect Color ����

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
            //�÷��̾� �ǰ� ���� / �ִϸ��̼� / ����Ʈ
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
        //��� �� ���� ����
        Transform_Stat(0, 0, false, false);
        Transform_Mesh(false, false);

        //��� ����Ʈ
        deathEffect.particle.Play();
        ES_Manager.Play_SoundEffect("Player_Death");

        dmgBullet.Player_Kill(player_Input.player_Name); //��뿡�� �ߴ� ų�α�

        GameManager.Instance.Player_Dead_Check(); //Player Dead UI üũ;

        if(_enemy == null) //��� �� �� ���� ���
        StartCoroutine(_player_shot.Enemy_Data_UI(dmgBullet.player_Shot.player_Input.player_Name, dmgBullet.player_Shot.player_ScoreSet));
    }
    public override void Respawn(ETeam team, bool falling)
    {
        //�����ð�
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
                //�� ����
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

                //ī�޶� ��ȯ
                if (_enemy == null)
                {
                    GameManager.Instance.MapCam(false, _player_shot.playerCam.cam_Obj.gameObject);
                    for (int i = 0; i < _player_shot.hit_UI.Length; i++)
                    {
                        _player_shot.hit_UI[i].SetActive(false);
                    }
                }

                //������ �ð� �ʱ�ȭ
                spawnTime = 5f;
                plusTime = 0;

                //�÷��̾� ���� �ʱ�ȭ
                player_CurHealth = player_Stat.max_Heath;
                _player_shot.weapon.weapon_CurAmmo = _player_shot.weapon.weapon_MaxAmmo;
                Transform_Stat(0, player_Stat.moveZone_Speed, false, true);
                Transform_Mesh(false, true);
                //����
                isDead = false;
                GameManager.Instance.Player_Dead_Check(); //Player Dead UI;
                return;
            }
        }
        else //�� ������ ������ ��
        {
            if (plusTime >= spawnTime)
            {
                //�� ����
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
                //ī�޶� ��ȯ
                GameManager.Instance.MapCam(false, _player_shot.playerCam.cam_Obj.gameObject);
                for (int i = 0; i < _player_shot.hit_UI.Length; i++)
                {
                    _player_shot.hit_UI[i].SetActive(false);
                }
                //������ �ʱ�ȭ, ����
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

            if (teamZone.team == player_Team.team) // �� ������ ���� �ٴ� ������ ���� ��
            {
                if (SquidForm)// ��¡�� ����
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

                else //��� ����
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

            else if (teamZone.team == ETeam.Etc) //�� �̿��� ������ ��
            {
                if (SquidForm)// ��¡�� ����
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

                else //��� ����
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

            else //�� ������ ���� ��
            {
                if (SquidForm)// ��¡�� ����
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

                else //��� ����
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
        else //���߿� ���� ��
        {
            _isJump = true;

            raycast_Object = null;

            if (SquidForm)// ��¡�� ����
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

            else //��� ����
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
    }                //Ray�� �ٴ� Ȯ��
    private void RaycastWall(bool SquidForm)
    {
        if (player_Input.move_Vec == transform.forward)
        {
            player_Input.isWall_Hor = false;
            player_Input.isWall_Left = false;
        }

        Wall_Vec(SquidForm, transform.forward);
    }                 //Ray�� �� Ȯ��
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

            else // �� ������ ��
            {
                if (SquidForm)// ��¡�� ����
                {

                    RestoreHp(recovery_Speed * 3);

                    Transform_Stat(30, player_Stat.dashSpeed, false, false);

                    if (!_isHuman)
                    {
                        Transform_Mesh(false, false);
                        _isHuman = true;
                    }
                }

                else //��� ����
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

            if (player_Input.Move_Ver < 0) // ��Ż �� �ڷ� �̵� ��
            {
                _Wall_RacastOn = false;
                MoveWall(false, null);
            }
        }
        else // ����ĳ��Ʈ ���� �ȵ� ��
        {
            MoveWall(false, null);
        }
    }        //�� ��ġ Ȯ��
    public void MoveWall(bool moveWall, GameObject wallObj)
    {
        player_Input.isWall = moveWall;
        _player_rigid.isKinematic = moveWall;
        _player_rigid.useGravity = !moveWall;
        raycast_Wall_Object = wallObj;
    }  //���� ����� �� ���� ��ȯ

    public void Transform_Stat(int ammo, float speed, bool Squid, bool Human)
    {
        //�̵��ӵ� �� ������ �߰��� ����Ʈ / ���� ��ȯ ����Ʈ

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

            player_Wave[2].Play(); //1ȸ�� ����Ǵ� ��ȯ ����Ʈ
        }

        _player_shot.Reload_Ammo(ammo); // ������

        //�̵��ӵ� ����
        _player_Speed = speed;
        if (_enemy != null)
            _enemy.ai_nav.speed = speed * 1.5f;

        if (Human)
        {
            player_Wave[1].Stop();
            player_Wave[0].Play(); // �ΰ� �߰���
        }
        else
        {
            player_Wave[0].Stop();
            player_Wave[1].Play(); // ��¡�� �߰���
        }

    }


    //���º�ȯ �޼���
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
