using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTeams : MonoBehaviour
{
    public ETeam team;
    [SerializeField] private PlayerController _player_Con;
    [SerializeField] private PlayerShooter _player_Shooter;

    [SerializeField] private Material[] _player_Color_Mat;
    [SerializeField] private Bullet[] bullets;

    private void Awake()
    {
        TryGetComponent(out _player_Con);
        TryGetComponent(out _player_Shooter);
        Player_ColorSet();
    }


    public void Player_ColorSet()
    {
        switch (team)
        {
            case ETeam.Blue:

                foreach(Material mat in _player_Color_Mat)
                {
                    mat.color = Color.blue;
                }
                foreach (Bullet bullet in bullets)
                {
                    //bullet.paint.brush.splatChannel = 2;
                }

                break;

            case ETeam.Yellow:
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
