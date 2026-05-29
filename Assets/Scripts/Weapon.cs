using UnityEngine;

public class Weapon : MonoBehaviour
{
	private enum State
	{
		Idle,
		Shooting
	}
	
	private Animator anim;
	private bool shootingState;
	private State currentState = State.Idle;
	
	[Header ("Game objects")]
    [SerializeField] private Camera mainCamera;
    
    [Header("Weapon settings")]
    [SerializeField] private float range = 100f;
	
	void Start()
    {
        anim = GetComponentInChildren<Animator>();
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
		else
		{
			TransitionToState(State.Idle);
		}
	}
	
	private void TransitionToState(State newState)
	{
		if (currentState == newState) return;
		
		currentState = newState;
		
		if(anim != null)
		{
			anim.SetBool("isShooting", currentState == State.Shooting);
		}
	}
    
    public void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, range))
        {   
            Enemy enemy = hit.transform.GetComponent<Enemy>();
            
            if (enemy != null)
            {
				enemy.takeDamage();
            }
        }
		shootingState = true;
    }
	
	public void StopShooting()
	{
		shootingState = false;
	}
}
