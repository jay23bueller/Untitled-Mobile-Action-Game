using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip hitAudioClip;


    private AudioSource hitManagerAudioSource;

    [SerializeField]
    private GameObject swordHitParticleSystemPrefab;

    public static HitManager Instance;
    
    private void Awake()
    {
        hitManagerAudioSource = gameObject.AddComponent<AudioSource>();
        Instance = this;
    }

    public void PlayHitSound()
    {
        hitManagerAudioSource.PlayOneShot(hitAudioClip);
    }

    public void PlayWeaponSound(AudioClip weaponSound)
    {
        hitManagerAudioSource.PlayOneShot(weaponSound);
    }

    public void PlaySwordHitEffect(Vector3 hitLocation)
    {
        Instantiate(swordHitParticleSystemPrefab, hitLocation, Quaternion.identity);
    }
}
