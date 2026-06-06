using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource Walkingsource;
	[SerializeField] private AudioSource Sprintingsource;
	[SerializeField] private AudioSource shootingSource;
	[SerializeField] private AudioSource reloadingSource;
	[SerializeField] private AudioSource backgroundSource;

    [Header("Music Clips")]
    public AudioClip footsteps;
	public AudioClip runningSteps;
	public AudioClip shooting;
	public AudioClip reloading;
	public AudioClip backgroundSound;

    public void StartFootsteps()
    {
        if (!Walkingsource.isPlaying)
        {
            Walkingsource.clip = footsteps;
			Walkingsource.loop = true;
            Walkingsource.Play();
        }
    }

    public void StopFootsteps()
    {
        Walkingsource.Stop();
    }
	
	public void StartRunningSteps()
	{
		if(!Sprintingsource.isPlaying)
		{
			Sprintingsource.clip = runningSteps;
			Sprintingsource.loop = true;
			Sprintingsource.Play();
		}
	}
	
	public void StopRunningSteps()
	{
		Sprintingsource.Stop();
	}
	
	public void StartShootingAudio()
	{
		shootingSource.clip = shooting;
		shootingSource.loop = true;
		shootingSource.Play();
	}
	
	public void StopShootingAudio()
	{
		shootingSource.Stop();
	}
	
	public void ReloadAudio()
	{
		reloadingSource.clip = reloading;
		reloadingSource.Play();
	}
	
	public void PlayBackground()
	{
		backgroundSource.clip = backgroundSound;
		backgroundSource.loop = true;
		backgroundSource.Play();
	}
}
