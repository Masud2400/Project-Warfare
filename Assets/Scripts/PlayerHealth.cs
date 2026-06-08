using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private float maxHealth = 20f;
    [SerializeField] private float currentHealth = 20f;
    [SerializeField] private float damageInterval = 1f;
    [SerializeField] private float healDelay = 5f;
	[SerializeField] private Image damageAlertImage;
	[SerializeField] private Animator anim;
	[SerializeField] private Transform mainCam;
	[SerializeField] private Image healthImage;

    private float damageTimer;
    private float healTimer;
    private bool tookDamageThisFrame;
	public Volume globalVolume;
    private DepthOfField depthOfField;
	private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        damageTimer = 0f; 
        healTimer = healDelay;
		
		if (globalVolume.profile.TryGet<DepthOfField>(out var dof))
        {
            depthOfField = dof;
            depthOfField.mode.value = DepthOfFieldMode.Off; 
        }
    }

    void Update()
    {
        if (!tookDamageThisFrame)
        {
            healTimer -= Time.deltaTime;
            if (healTimer <= 0f && currentHealth < maxHealth)
            {
                currentHealth = maxHealth;
				UpdateHealthColor();
            }
        }

        tookDamageThisFrame = false;
		
		if (currentHealth >= maxHealth)
		{
			damageAlertImage.gameObject.SetActive(false);
		}
		
		if(currentHealth < 0 && !isDead)
		{
			isDead = true;
			
			GetComponent<PlayerLook>().isEnabled = false;
			GetComponent<PlayerMotor>().isEnabled = false;
			
			Vector3 targetPosition = new Vector3(mainCam.transform.position.x, 0.2f, mainCam.transform.position.z);
			Quaternion targetRotation = Quaternion.Euler(0, mainCam.transform.eulerAngles.y, -90f);

			mainCam.transform.position = targetPosition;
			mainCam.transform.rotation = targetRotation;
			
			if (depthOfField != null)
            {
                depthOfField.mode.value = DepthOfFieldMode.Bokeh;
            }
			
			RestartScene();
		}
    }

    public void takeDamage()
    {
		if (isDead) return;
		
        tookDamageThisFrame = true;
        healTimer = healDelay; 

        if (damageTimer > 0)
        {
            damageTimer -= Time.deltaTime;
        }
        else
        {
            currentHealth -= 2;
            damageTimer = damageInterval; 
        }
		
		if (damageAlertImage != null)
		{
			damageAlertImage.gameObject.SetActive(true);

			float alpha = 1f - (currentHealth / maxHealth);

			Color color = damageAlertImage.color;
			color.a = alpha;
			damageAlertImage.color = color;
		}
		
		UpdateHealthColor();
    }
	
	private void UpdateHealthColor()
	{
        float fillRatio = currentHealth / maxHealth;
        
        fillRatio = Mathf.Clamp01(fillRatio);

        healthImage.fillAmount = fillRatio;

        healthImage.color = Color.Lerp(Color.red, Color.blue, fillRatio);
	}
	
	private void RestartScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        StartCoroutine(LoadMainScene(currentSceneName));
    }
	
	private IEnumerator LoadMainScene(string sceneName)
	{
		yield return new WaitForSeconds(3.5f);
		
		SceneManager.LoadScene(sceneName);
	}
}
