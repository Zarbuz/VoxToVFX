using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.Utils.Navigation
{
	public class NavigationInputFields : MonoBehaviour
	{
		#region Fields

		private EventSystem mCurrentEventSystem;
		private List<TMP_InputField> mInputFields;
		#endregion

		#region UnityMethods

		private void Start()
		{
			mCurrentEventSystem = EventSystem.current;
			Refresh();
		}

		private void Update()
		{
			if (Keyboard.current.tabKey.wasPressedThisFrame)
			{
				if (mInputFields.Count == 0)
				{
					return;
				}

				if (mCurrentEventSystem.currentSelectedGameObject == null)
				{
					SelectInputField(mInputFields[0]);
					return;
				}

				Selectable currentSelectable = mCurrentEventSystem.currentSelectedGameObject.GetComponent<Selectable>();
				mInputFields = mInputFields.Where(input => input != null).ToList();
				int index = FindIndexOfSelectable(currentSelectable) + 1;
				if (index >= mInputFields.Count)
				{
					index = 0;
				}
				//Debug.Log("index: " + index + " " + mInputFields.Count);
				SelectInputField(mInputFields[index]);
			}
		}

		#endregion

		#region PublicMethods

		public void Refresh()
		{
			mInputFields = GetComponentsInChildren<TMP_InputField>(false).ToList();
		}

		#endregion

		#region PrivateMethods

		private void SelectInputField(TMP_InputField inputField)
		{
			inputField.OnPointerClick(new PointerEventData(mCurrentEventSystem));
			mCurrentEventSystem.SetSelectedGameObject(inputField.gameObject, new BaseEventData(mCurrentEventSystem));
		}

		private int FindIndexOfSelectable(Selectable selectable)
		{
			TMP_InputField inputField = selectable.GetComponent<TMP_InputField>();
			if (inputField == null)
			{
				return -1;
			}

			return mInputFields.FindIndex(s => s == inputField);
		}

		#endregion
	}
}
