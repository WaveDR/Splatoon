using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class Sound
{
    //게임 내 선언되어 생성되는 클래스
    public string sound_Name;
    public AudioClip clip;
}

public class Sound_Manager : MonoBehaviour
{
    //SFX 
    [Header("Use Sound Name")]
    public string[] play_SoundName;

    [Header("Declaration Sound Name")]
    public Sound[] effect_Sounds;

    [Header("Use Sound Source")]
    public AudioSource[] audio_Source_Effects;

    private void Start()
    {
        //가용 사운드 소스 배열 크기 초기화
        play_SoundName = new string[audio_Source_Effects.Length];
    }

    //호출되는 매개변수 Sound Name을 찾아 재생
    public void Play_SoundEffect(string name)
    {
        for (int i = 0; i < effect_Sounds.Length; i++)
        {
            if (name == effect_Sounds[i].sound_Name)
            {

                for (int j = 0; j < audio_Source_Effects.Length; j++)
                {
                    if (!audio_Source_Effects[j].isPlaying)
                    {
                        play_SoundName[j] = effect_Sounds[i].sound_Name;
                        audio_Source_Effects[j].clip = effect_Sounds[i].clip;
                        audio_Source_Effects[j].Play();
                        return;
                    }
                }
                Debug.Log("모든 가용 Audio Source가 사용 중 입니다!");
                return;
            }
        }
        Debug.Log(name + "Sound Manager에 등록되지 않은 SoundSource입니다!");
    }

    //Stop All Sfx
    public void Stop_All_Sound_Effect()
    {
        for (int i = 0; i < audio_Source_Effects.Length; i++)
        {
            audio_Source_Effects[i].Stop();
        }
    }

    //Stop Sfx
    public void Stop_Sound_Effect(string name)
    {
        for (int i = 0; i < effect_Sounds.Length; i++)
        {
            if(play_SoundName[i] == name)
            {
                audio_Source_Effects[i].Stop();
                break;
            }
        }
        Debug.Log("재생 중인" + name + "Sound가 없습니다!");
    }
}
