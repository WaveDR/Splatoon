using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Sound
{
    public string sound_Name;
    public AudioClip clip;
}
public class Sound_Manager : MonoBehaviour
{
    public AudioSource[] audio_Source_Effects;

    public Sound[] effect_Sounds;
    public string[] play_SoundName;


    public void Play_SoundEffect(string name)
    {
        for (int i = 0; i < effect_Sounds.Length; i++)
        {
            if (name == effect_Sounds[i].sound_Name)
            {
                for (int j = 0; j < audio_Source_Effects.Length; j++)
                {
                    if (audio_Source_Effects[j].isPlaying)
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

 
    public void Stop_All_Sound_Effect()
    {
        for (int i = 0; i < audio_Source_Effects.Length; i++)
        {
            audio_Source_Effects[i].Stop();
        }
    }


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
