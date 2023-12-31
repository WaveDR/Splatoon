using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGM_Manager : MonoBehaviour
{
    //싱글톤 패턴
    public static BGM_Manager Instance = null;

    [Header("Use Sound Name")]
    public string[] play_SoundName;

    [Header("Declaration Sound Name")]
    public Sound[] bgm_Sounds;

    [Header("Use Sound Source")]
    public AudioSource[] audio_Source_Bgms;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Play_Sound_BGM("BGM_Lobby");

    }
    private void Start()
    {
        //가용 사운드 소스 배열 크기 초기화
        play_SoundName = new string[audio_Source_Bgms.Length];
    }

    //호출되는 매개변수 Sound Name을 찾아 재생
    public void Play_Sound_BGM(string name)
    {
        for (int i = 0; i < bgm_Sounds.Length; i++)
        {
            if (name == bgm_Sounds[i].sound_Name)
            {
                for (int j = 0; j < audio_Source_Bgms.Length; j++)
                {
                    if (!audio_Source_Bgms[j].isPlaying)
                    {
                        play_SoundName[j] = bgm_Sounds[i].sound_Name;
                        audio_Source_Bgms[j].clip = bgm_Sounds[i].clip;
                        audio_Source_Bgms[j].Play();
                        return;
                    }
                }
                Debug.Log("모든 가용 Audio Source가 사용 중 입니다!");
                return;
            }
        }
        Debug.Log(name + "Sound Manager에 등록되지 않은 SoundSource입니다!");
    }

    //BGM All Stop
    public void Stop_All_Sound_BGM()
    {
        for (int i = 0; i < audio_Source_Bgms.Length; i++)
        {
            audio_Source_Bgms[i].Stop();
        }
    }
}
