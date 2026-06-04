using UnityEngine;

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
	private CharacterController controller;
	private State currentState = State.Running;
	private float stanceTimer = 0f;
	AudioManager audioManager;
	private float verticalVelocity = 0f;
	private float gravity = -9.81f;
	private bool muzzleFlashPlaying = false;

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

	void Awake()
	{
		audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
	}

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
		controller = GetComponent<CharacterController>();
    }

    void Update()
    {	
		if(currentState == State.Dead) return;
		
		if (stanceTimer > 0)
		{
			stanceTimer -= Time.deltaTime;
		}
		
		bool atAttackingRange = DetectPlayer(DetectionRangeAttacking);
        bool atShootingRange = DetectPlayer(DetectionRangeShooting);
		
		DetermineState(atAttackingRange, atShootingRange);

        ExecuteCurrentState();
		
		if (controller.isGrounded)
		{
			verticalVelocity = -2f;
		}
		else
		{
			verticalVelocity += gravity * Time.deltaTime;
		}

		Vector3 gravityMove = new Vector3(0, verticalVelocity, 0);
		controller.Move(gravityMove * Time.deltaTime);
    }
	
	private void DetermineState(bool atAttackingRange, bool atShootingRange)
	{
		//------------- Audio Part -----------------//
		if(currentState == State.Attacking || currentState == State.Running)
		{
			audioManager.StopEnemyShooting();
		}
		
		if(currentState != State.Attacking)
		{
			audioManager.StopAttackingSound();
		}
		//------------- End -----------------//
		
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

				audioManager.StartEnemyShooting();
				
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
            Vector3 lookDirection = Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized;
			
			if (lookDirection.sqrMagnitude != 0f)
			{
				transform.forward = lookDirection;
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
		
		audioManager.StartAttackingSound();
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
		currentState = State.Dead;
		
		if(anim != null)
		{
			anim.SetTrigger("isDead");
		}
		
		if (controller != null)
		{
			controller.enabled = false;
		}
		
		audioManager.playDeathAudio();
		audioManager.StopEnemyShooting();
		audioManager.StopAttackingSound();
		
		muzzleFlashPlaying = false;
		muzzleFlash.Stop();
		
		Destroy(gameObject, 5f);
	}
}
