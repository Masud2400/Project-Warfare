using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public enum PlayerMovementState
    {
        Idle,
        Walking,
        Sprinting,
        Crouching,
        Lying,
        Sliding
    }

    [Header("State")]
    public PlayerMovementState currentState = PlayerMovementState.Idle;

    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;
    
    private float targetHeight;
    private float tiltTimer;
    private float slideSpeed;
    private float currentSpeed;
    
    public bool runningState => currentState == PlayerMovementState.Sprinting;
    public bool walkingState => currentState == PlayerMovementState.Walking;
    
    [Header ("Player Movement settings")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumptHeight = 0.6f;
    [SerializeField] private float walkingSpeed = 5f;
    [SerializeField] private float sprintingSpeed = 10f;
    
    [Header ("Crouch settings")]
    [SerializeField] private float CapsuleCrouchingHeight = 0.9f;
    [SerializeField] private float CapsuleStandingHeigth = 1.8f;
    
    [Header ("Sprinting settings")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float tiltSpeed = 14f;
    [SerializeField] private float tiltIntensity = 1f;
    
    [Header ("Lie Down Settings")]
    [SerializeField] private float CapsuleLyingHeight = 0.5f;
    
    private Vector3 GetCapsuleBottomHemisphere()
    {
        return transform.position + Vector3.up * controller.radius;
    }
    
    private Vector3 GetCapsuleTopHemisphere(float height)
    {
        return transform.position + Vector3.up * (height - controller.radius);
    }
    
    private bool CheckHeight(float heightToCheck)
    {
        Collider[] standingProjection = Physics.OverlapCapsule(
            GetCapsuleBottomHemisphere(),
            GetCapsuleTopHemisphere(heightToCheck),
            controller.radius, -1
        );
        foreach(Collider c in standingProjection)
        {
            if(c != controller)
            {
                return false;
            }
        }
        return true;
    }
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        targetHeight = CapsuleStandingHeigth;
        currentSpeed = walkingSpeed;
    }
    
    void Update()
    {
		if(isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2.0f;
        }
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
		
        isGrounded = controller.isGrounded;

        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 16f);
        
        float targetRadius = Mathf.Min(0.5f, controller.height * 0.25f); 
        controller.radius = Mathf.Lerp(controller.radius, targetRadius, Time.deltaTime * 16f);
        
        cameraHolder.localPosition = new Vector3(0, controller.height / 2.5f, 0);
        controller.center = new Vector3(0, controller.height / 2f, 0);
        
        if (currentState == PlayerMovementState.Sliding)
        {
            Slide();
        }
    }
    
    public void ProcessMove(Vector2 input)
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;
        
        controller.Move(transform.TransformDirection(moveDirection) * currentSpeed * Time.deltaTime);

        if (currentState == PlayerMovementState.Idle || currentState == PlayerMovementState.Walking)
        {
            currentState = input.magnitude > 0 ? PlayerMovementState.Walking : PlayerMovementState.Idle;
        }
        
        if (currentState == PlayerMovementState.Sprinting && input.magnitude > 0)
        {
            tiltTimer += Time.deltaTime * tiltSpeed;
            float tiltAmount = Mathf.Sin(tiltTimer) * tiltIntensity;

            Vector3 currentRot = cameraHolder.localEulerAngles;
            cameraHolder.localRotation = Quaternion.Euler(currentRot.x, currentRot.y, tiltAmount);
        }
        else
        {
            tiltTimer = 0;
            Vector3 currentRot = cameraHolder.localEulerAngles;
            cameraHolder.localRotation = Quaternion.Lerp(
                cameraHolder.localRotation, 
                Quaternion.Euler(currentRot.x, currentRot.y, 0f), 
                Time.deltaTime * 10f
            );
        }
    }
    
    public void StopWalking()
    {
        if (currentState == PlayerMovementState.Walking)
        {
            currentState = PlayerMovementState.Idle;
        }
    }
    
    public void Jump()
    {
        if (currentState == PlayerMovementState.Crouching || currentState == PlayerMovementState.Lying) return;
        
        if(isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumptHeight * -3.0f * gravity);
        }
    }
    
    private bool SetCrouchingState(bool crouched)
    {
        if(crouched)
        {
            targetHeight = CapsuleCrouchingHeight;
            currentState = PlayerMovementState.Crouching;
        }
        else
        {
            bool successfullyStandUp = CheckHeight(CapsuleStandingHeigth);
            if(!successfullyStandUp) return false;
            
            targetHeight = CapsuleStandingHeigth;
            currentState = PlayerMovementState.Idle;
        }
        
        return true;
    }
    
    public void Crouch()
    {
        if (!isGrounded) return;
        
        bool isCurrentlyCrouching = currentState == PlayerMovementState.Crouching;
        SetCrouchingState(!isCurrentlyCrouching);
    }
    
    public void StartSprint()
    {
        if(isGrounded)
        {   
            if (currentState == PlayerMovementState.Sliding)
			{
				currentSpeed = walkingSpeed;
				return;
			}
            
            SetCrouchingState(false);
			
            currentState = PlayerMovementState.Sprinting;
            currentSpeed = sprintingSpeed;
        }
    }
    
    public void StopSprint()
    {
        if (currentState == PlayerMovementState.Sprinting)
        {
            currentState = PlayerMovementState.Idle;
        }
        currentSpeed = walkingSpeed;
    }
    
    public void LieDown()
    {
        if(!isGrounded) return;
        
        if (currentState == PlayerMovementState.Sprinting)
        {
            currentState = PlayerMovementState.Sliding;
            slideSpeed = 30f;
            targetHeight = CapsuleLyingHeight;
            return;
        }
        
        if (currentState != PlayerMovementState.Lying)
        {
            targetHeight = CapsuleLyingHeight;
            currentState = PlayerMovementState.Lying;
        }
        else
        {
            bool successfullyCrouchUp = CheckHeight(CapsuleStandingHeigth);
            if (successfullyCrouchUp)
            {
                targetHeight = CapsuleStandingHeigth;
                currentState = PlayerMovementState.Idle;
            }
        }
    }
    
    private void Slide()
    {
        controller.Move(transform.forward * slideSpeed * Time.deltaTime);

        slideSpeed = Mathf.Lerp(slideSpeed, 0f, Time.deltaTime * 2f);

        if (slideSpeed <= 0.1f)
        {
            slideSpeed = 0f;
            
            currentState = PlayerMovementState.Lying; 
        }
    }
}