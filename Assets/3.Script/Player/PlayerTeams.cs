using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTeams : MonoBehaviour
{
    public ETeam team;
    [SerializeField] private PlayerController _player_Con;
    [SerializeField] private PlayerShooter _player_Shooter;

    [SerializeField] private ParticleSystem[] _player_Color_Par;
    [SerializeField] private Material[] _player_Color_Mat;

    Color32 team_Yellow = new Color32(253, 242, 63, 255);
    Color32 team_Blue = new Color32(129, 67, 255, 255);

    private void Awake()
    {
        TryGetComponent(out _player_Con);
        TryGetComponent(out _player_Shooter);
    }
    private void OnEnable()
    {
        Player_ColorSet();
    }

    public void Player_ColorSet()
    {
        switch (team)
        {
            case ETeam.Blue:

                foreach(Material mat in _player_Color_Mat)
                {
                    mat.color = team_Blue;
                }

                foreach (ParticleSystem par in _player_Color_Par)
                {
                    var particle = par.main;
                    particle.startColor = (Color)team_Blue;
                   
                }
                break;

            case ETeam.Yellow:

                foreach (Material mat in _player_Color_Mat)
                {
                    mat.color = team_Yellow;
                }
                foreach (ParticleSystem par in _player_Color_Par)
                {
                    var particle = par.main;
                    particle.startColor = (Color)team_Yellow;
                }

                break;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
