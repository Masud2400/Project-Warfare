using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
	private enum State
    {
        Running,
        Attacking,
        StandingShooting,
        CrouchShooting,
        Dead
    }
	
    private Animator anim;
	private NavMeshAgent agent;
	private AudioSource audioSource;
	private State currentState = State.Running;
	private float stanceTimer = 0f;
	private bool muzzleFlashPlaying = false;
	
	private ModelSpawner spawner;
	private Transform mySpawnPoint;
	
	private bool isDead = false;

	[Header ("Game Objects")]
    [SerializeField] private GameObject player;
	[SerializeField] private PlayerHealth playerHealthScript;
	[SerializeField] private CharacterController playerController;
	[SerializeField] private ParticleSystem muzzleFlash;
	
	[Header ("Settings")]
	[SerializeField] private float DetectionRangeShooting = 5f;
	[SerializeField] private float DetectionRangeAttacking = 2f;
	[SerializeField] private float health = 100f;
	[SerializeField] private float stanceChangeCooldown = 1.5f;
	
	[Header ("Audio Clips")]
	[SerializeField] private AudioClip shootClip;
	[SerializeField] private AudioClip deathClip;
	[SerializeField] private AudioClip attackClip;

	void Awake()
	{
		player = GameObject.FindGameObjectWithTag("Player");

		if (player != null)
		{
			playerHealthScript = player.GetComponent<PlayerHealth>();
			playerController = player.GetComponent<CharacterController>();
		}
		
		audioSource = GetComponent<AudioSource>();
	}

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
		agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {	
		if(currentState == State.Dead)
		{
			if (anim.GetCurrentAnimatorStateInfo(0).IsName("Dying"))
			{
				anim.applyRootMotion = true;
			}
			
			return;
		}
		
		if (stanceTimer > 0)
		{
			stanceTimer -= Time.deltaTime;
		}
		
		bool atAttackingRange = DetectPlayer(DetectionRangeAttacking);
        bool atShootingRange = DetectPlayer(DetectionRangeShooting);
		
		DetermineState(atAttackingRange, atShootingRange);

        ExecuteCurrentState();
		
		ControlAudio();
    }
	
	private void DetermineState(bool atAttackingRange, bool atShootingRange)
	{	
		if (atAttackingRange)
		{
			TransitionToState(State.Attacking);
			muzzleFlashPlaying = false;
			muzzleFlash.Stop();
			return; 
		}

		if (stanceTimer > 0 && (currentState == State.StandingShooting || currentState == State.CrouchShooting))
		{
			return; 
		}

		if (atShootingRange)
		{
			if (currentState != State.StandingShooting && currentState != State.CrouchShooting)
			{
				State randomShootState = (Random.Range(0, 2) == 0) ? State.StandingShooting : State.CrouchShooting;
				TransitionToState(randomShootState);
				
				stanceTimer = stanceChangeCooldown;
				  
				if (muzzleFlashPlaying == false)
				{
					muzzleFlash.Play();
					muzzleFlashPlaying = true;
				}
			}
		}
		else
		{
			TransitionToState(State.Running);
			muzzleFlashPlaying = false;
			muzzleFlash.Stop();
		}
	}
	
	private void TransitionToState(State newState)
    {
        if (currentState == newState) return;

        currentState = newState;
		
		if (agent != null && agent.isOnNavMesh)
        {
            if (currentState == State.Running)
            {
                agent.isStopped = false; 
            }
            else
            {
                agent.isStopped = true; 
                agent.velocity = Vector3.zero;
            }
        }

        if (anim != null)
        {
            anim.SetBool("isStandShooting", currentState == State.StandingShooting);
            anim.SetBool("isSitShooting", currentState == State.CrouchShooting);
            anim.SetBool("isAttacking", currentState == State.Attacking);
        }
    }
	
	private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case State.Running:
                RunAction();
                break;
            case State.StandingShooting:
            case State.CrouchShooting:
                AimAtPlayer();
                break;
            case State.Attacking:
				Attack();
                break;
        }
    }
	
	private bool DetectPlayer(float DetectionRange)
	{	
        Vector3 origin = transform.position;

        Vector3 centerPoint = playerController.bounds.center;

        Vector3 direction = centerPoint - origin;
		float sqrDistance = direction.sqrMagnitude;

		if (sqrDistance < DetectionRange * DetectionRange)
		{
			if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, DetectionRange))
			{
				if (hit.collider.gameObject == player)
				{
					playerHealthScript.takeDamage();
					
					return true;
				}
			}
		}
		return false;
	}

    private void RunAction()
    {
        if(player != null)
        {	
            if(player != null && agent != null && agent.isOnNavMesh)
			{	
				agent.SetDestination(player.transform.position);
			}
        }
    }

    private void AimAtPlayer()
    {		
		Vector3 direction = player.transform.position - transform.position;
		direction.y = 0f;
		Quaternion targetRotation = Quaternion.LookRotation(direction);
		transform.rotation = targetRotation * Quaternion.Euler(0, 50, 0);
    }
	
	private void Attack()
	{
		Vector3 direction = player.transform.position - transform.position;
		direction.y = 0f;
		Quaternion targetRotation = Quaternion.LookRotation(direction);
		transform.rotation = targetRotation * Quaternion.Euler(0, 30, 0);
		
		playerHealthScript.takeDamage();
	}
	
	public void SetupSpawner(ModelSpawner srcSpawner, Transform srcPoint)
	{
		spawner = srcSpawner;
		mySpawnPoint = srcPoint;
	}
	
	public void TriggerRespawn()
	{
		if (spawner != null && mySpawnPoint != null)
		{
			spawner.TrackRespawn(mySpawnPoint);
		}
	}
	
	public void takeDamage()
    {
        health -= 2;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            if (anim != null)
            {
                if (currentState == State.StandingShooting) anim.SetTrigger("ShotStanding");
                if (currentState == State.CrouchShooting) anim.SetTrigger("ShotSitting");
            }
        }
    }
	
	private void Die()
	{
		if (isDead) return;
		isDead = true;
		
		if (audioSource != null)
		{
			audioSource.Stop();
			
			if (deathClip != null)
			{
				audioSource.PlayOneShot(deathClip);
			}
		}
		
		currentState = State.Dead;
		
		if(anim != null)
		{
			anim.SetTrigger("isDead");
		}
		
		if (agent != null)
        {
            agent.enabled = false;
        }
		
		muzzleFlashPlaying = false;
		muzzleFlash.Stop();
		
		TriggerRespawn();
		
		Destroy(gameObject, 5f);
	}
	
	private void ControlAudio()
	{
		AudioClip targetClip = null;
		bool shouldLoop = false;

		if (currentState == State.StandingShooting || currentState == State.CrouchShooting)
		{
			targetClip = shootClip;
			shouldLoop = true;
		}
		else if (currentState == State.Attacking)
		{
			targetClip = attackClip;
			shouldLoop = true; 
		}

		if (targetClip != null)
		{
			if (audioSource.clip != targetClip || !audioSource.isPlaying)
			{
				audioSource.clip = targetClip;
				audioSource.loop = shouldLoop;
				audioSource.Play();
			}
		}
		else
		{
			if (audioSource.isPlaying)
			{
				audioSource.Stop();
				audioSource.clip = null; 
			}
		}
	}
}
