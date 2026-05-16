using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Camera cam;
	private float xRotation = 0f;
	[SerializeField] private float xSensitivity = 30f;
	[SerializeField] private float ySensitivity = 30f;
	
	public void lookAround(Vector2 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        // Calculate camera rotation for looking up and down
        xRotation -= (mouseY * Time.deltaTime) * ySensitivity;
		// This prevents from 360 degrees vertical rotation
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        // Apply this to our camera transform
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        // Rotate player to look left and right
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * xSensitivity);
    }
}
