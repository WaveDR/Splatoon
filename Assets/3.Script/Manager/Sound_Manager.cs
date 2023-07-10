using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class Sound
{
    //���� �� ����Ǿ� �����Ǵ� Ŭ����
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
        //���� ���� �ҽ� �迭 ũ�� �ʱ�ȭ
        play_SoundName = new string[audio_Source_Effects.Length];
    }

    //ȣ��Ǵ� �Ű����� Sound Name�� ã�� ���
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
                Debug.Log("��� ���� Audio Source�� ��� �� �Դϴ�!");
                return;
            }
        }
        Debug.Log(name + "Sound Manager�� ��ϵ��� ���� SoundSource�Դϴ�!");
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
        Debug.Log("��� ����" + name + "Sound�� �����ϴ�!");
    }
}
