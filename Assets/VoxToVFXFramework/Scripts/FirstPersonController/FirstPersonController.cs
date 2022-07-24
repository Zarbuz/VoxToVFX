using UnityEngine;
using UnityEngine.InputSystem;
using VoxToVFXFramework.Scripts.InputSystem;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.UI;

namespace VoxToVFXFramework.Scripts.FirstPersonController
{
	[RequireComponent(typeof(CharacterController))]
	[RequireComponent(typeof(PlayerInput))]
	public class FirstPersonController : MonoBehaviour
	{
		#region ScriptParameters

		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		#endregion

		#region Fields

		// cinemachine
		private float mCinemachineTargetPitch;

		// player
		private float mSpeed;
		private float mRotationVelocity;
		private float mVerticalVelocity;
		private float mTerminalVelocity = 53.0f;

		// timeout deltatime
		private float mJumpTimeoutDelta;
		private float mFallTimeoutDelta;
	
		private PlayerInput mPlayerInput;
		private CharacterController mController;
		private InputReceiver mInputReceiver;
		private GameObject mMainCamera;

		private bool IsCurrentDeviceMouse
		{
			get
			{
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
				return mPlayerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
			}
		}

		#endregion

		#region ConstStatic

		private const float THRESHOLD = 0.01f;

		#endregion

		#region UnityMethods

		private void Awake()
		{
			// get a reference to our main camera
			if (mMainCamera == null)
			{
				mMainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			mController = GetComponent<CharacterController>();
			mInputReceiver = GetComponent<InputReceiver>();
			mPlayerInput = GetComponent<PlayerInput>();
			// reset our timeouts on start
			mJumpTimeoutDelta = JumpTimeout;
			mFallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
			if (!RuntimeVoxManager.Instance.IsReady ||CameraManager.Instance.CameraState != eCameraState.FIRST_PERSON)
			{
				return;
			}

			JumpAndGravity();
			GroundedCheck();
			Move();
		}

		private void LateUpdate()
		{
			if (!RuntimeVoxManager.Instance.IsReady || CameraManager.Instance.CameraState != eCameraState.FIRST_PERSON)
			{
				return;
			}

			CameraRotation();
		}

		#endregion

		#region PrivateMethods

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			if (CanvasPlayerPCManager.Instance.CanvasPlayerPcState != CanvasPlayerPCState.Closed)
			{
				return;
			}

			// if there is an input
			if (mInputReceiver.Look.sqrMagnitude >= THRESHOLD)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				mCinemachineTargetPitch += mInputReceiver.Look.y * RotationSpeed * deltaTimeMultiplier;
				mRotationVelocity = mInputReceiver.Look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				mCinemachineTargetPitch = ClampAngle(mCinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(mCinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * mRotationVelocity);
			}
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = mInputReceiver.Sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (mInputReceiver.Move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(mController.velocity.x, 0.0f, mController.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = mInputReceiver.AnalogMovement ? mInputReceiver.Move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				mSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				mSpeed = Mathf.Round(mSpeed * 1000f) / 1000f;
			}
			else
			{
				mSpeed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(mInputReceiver.Move.x, 0.0f, mInputReceiver.Move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (mInputReceiver.Move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * mInputReceiver.Move.x + transform.forward * mInputReceiver.Move.y;
			}

			// move the player
			mController.Move(inputDirection.normalized * (mSpeed * Time.deltaTime) + new Vector3(0.0f, mVerticalVelocity, 0.0f) * Time.deltaTime);
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				mFallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (mVerticalVelocity < 0.0f)
				{
					mVerticalVelocity = -2f;
				}

				// Jump
				if (mInputReceiver.Jump && mJumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					mVerticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (mJumpTimeoutDelta >= 0.0f)
				{
					mJumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				mJumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (mFallTimeoutDelta >= 0.0f)
				{
					mFallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				mInputReceiver.Jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (mVerticalVelocity < mTerminalVelocity)
			{
				mVerticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}

		#endregion
	}
}