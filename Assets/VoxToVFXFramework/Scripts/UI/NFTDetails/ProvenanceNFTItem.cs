using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;

public class ProvenanceNFTItem : MonoBehaviour
{
	#region ScriptParameters

	[SerializeField] private TextMeshProUGUI ActionText;
	[SerializeField] private TextMeshProUGUI DateText;
	[SerializeField] private TextMeshProUGUI PriceText;
	[SerializeField] private AvatarImage AvatarImage;
	[SerializeField] private Button OpenTransactionButton;

	#endregion

	#region PublicMethods

	public void Initialize(AbstractContractEvent contractEvent)
	{
		if (contractEvent is CollectionMintedEvent collectionMintedEvent)
		{
		}
	}

	#endregion
}
