using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    
    [Header("Weapon settings")]
    [SerializeField] private float range = 100f;
    
    public void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, range))
        {
            Debug.Log("Hit: " + hit.transform.name);
            
            Enemy enemy = hit.transform.GetComponent<Enemy>();
            
            if (enemy != null)
            {
                enemy.gotShotStanding();
				enemy.gotShotSitting();
				enemy.takeDamage();
            }
        }
    }
}
