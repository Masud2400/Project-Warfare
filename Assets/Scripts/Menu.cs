using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
	[SerializeField] private AudioMixer mainMixer;

    private bool isPaused;
	private bool isMuted = false;
	
	public void controlGameState()
	{
		if(isPaused)
		{
			ResumeGame();
			ToggleAudio();
		}
		else
		{
			PauseGame();
			ToggleAudio();
		}	
	}
	
	private void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    private void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
	
	public void ToggleAudio()
    {
        isMuted = !isMuted;
        
        float targetVolume = isMuted ? -80f : 0f; 
        
        mainMixer.SetFloat("MasterVolume", targetVolume);
    }
}
