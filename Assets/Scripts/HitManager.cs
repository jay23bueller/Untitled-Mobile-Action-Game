using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HitManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip hitAudioClip;

    private static HitManager _instance;

    public static HitManager Instance
    {
        get { return _instance; }
    }

    private AudioSource hitManagerAudioSource;

    [SerializeField]
    private AudioClip[] walkingFootstepAudioClips;

    [SerializeField]
    private AudioClip[] runningFootstepAudioClips;

    [SerializeField]
    private GameObject swordHitParticleSystemPrefab;

    private void Awake()
    {
        _instance = this;
        hitManagerAudioSource = gameObject.AddComponent<AudioSource>();
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

    public void PlayFootstepSound(Vector3 footstepPosition, bool isRunning)
    {
        AudioClip selectedAudioClip;
        if (!isRunning)
            selectedAudioClip = walkingFootstepAudioClips[Random.Range(0, walkingFootstepAudioClips.Length)];
        else
            selectedAudioClip = runningFootstepAudioClips[Random.Range(0, runningFootstepAudioClips.Length)];

        AudioSource.PlayClipAtPoint(selectedAudioClip,footstepPosition);
    }

}
