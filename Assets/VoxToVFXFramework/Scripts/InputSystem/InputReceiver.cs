using UnityEngine;
using UnityEngine.InputSystem;

namespace VoxToVFXFramework.Scripts.InputSystem
{
	public class InputReceiver : MonoBehaviour
	{
		#region ScriptParameters

		[Header("Character Input Values")]
		public Vector2 Move;
		public Vector2 Look;
		public bool Jump;
		public bool Sprint;

		[Header("Movement Settings")]
		public bool AnalogMovement;

		[Header("Mouse Cursor Settings")]
		public bool CursorLocked = true;
		public bool CursorInputForLook = true;

		#endregion

		#region PublicMethods

		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(CursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
		public void MoveInput(Vector2 newMoveDirection)
		{
			Move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			Look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			Jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			Sprint = newSprintState;
		}

		#endregion

	}
	
}