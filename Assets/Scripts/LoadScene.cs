using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider loadingBar;
	[SerializeField] private Menu menu;

    public void StartScene(string sceneName)
    {
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    public void RestartScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        StartCoroutine(LoadAsynchronously(currentSceneName));
		menu.controlGameState();
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
}
