using VoxToVFXFramework.Scripts.UI.Popups;
using VoxToVFXFramework.Scripts.UI.Popups.Descriptor;

public interface IPopup
{
	MessagePopupUnicityTag UnicityTag { get; }
	void Show();
	void Hide(bool removeFromList = true);

	void UpdateText(string str);
}

public interface InitalizablePopup<T> : IPopup where T : IMessagePopupDescriptor
{
	void Init(T descriptor);
}