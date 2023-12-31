using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class Player_SettingUI : MonoBehaviour
{
    [Header("Player Audio")]
    public AudioMixer audio_Mixer;

    [Header("Player UI")]
    public Slider bgm_Slider;
    public Slider sfx_Slider;
    public GameObject SettingUI;

    private void Awake()
    {
        bgm_Slider.value = 0.5f;
        sfx_Slider.value = 0.5f;
    }

    public void Set_BGM_Volme()
    {
        audio_Mixer.SetFloat("BGM", Mathf.Log10(bgm_Slider.value) * 20);
    }

    public void Set_SFX_Volme()
    {
        audio_Mixer.SetFloat("SFX", Mathf.Log10(sfx_Slider.value) * 20);
    }

    public void SettingOpen()
    {
        SettingUI.SetActive(true);
    }

    public void SettingClose()
    {
        SettingUI.SetActive(false);
    }
}
