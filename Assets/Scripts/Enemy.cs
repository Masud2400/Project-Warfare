using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Animator anim;

	[Header("Settings")]
    [SerializeField] private GameObject player;
    [SerializeField] private float speed = 1f;
	[SerializeField] private float DetectionRangeShooting = 5f;
	[SerializeField] private float DetectionRangeAttacking = 2f;
	[SerializeField] private float health = 10f;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {	
		bool atAttackingRange = DetectPlayer(DetectionRangeAttacking);
		bool atShootingRange = DetectPlayer(DetectionRangeShooting);
			
		if(atShootingRange && atAttackingRange == false)
		{	
			int randomChoice = Random.Range(0, 2);
			
			switch(randomChoice)
			{
				case 0:
					Shoot();
					break;
				case 1:
					sitShoot();
					break;
			}
		}
		else if(atAttackingRange)
		{
			attack();
		}
		else
		{
			Run();
		}
    }
	
	private bool DetectPlayer(float DetectionRange)
	{	
		Vector3 direction = player.transform.position - transform.position;
		float sqrDistance = direction.sqrMagnitude;

		Vector3 origin = transform.position + Vector3.up * 1.5f;

		if (sqrDistance < DetectionRange * DetectionRange)
		{
			if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, DetectionRange))
			{
				if (hit.collider.gameObject == player)
				{
					return true;
				}
			}
		}
		return false;
	}

    private void Run()
    {
        if(player != null)
        {	
			if(anim != null)
			{
				anim.SetBool("isStandShooting", false);
				anim.SetBool("isSitShooting", false);
				anim.SetBool("isAttacking", false);
			}
			
            Vector3 lookDirection = Vector3.ProjectOnPlane(player.transform.position - transform.position, Vector3.up).normalized;
			transform.position += lookDirection * speed * Time.deltaTime;
			if (lookDirection.sqrMagnitude != 0f)
			{
				transform.forward = lookDirection;
			}
        }
    }

    public void gotShotStanding()
    {
        if(anim != null)
        {
			if(anim.GetBool("isStandShooting"))
			{
				anim.SetTrigger("ShotStanding");
			}
        }
    }
	
	public void gotShotSitting()
    {
        if(anim != null)
        {
			if(anim.GetBool("isSitShooting"))
			{
				anim.SetTrigger("ShotSitting");
			}
        }
    }

    private void Shoot()
    {
        if(anim != null)
		{
			anim.SetBool("isStandShooting", true);
			anim.SetBool("isSitShooting", false);
		}
		
		Vector3 direction = player.transform.position - transform.position;
		direction.y = 0f;
		Quaternion targetRotation = Quaternion.LookRotation(direction);
		transform.rotation = targetRotation * Quaternion.Euler(0, 50, 0);
    }
	
	private void sitShoot()
	{
		if(anim != null)
		{
			anim.SetBool("isSitShooting", true);
			anim.SetBool("isStandShooting", false);
		}
		
		Vector3 direction = player.transform.position - transform.position;
		direction.y = 0f;
		Quaternion targetRotation = Quaternion.LookRotation(direction);
		transform.rotation = targetRotation * Quaternion.Euler(0, 50, 0);
	}
	
	private void attack()
	{
		if(anim != null)
		{
			anim.SetBool("isAttacking", true);
			anim.SetBool("isStandShooting", false);
			anim.SetBool("isSitShooting", false);
		}
	}
	
	public void takeDamage()
	{
		health -= 2;
		
		if(health <= 0)
		{
			Die();
		}
	}
	
	private void Die()
	{
		if(anim != null)
		{
			
		}
	}
}
