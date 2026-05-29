using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
	private float timer = 1;
	
	[Header ("Player settings")]
	[SerializeField] private float playerHealth = 20f;
	[SerializeField] private float damageInterval = 1f;
	
    public void takeDamage()
	{	
		if(timer > 0)
		{
			timer -= Time.deltaTime;
		}
		else
		{
			timer = damageInterval;
			playerHealth -= 2;
		}
	}
}
