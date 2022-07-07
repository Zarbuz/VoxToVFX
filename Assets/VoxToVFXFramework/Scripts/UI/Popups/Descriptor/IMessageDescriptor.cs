namespace VoxToVFXFramework.Scripts.UI.Popups.Descriptor
{
	public interface IMessagePopupDescriptor
	{
		/// <summary>
		/// Indicates a "kind of" messagePopup that can exist only once in the MessagePopup Waiting queue.
		/// </summary>
		MessagePopupUnicityTag UnicityTag { get; }

		/// <summary>
		/// The time in seconds the popup is displayed. -1 means it is displayed until an user action.
		/// </summary>
		float PopupDisplayDuration { get; }
	}
}