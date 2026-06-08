using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FirstMenu : MonoBehaviour
{
	[Header("UI Elements")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingBar;
	
    public void LoadMainScene()
    {
		StartCoroutine(LoadAsynchronously("MainScene"));
		Time.timeScale = 1f;
    }
	
	private IEnumerator LoadAsynchronously(string sceneName)
    {
        loadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            loadingBar.value = progress;

            yield return null;
        }
    }
	
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
