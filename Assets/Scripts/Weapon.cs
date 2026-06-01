using UnityEngine;
using TMPro;

public class Weapon : MonoBehaviour
{
	private enum State
	{
		Idle,
		Shooting,
		Running,
		Walking,
		Reloading
	}
	
	private Animator anim;
	private bool shootingState;
	private int bulletCount = 50;
	AudioManager audioManager;
	private State currentState = State.Idle;
	
	[Header ("Game objects")]
    [SerializeField] private Camera mainCamera;
	[SerializeField] private PlayerMotor motorScript;
	[SerializeField] private TMP_Text bullets;
    
    [Header("Weapon settings")]
    [SerializeField] private float range = 100f;
	[SerializeField] private ParticleSystem muzzleFlash;
	
	void Awake()
	{
		audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
	}
	
	void Start()
    {
        anim = GetComponentInChildren<Animator>();
		bullets.text = bulletCount.ToString();
    }
	
	void Update()
	{	
		DetermineState();
	}
	
	private void DetermineState()
	{
		if(shootingState)
		{
			TransitionToState(State.Shooting);
		}
		else if(motorScript.runningState)
		{
			TransitionToState(State.Running);
		}
		else if(motorScript.walkingState)
		{
			TransitionToState(State.Walking);
		}
		else
		{
			TransitionToState(State.Idle);
		}
	}
	
	private void TransitionToState(State newState)
	{
		if (currentState == newState) return;
		
		currentState = newState;
		
		if(currentState == State.Idle)
		{
			anim.SetFloat("IdleWalk", 0);
		}
		else if(currentState == State.Walking)
		{
			anim.SetFloat("IdleWalk", 1);
		}
		
		if(anim != null)
		{
			anim.SetBool("isShooting", currentState == State.Shooting);
			anim.SetBool("isRunning", currentState == State.Running);
		}
	}
    
    public void Shoot()
    {
		if (bulletCount <= 0)
		{
			StopShooting();
			return;
		}
		
		if (!shootingState)
		{
			shootingState = true;
			audioManager.StartShootingAudio();
		}
		
        RaycastHit hit;
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, range))
        {   
            Enemy enemy = hit.transform.GetComponent<Enemy>();
            
            if (enemy != null)
            {
				enemy.takeDamage();
            }
        }
		muzzleFlash.Play();
		
		if (bulletCount > 0)
        {
            bulletCount -= 1;
            bullets.text = bulletCount.ToString();
        }
    }
	
	public void StopShooting()
	{
		shootingState = false;
		audioManager.StopShootingAudio();
	}
	
	public void Reload()
	{
		currentState = State.Reloading;
		
		audioManager.ReloadAudio();
		
		if(anim != null)
		{
			anim.SetTrigger("Reload");
		}
		
		bulletCount = 50;
		bullets.text = bulletCount.ToString();
	}
}
