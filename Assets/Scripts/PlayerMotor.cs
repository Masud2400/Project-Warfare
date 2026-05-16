using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
	private Vector3 playerVelocity;
	private bool isGrounded;
	
	private bool IsCrouching;
	private float targetHeight;
	
	private bool IsSprinting;
	private float tiltTimer;
	
	[Header ("Player Movement settings")]
	[SerializeField] private float speed = 5f;
	[SerializeField] private float gravity = -20f; // Gravity can be higher, like -20f, in games,
	[SerializeField] private float jumptHeight = 0.6f;
	
	[Header ("Crouch settings")]
	[SerializeField] private float CapsuleCrouchingHeight = 0.9f;
	[SerializeField] private float CapsuleStandingHeigth = 1.8f;
	
	[Header ("Sprinting settings")]
	[SerializeField] private Transform cameraHolder;
	[SerializeField] private float tiltSpeed = 14f;
	[SerializeField] private float tiltIntensity = 1f;
	
	private Vector3 GetCapsuleBottomHemisphere()
	{
		return transform.position + Vector3.up * controller.radius;
	}
	
	private Vector3 GetCapsuleTopHemisphere(float height)
	{
		return transform.position + Vector3.up * (height - controller.radius);
	}
	
	void Start()
	{
		controller = GetComponent<CharacterController>();
		targetHeight = CapsuleStandingHeigth;
	}
	
	void Update()
	{
		isGrounded = controller.isGrounded;
		// This makes the crouching more slower, like, not snappy
		controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 16f);
	}
	
	public void ProcessMove(Vector2 input)
	{
		Vector3 moveDirection = Vector3.zero;
		moveDirection.x = input.x;
		moveDirection.z = input.y;
		// Without time.DeltaTime it would move faster or slower in different computers
		// This is for horizontal movement
		
		controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);
		
		if (IsSprinting && input.magnitude > 0)
		{
			// Increase timer based on time. 14f is the "speed" of the tilt
			tiltTimer += Time.deltaTime * tiltSpeed;
			
			// Calculate the tilt value using a Sine wave
			// 2f is the "intensity" or degrees of the tilt
			float tiltAmount = Mathf.Sin(tiltTimer) * tiltIntensity;

			// Apply to the Z-axis while preserving existing Mouse Look (X and Y)
			Vector3 currentRot = cameraHolder.localEulerAngles;
			cameraHolder.localRotation = Quaternion.Euler(currentRot.x, currentRot.y, tiltAmount);
		}
		else
		{
			// Reset timer when not sprinting
			tiltTimer = 0;
			
			// Smoothly return Z-axis to 0
			Vector3 currentRot = cameraHolder.localEulerAngles;
			cameraHolder.localRotation = Quaternion.Lerp(
				cameraHolder.localRotation, 
				Quaternion.Euler(currentRot.x, currentRot.y, 0f), 
				Time.deltaTime * 10f
			);
		}
		
		if(isGrounded && playerVelocity.y < 0)
		{
			playerVelocity.y = -2.0f;
		}
		playerVelocity.y += gravity * Time.deltaTime;
		// This is for vertical movement
		controller.Move(playerVelocity * Time.deltaTime);
	}
	
	public void Jump()
	{
		if(isGrounded && !IsCrouching)
		{
			playerVelocity.y = Mathf.Sqrt(jumptHeight * -3.0f * gravity);
		}
	}
	
	private bool SetCrouchingState(bool crouched)
	{
		if(crouched)
		{
			// If crouched is true, the player should crouch
			targetHeight = CapsuleCrouchingHeight;
		}
		else
		{
			// If not, it should project an imaginary capsule to check if it can stand up
			Collider[] standingProjection = Physics.OverlapCapsule(
				GetCapsuleBottomHemisphere(),
				GetCapsuleTopHemisphere(CapsuleStandingHeigth),
				controller.radius, -1
			);
			foreach(Collider c in standingProjection)
			{
				// If collider is not characterController itself, it should return false
				if(c != controller)
				{
					return false;
				}
			}
			
			targetHeight = CapsuleStandingHeigth;
		}
		
		IsCrouching = crouched;
		return true;
	}
	
	public void Crouch()
	{
		// After crouching started, it should stop sprinting
		if(IsSprinting)
			IsSprinting = false;
		
		if(isGrounded)
		{
			if(!IsCrouching)
			{
				SetCrouchingState(true);
			}
			else
			{
				SetCrouchingState(false);
			}
		}
	}
	
	public void StartSprint()
	{
		if(isGrounded)
		{
			IsSprinting = true;
			// When sprinting is started, it should not be in a crouching state
			SetCrouchingState(false);
			speed = 10f;
		}
	}
	
	public void StopSprint()
	{
		IsSprinting = false;
		speed = 5f;
	}
}
