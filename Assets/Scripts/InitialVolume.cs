using System;
using UnityEngine;
using UnityEngine.Audio;

public class InitialVolume : MonoBehaviour
{
     [SerializeField] private AudioMixer audioMixer;
     
     private const string PlayerPrefsMasterVolumeKey = "MasterVolume";
     private const string PlayerPrefsMusicVolumeKey = "MusicVolume";
     private const string PlayerPrefsSfxVolumeKey = "SFXVolume";

     private void Start()
     {
          if (PlayerPrefs.HasKey(PlayerPrefsMasterVolumeKey))
          {
               var value = PlayerPrefs.GetFloat(PlayerPrefsMasterVolumeKey);
               SetMasterLevel(value);
          }
          if (PlayerPrefs.HasKey(PlayerPrefsMusicVolumeKey))
          {
               var value = PlayerPrefs.GetFloat(PlayerPrefsMusicVolumeKey);
               SetMusicLevel(value);
          }
          if (PlayerPrefs.HasKey(PlayerPrefsSfxVolumeKey))
          {
               var value = PlayerPrefs.GetFloat(PlayerPrefsSfxVolumeKey);
               SetSfxLevel(value);
          }
     }


     private void SetMasterLevel(float value)
     {
          audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
     }

     private void SetMusicLevel(float value)
     {
          audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
     }

     private void SetSfxLevel(float value)
     {
          audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
     }
}