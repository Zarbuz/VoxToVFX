using UnityEngine;
using UnityEngine.InputSystem;

namespace VoxToVFXFramework.Scripts.InputSystem
{
	public class RebindSaveLoad : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private InputActionAsset Actions;

		#endregion

		#region UnityMethods

		public void OnEnable()
		{
			string rebinds = PlayerPrefs.GetString("rebinds");
			if (!string.IsNullOrEmpty(rebinds))
			{
				Actions.LoadBindingOverridesFromJson(rebinds);
			}
		}

		public void OnDisable()
		{
			string rebinds = Actions.SaveBindingOverridesAsJson();
			PlayerPrefs.SetString("rebinds", rebinds);
		}

		#endregion
	}

}
