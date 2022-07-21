using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.UI;

namespace VoxToVFXFramework.Scripts.Camera
{
	/// <summary>
	/// Utility Free Camera component.
	/// </summary>
	[CoreRPHelpURL("Free-Camera")]
	public class FreeCamera : MonoBehaviour
	{
		#region ConstStatic

		private const float MOUSE_SENSITIVITY_MULTIPLIER = 0.01f;

		#endregion

		#region ScriptParameterss

		/// <summary>
		/// Rotation speed when using a controller.
		/// </summary>
		public float LookSpeedController = 120f;
		/// <summary>
		/// Rotation speed when using the mouse.
		/// </summary>
		public float LookSpeedMouse = 4.0f;
		/// <summary>
		/// Movement speed.
		/// </summary>
		public float MoveSpeed = 10.0f;
		/// <summary>
		/// Value added to the speed when incrementing.
		/// </summary>
		public float MoveSpeedIncrement = 2.5f;
		/// <summary>
		/// Scale factor of the turbo mode.
		/// </summary>
		public float Turbo = 10.0f;

		#endregion

		#region Fields

		private InputAction mLookAction;
		private InputAction mOveAction;
		private InputAction mSpeedAction;
		private InputAction mYMoveAction;

		private float mInputRotateAxisX, mInputRotateAxisY;
		private float mInputChangeSpeed;
		private float mInputVertical, mInputHorizontal, mInputYAxis;
		private bool mLeftShiftBoost, mLeftShift, mFire1;

		#endregion

		#region UnityMethods

		private void OnEnable()
		{
			RegisterInputs();
		}

		private void Update()
		{
			// If the debug menu is running, we don't want to conflict with its inputs.
			if (CanvasPlayerPCManager.Instance.CanvasPlayerPcState != CanvasPlayerPCState.Closed)
			{
				return;
			}

			if (!RuntimeVoxManager.Instance.IsReady)
			{
				return;
			}

			UpdateInputs();

			if (mInputChangeSpeed != 0.0f)
			{
				MoveSpeed += mInputChangeSpeed * MoveSpeedIncrement;
				if (MoveSpeed < MoveSpeedIncrement) MoveSpeed = MoveSpeedIncrement;
			}

			bool moved = mInputRotateAxisX != 0.0f || mInputRotateAxisY != 0.0f || mInputVertical != 0.0f || mInputHorizontal != 0.0f || mInputYAxis != 0.0f;
			if (moved)
			{
				float rotationX = transform.localEulerAngles.x;
				float newRotationY = transform.localEulerAngles.y + mInputRotateAxisX;

				// Weird clamping code due to weird Euler angle mapping...
				float newRotationX = (rotationX - mInputRotateAxisY);
				if (rotationX <= 90.0f && newRotationX >= 0.0f)
					newRotationX = Mathf.Clamp(newRotationX, 0.0f, 90.0f);
				if (rotationX >= 270.0f)
					newRotationX = Mathf.Clamp(newRotationX, 270.0f, 360.0f);

				transform.localRotation = Quaternion.Euler(newRotationX, newRotationY, transform.localEulerAngles.z);

				float moveSpeed = Time.deltaTime * MoveSpeed;
				if (mFire1 || mLeftShiftBoost && mLeftShift)
					moveSpeed *= Turbo;
				transform.position += transform.forward * moveSpeed * mInputVertical;
				transform.position += transform.right * moveSpeed * mInputHorizontal;
				transform.position += Vector3.up * moveSpeed * mInputYAxis;
			}
		}

		#endregion

		#region PrivateMethods

		private void RegisterInputs()
		{
			InputActionMap map = new InputActionMap("Free Camera");

			mLookAction = map.AddAction("look", binding: "<Mouse>/delta");
			mOveAction = map.AddAction("move", binding: "<Gamepad>/leftStick");
			mSpeedAction = map.AddAction("speed", binding: "<Gamepad>/dpad");
			mYMoveAction = map.AddAction("yMove");

			mLookAction.AddBinding("<Gamepad>/rightStick").WithProcessor("scaleVector2(x=15, y=15)");
			mOveAction.AddCompositeBinding("Dpad")
				.With("Up", "<Keyboard>/w")
				.With("Up", "<Keyboard>/upArrow")
				.With("Down", "<Keyboard>/s")
				.With("Down", "<Keyboard>/downArrow")
				.With("Left", "<Keyboard>/a")
				.With("Left", "<Keyboard>/leftArrow")
				.With("Right", "<Keyboard>/d")
				.With("Right", "<Keyboard>/rightArrow");
			mSpeedAction.AddCompositeBinding("Dpad")
				.With("Up", "<Keyboard>/home")
				.With("Down", "<Keyboard>/end");
			mYMoveAction.AddCompositeBinding("Dpad")
				.With("Up", "<Keyboard>/pageUp")
				.With("Down", "<Keyboard>/pageDown")
				.With("Up", "<Keyboard>/e")
				.With("Down", "<Keyboard>/q")
				.With("Up", "<Gamepad>/rightshoulder")
				.With("Down", "<Gamepad>/leftshoulder");

			mOveAction.Enable();
			mLookAction.Enable();
			mSpeedAction.Enable();
			mYMoveAction.Enable();
		}

		private void UpdateInputs()
		{
			mInputRotateAxisX = 0.0f;
			mInputRotateAxisY = 0.0f;
			mLeftShiftBoost = false;
			mFire1 = false;

			Vector2 lookDelta = mLookAction.ReadValue<Vector2>();
			mInputRotateAxisX = lookDelta.x * LookSpeedMouse * MOUSE_SENSITIVITY_MULTIPLIER;
			mInputRotateAxisY = lookDelta.y * LookSpeedMouse * MOUSE_SENSITIVITY_MULTIPLIER;

			mLeftShift = Keyboard.current.leftShiftKey.isPressed;
			mFire1 = Mouse.current?.leftButton?.isPressed == true || Gamepad.current?.xButton?.isPressed == true;

			mInputChangeSpeed = mSpeedAction.ReadValue<Vector2>().y;

			Vector2 moveDelta = mOveAction.ReadValue<Vector2>();
			mInputVertical = moveDelta.y;
			mInputHorizontal = moveDelta.x;
			mInputYAxis = mYMoveAction.ReadValue<Vector2>().y;
		}

		#endregion

	}
}
