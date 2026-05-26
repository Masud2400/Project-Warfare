using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Animator anim;
	private bool playerDetected;

    [SerializeField] private GameObject player;
    [SerializeField] private float speed = 1f;
	[SerializeField] private float DetectionRange = 5f;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
		DetectPlayer();
		
        if(playerDetected)
		{
			Shoot();
		}
		else
		{
			Run();
		}
    }
	
	private void DetectPlayer()
	{	
		playerDetected = false;

		Vector3 direction = player.transform.position - transform.position;
		float sqrDistance = direction.sqrMagnitude;

		Vector3 origin = transform.position + Vector3.up * 1.5f;

		Debug.DrawRay(origin, direction.normalized * DetectionRange, Color.red);

		if (sqrDistance < DetectionRange * DetectionRange)
		{
			if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, DetectionRange))
			{
				Debug.Log("Ray hit: " + hit.collider.name + " | Tag: " + hit.collider.tag);

				if (hit.collider.gameObject == player)
				{
					playerDetected = true;
				}
			}
		}
	}

    private void Run()
    {
        if(player != null)
        {
			if(anim != null)
			{
				anim.SetFloat("RunShoot", 0);
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
            anim.SetTrigger("Shot");
        }
    }

    private void Shoot()
    {
        if(anim != null)
		{
			anim.SetFloat("RunShoot", 1);
		}
    }
}
