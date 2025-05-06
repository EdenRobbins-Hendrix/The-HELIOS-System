using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider soundFXSlider;

    public void Start()
    {
        if (!PlayerPrefs.HasKey("masterVolume"))
        {
            PlayerPrefs.SetFloat("masterVolume", 1);
            Load();
        }
        else if (!PlayerPrefs.HasKey("soundFXVolume"))
        {
            PlayerPrefs.SetFloat("soundFXVolume", 1);
            Load();
        }
        else if (!PlayerPrefs.HasKey("musicVolume"))
        {
            PlayerPrefs.SetFloat("musicVolume", 1);
            Load();
        }
        else
        {
            Load();
        }
    }

    public void SetMasterVolume(float level)
    {
        audioMixer.SetFloat("masterVolume", Mathf.Log10(level) * 20f);
        Save();
    }

    public void SetFXVolume(float level)
    {
        audioMixer.SetFloat("soundFXVolume", Mathf.Log10(level) * 20f);
        Save();
    }

    public void SetMusicVolume(float level)
    {
        audioMixer.SetFloat("musicVolume", Mathf.Log10(level) * 20f);
        Save();
    }

    public void Save()
    {
        PlayerPrefs.SetFloat("masterVolume", masterSlider.value);
        PlayerPrefs.SetFloat("soundFXVolume", soundFXSlider.value);
        PlayerPrefs.SetFloat("musicVolume", musicSlider.value);
    }

    public void Load()
    {
        masterSlider.value = PlayerPrefs.GetFloat("masterVolume");
        soundFXSlider.value = PlayerPrefs.GetFloat("soundFXVolume");
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
    }
}