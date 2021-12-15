using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
     [SerializeField] private AudioMixer audioMixer;
     [SerializeField] private Slider masterSlider, musicSlider, sfxSlider;
     private const string PlayerPrefsMasterVolumeKey = "MasterVolume";
     private const string PlayerPrefsMusicVolumeKey = "MusicVolume";
     private const string PlayerPrefsSfxVolumeKey = "SFXVolume";

     private void Start()
     {
          if (PlayerPrefs.HasKey(PlayerPrefsMasterVolumeKey))
          {
               var value = PlayerPrefs.GetFloat(PlayerPrefsMasterVolumeKey);
               masterSlider.value = value;
               SetMasterLevel(value);
          }
          if (PlayerPrefs.HasKey(PlayerPrefsMusicVolumeKey))
          {
               var value = PlayerPrefs.GetFloat(PlayerPrefsMusicVolumeKey);
               musicSlider.value = value;
               SetMusicLevel(value);
          }
          if (PlayerPrefs.HasKey(PlayerPrefsSfxVolumeKey))
          {
               var value = PlayerPrefs.GetFloat(PlayerPrefsSfxVolumeKey);
               sfxSlider.value = value;
               SetSfxLevel(value);
          }
     }

     public void SetMasterLevel(float value)
     {
          audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
     }

     public void SetMusicLevel(float value)
     {
          audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
     }

     public void SetSfxLevel(float value)
     {
          audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
     }
}