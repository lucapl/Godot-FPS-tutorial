using Godot;
using System;
using System.Numerics;

public partial class Player : CharacterBody3D
{
	[Export]
	public float Gravity = -24.8f;
	[Export]
	public float MaxSpeed = 20.0f;
	[Export]
	public float MaxSprintSpeed = 30.0f;
	[Export]
	public float JumpSpeed = 18.0f;
	[Export]
	public float Accel = 4.5f;
	[Export]
	public float SprintAccel = 18.0f;
	private bool _IsSprinting = false;
	[Export]
	public float Deaccel = 16.0f;
	[Export]
	public float MaxSlopeAngle = 40.0f;
	[Export]
	public float MouseSensivity = 0.05f;
	[Export]
	public float JoystickSensivity = 3f;
	
	private Godot.Vector3 _vel = new Godot.Vector3();
	private Godot.Vector3 _dir = new Godot.Vector3();

	private Godot.Vector2 _joyDir = new Godot.Vector2();

	private Camera3D _camera;
	private Node3D _rotationHelper;

	private SpotLight3D _flashlight;

	public override void _Ready()
	{
		_camera = GetNode<Camera3D>("Rotation_Helper/Camera");
		_rotationHelper = GetNode<Node3D>("Rotation_Helper");
		_flashlight = GetNode<SpotLight3D>("Rotation_Helper/Flashlight");

		Input.SetMouseMode(Input.MouseModeEnum.Captured);
	}


	public override void _PhysicsProcess(double delta)
	{
		ProcessInput((float)delta);
		ProcessMovement((float)delta);
	}

	private void ProcessInput(float delta)
	{
		_dir = new Godot.Vector3();
		Transform3D camXform = _camera.GetCameraTransform();

		Godot.Vector2 inputMovementVector = Input.GetVector("movement_left","movement_right","movement_backward","movement_forward");
		
		//new Godot.Vector2();
		// if (Input.IsActionPressed("movement_forward"))
		// {
		// 	inputMovementVector.Y += 1;
		// }
		// if (Input.IsActionPressed("movement_backward"))
		// {
		// 	inputMovementVector.Y -= 1;
		// }
		// if (Input.IsActionPressed("movement_right"))
		// {
		// 	inputMovementVector.X += 1;
		// }
		// if (Input.IsActionPressed("movement_left"))
		// {
		// 	inputMovementVector.X -= 1;
		// }
		_IsSprinting = Input.IsActionPressed("movement_sprint");
		
		//inputMovementVector = inputMovementVector.Normalized();

		_dir += -camXform.Basis.Z * inputMovementVector.Y;
		_dir += camXform.Basis.X * inputMovementVector.X;

		if (IsOnFloor())
		{
			if (Input.IsActionPressed("movement_jump"))
			{
				_vel.Y = JumpSpeed;
			}
		}

		if (Input.IsActionPressed("ui_cancel"))
		{
			if (Input.GetMouseMode() == Input.MouseModeEnum.Visible)
				Input.SetMouseMode(Input.MouseModeEnum.Captured);
			else
				Input.SetMouseMode(Input.MouseModeEnum.Visible);
		}

		if (Input.IsActionJustPressed("flashlight"))
		{
			if(_flashlight.IsVisibleInTree())
			{
				_flashlight.Hide();
			}
			else
			{
				_flashlight.Show();
			}
		}

		_joyDir = Input.GetVector(
			"joystick_camera_left",
			"joystick_camera_right",
			"joystick_camera_up",
			"joystick_camera_down");
	}

	private void ProcessMovement(float delta)
	{
		if (_joyDir.Length() != 0)
		{
			_rotationHelper.RotateX(Mathf.DegToRad(_joyDir.Y*JoystickSensivity));
			RotateY(Mathf.DegToRad(-_joyDir.X * JoystickSensivity));

			ClampHelper();
		}


		_dir.Y = 0;
		_dir = _dir.Normalized();

		_vel.Y += delta * Gravity;

		Godot.Vector3 hvel = _vel;
		hvel.Y = 0;

		Godot.Vector3 target = _dir;

		target *= _IsSprinting?MaxSprintSpeed:MaxSpeed;

		float accel;
		if (_dir.Dot(hvel) > 0)
			accel = _IsSprinting?SprintAccel:Accel;
		else
			accel = Deaccel;

		hvel = hvel.Lerp(target, accel * delta);
		_vel.X = hvel.X;
		_vel.Z = hvel.Z;
		Velocity = _vel;
		//as the following are constants they could be set once in _Ready
		FloorMaxAngle = Mathf.DegToRad(MaxSlopeAngle);
		FloorStopOnSlope = false;
		UpDirection = new Godot.Vector3(0,1,0);
		MoveAndSlide();
		//_vel = MoveAndSlide(_vel, new Vector3(0,1,0),false,Mathf.DegToRad(MaxSlopeAngle));
	}

	private void ClampHelper()
	{
		Godot.Vector3 cameraRot = _rotationHelper.RotationDegrees;
		cameraRot.X = Mathf.Clamp(cameraRot.X, -70,70);
		_rotationHelper.RotationDegrees = cameraRot;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion && Input.GetMouseMode() == Input.MouseModeEnum.Captured)
		{
			InputEventMouseMotion mouseEvent = @event as InputEventMouseMotion;

			_rotationHelper.RotateX(Mathf.DegToRad(mouseEvent.Relative.Y*MouseSensivity));
			RotateY(Mathf.DegToRad(-mouseEvent.Relative.X * MouseSensivity));

			ClampHelper();
		}
	}

}
