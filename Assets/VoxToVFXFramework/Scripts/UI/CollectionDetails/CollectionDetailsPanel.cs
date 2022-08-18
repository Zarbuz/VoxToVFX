using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Atomic;

namespace VoxToVFXFramework.Scripts.UI.CollectionDetails
{
	public class CollectionDetailsPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private Image MainImage;
		[SerializeField] private TextMeshProUGUI CollectionNameText;
		[SerializeField] private OpenUserProfileButton OpenUserProfileButton;
		[SerializeField] private Button EditCollectionButton;
		[SerializeField] private TextMeshProUGUI CollectionOfCountText;
		[SerializeField] private TextMeshProUGUI OwnedByCountText;
		[SerializeField] private TextMeshProUGUI FloorPriceText;
		[SerializeField] private TextMeshProUGUI TotalSalesText;
		[SerializeField] private Button NFTTabButton;
		[SerializeField] private Button ActivityTabButton;
		[SerializeField] private GameObject NoItemFoundPanel;
		[SerializeField] private Button MintNftButton;

		[SerializeField] private Button OpenSymbolButton;
		[SerializeField] private TextMeshProUGUI CollectionSymbolText; 

		[SerializeField] private GameObject NFTPanel;
		[SerializeField] private GameObject ActivityPanel;
		#endregion

		#region PublicMethods

		public void Initialize(CollectionCreatedEvent collection)
		{
	
		}

		#endregion
	}
}
