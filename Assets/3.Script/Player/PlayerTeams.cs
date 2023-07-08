using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTeams : MonoBehaviour
{
    public ETeam team;
    [SerializeField] private PlayerController _player_Con;
    [SerializeField] private PlayerShooter _player_Shooter;

    [SerializeField] private ParticleSystem[] _player_Color_Par;
    [SerializeField] private Material[] _player_Yellow_Mat;
    [SerializeField] private Material[] _player_Blue_Mat;

    [SerializeField] private SkinnedMeshRenderer[] _human_Render;
    [SerializeField] private SkinnedMeshRenderer _squid_Render;


    public Color32 team_Yellow = new Color32(253, 242, 63, 255);
    public Color32 team_Blue = new Color32(129, 67, 255, 255);

    private void Awake()
    {
        TryGetComponent(out _player_Con);
        TryGetComponent(out _player_Shooter);
    }
    public void Player_ColorSet()
    {
        switch (team)
        {
            case ETeam.Blue:

                foreach (SkinnedMeshRenderer render in _human_Render)
                {
                    render.material = _player_Blue_Mat[0];
                    _squid_Render.material = _player_Blue_Mat[1];
                }


                foreach (ParticleSystem par in _player_Color_Par)
                {
                    var particle = par.main;
                    particle.startColor = (Color)team_Blue;

                }

                if (_player_Con.photonView.IsMine && _player_Con._enemy == null)
                {
                    _player_Shooter.ammoBack_UI.color = team_Blue;
                    _player_Shooter.ammoNot_UI.color = team_Blue;
                }
               
                break;

            case ETeam.Yellow:

                foreach (SkinnedMeshRenderer render in _human_Render)
                {
                    render.material = _player_Yellow_Mat[0];
                    _squid_Render.material = _player_Yellow_Mat[1];
                }


                foreach (ParticleSystem par in _player_Color_Par)
                {
                    var particle = par.main;
                    particle.startColor = (Color)team_Yellow;
                }

                if (_player_Con.photonView.IsMine && _player_Con._enemy == null)
                {
                    _player_Shooter.ammoBack_UI.color = team_Yellow;
                    _player_Shooter.ammoNot_UI.color = team_Yellow;
                }

                break;
        }
    }
}
