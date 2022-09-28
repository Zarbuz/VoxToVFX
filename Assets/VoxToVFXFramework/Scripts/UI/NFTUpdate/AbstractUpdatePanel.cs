using UnityEngine;
using UnityEngine.UI;

namespace VoxToVFXFramework.Scripts.UI.NFTUpdate
{
	public abstract class AbstractUpdatePanel : MonoBehaviour
	{
		#region ScriptParameters

		[Header("AbstractUpdatePanel")]
		[SerializeField] private Toggle Toggle;
		[SerializeField] private GameObject Panel;
		[SerializeField] private Image ArrowIcon;

		#endregion

		#region UnityMethods

		protected virtual void OnEnable()
		{
			Toggle.onValueChanged.AddListener(OnToggleValueChanged);
			OnToggleValueChanged(false);
		}

		protected virtual void OnDisable()
		{
			Toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
		}

		#endregion

		#region PrivateMethods

		private void OnToggleValueChanged(bool active)
		{
			ArrowIcon.transform.eulerAngles = active ? new Vector3(0, 0, 90) : new Vector3(0, 0, 270);
			Panel.SetActive(active);
		}

		#endregion
	}
}
