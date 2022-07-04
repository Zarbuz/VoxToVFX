using UnityEngine;

namespace VoxToVFXFramework.Scripts.Localization
{
	public abstract class LocalizedField : MonoBehaviour
	{
		#region Fields

		protected LocalizationManager mLocalizationManager => LocalizationManager.Instance;

		#endregion

		#region UnityMethods

		protected virtual void Start()
		{
			mLocalizationManager.AssignToManager(this);
		}

		#endregion

		#region Methods

		public abstract void UpdateLocalisation();

		#endregion
	}
}