using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
	private PlayerInput playerInput;
	private PlayerInput.OnFootActions onFoot;
	private PlayerMotor motor;
	private PlayerLook look;
	private Weapon weapon;
	
    void Awake()
    {
		playerInput = new PlayerInput();
		onFoot = playerInput.OnFoot;
		motor = GetComponent<PlayerMotor>();
		// ctx is a specific structure used by Unity’s New Input System to provide 
		// information about the input event that just occurred.
		look = GetComponent<PlayerLook>();
		weapon = GetComponentInChildren<Weapon>();
		onFoot.Jump.performed += ctx => motor.Jump();
		onFoot.Crouch.performed += ctx => motor.Crouch();
		onFoot.Sprint.started += ctx => motor.StartSprint();
		onFoot.Sprint.canceled += ctx => motor.StopSprint();
		onFoot.Lie.performed += ctx => motor.LieDown();
    }

    void FixedUpdate()
    {
        motor.ProcessMove(onFoot.Movement.ReadValue<Vector2>());
		
		if (onFoot.Shoot.IsPressed())
		{
			weapon.Shoot();
		}
		else
		{
			weapon.StopShooting();
		}
    }
	
	void LateUpdate()
	{
		look.lookAround(onFoot.Look.ReadValue<Vector2>());
	}
	
	private void OnEnable()
	{
		onFoot.Enable();
	}
	
	private void OnDisable()
	{
		onFoot.Disable();
	}
}
