using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundPrefab : NetworkBehaviour
{
     [SerializeField] private AudioSource audioSource;

     [SerializeField] private List<AudioClip> tankShootSounds = new List<AudioClip>();
     [SerializeField] private List<AudioClip> turretShootSounds = new List<AudioClip>();
     [SerializeField] private List<AudioClip> robotShootSounds = new List<AudioClip>();
     [SerializeField] private List<AudioClip> tankExplosionSounds = new List<AudioClip>();
     [SerializeField] private List<AudioClip> tankBulletExplosionSounds = new List<AudioClip>();
     [SerializeField] private List<AudioClip> robotExplosionSounds = new List<AudioClip>();

     [ServerCallback]
     private void Awake()
     {
          StartCoroutine(Destroy());
     }

     private IEnumerator Destroy()
     {
          yield return new WaitForSeconds(3f);
          NetworkServer.Destroy(gameObject);
     }

     [ClientRpc]
     public void RpcPlayTankShootSound()
     {
          if (tankShootSounds == null) return;
          audioSource.clip = tankShootSounds[Random.Range(0, tankShootSounds.Count)];
          audioSource.pitch = Random.Range(0.8f, 1.2f);
          audioSource.Play();
     }
     [ClientRpc]
     public void RpcPlayTurretShootSound()
     {
          if (turretShootSounds == null) return;
          audioSource.clip = turretShootSounds[Random.Range(0, turretShootSounds.Count)];
          audioSource.pitch = Random.Range(0.8f, 1.2f);
          audioSource.volume = 0.5f;
          audioSource.Play();
     }
     [ClientRpc]
     public void RpcPlayRobotShootSound()
     {
          if (robotShootSounds == null) return;
          audioSource.clip = robotShootSounds[Random.Range(0, robotShootSounds.Count)];
          audioSource.pitch = Random.Range(0.8f, 1.2f);
          audioSource.volume = 0.2f;
          audioSource.Play();
     }
     [ClientRpc]
     public void RpcPlayTankExplosionSound()
     {
          if (tankExplosionSounds == null) return;
          audioSource.clip = tankExplosionSounds[Random.Range(0, tankExplosionSounds.Count)];
          audioSource.pitch = Random.Range(0.8f, 1.2f);
          audioSource.Play();
     }
     [ClientRpc]
     public void RpcPlayRobotExplosionSound()
     {
          if (robotExplosionSounds == null) return;
          audioSource.clip = robotExplosionSounds[Random.Range(0, robotExplosionSounds.Count)];
          audioSource.pitch = Random.Range(0.8f, 1.2f);
          audioSource.volume = 0.2f;
          audioSource.Play();
     }
     [ClientRpc]
     public void RpcPlayTankBulletExplosionSound()
     {
          if (tankBulletExplosionSounds == null) return;
          audioSource.clip = tankBulletExplosionSounds[Random.Range(0, tankBulletExplosionSounds.Count)];
          audioSource.pitch = Random.Range(0.8f, 1.2f);
          audioSource.volume = 0.35f;
          audioSource.Play();
     }
}