using TMPro;
using UnityEngine;

namespace VoxToVFXFramework.Scripts.UI.Version
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class VersionText : MonoBehaviour
	{
		private void Start()
		{
			TextMeshProUGUI textMeshProUGUI = GetComponent<TextMeshProUGUI>();
			textMeshProUGUI.text = "WIP - v" + Application.version;
		}

	}
}
