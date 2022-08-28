using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.UI.Popups.Descriptor;
using VoxToVFXFramework.Scripts.UI.Popups.Popup;
using static UnityEngine.Application;

namespace VoxToVFXFramework.Scripts.UI.Popups
{
	public class MessagePopup : MonoBehaviour
	{
		#region ConstStatic

		public static MessagePopup Instance;

		#endregion

		#region ScriptParameters

		[SerializeField] private RectTransform BlockingConfirmPanel;
		[SerializeField] private RectTransform Content;

		[SerializeField] private GameObject MessagePrefab;
		[SerializeField] private GameObject BlockingConfirmPopup;
		[SerializeField] private GameObject EditCollectionPopup;
		[SerializeField] private GameObject ConfirmatioWalletPopup;
		[SerializeField] private GameObject ConfirmationBlockchainPopup;

		#endregion

		#region Fields
		public float MessageDuration = 5f;

		private List<IPopup> mDisplayedPopups;

		#endregion

		#region UnityMethods

		private void Awake()
		{
			Instance = this;
			mDisplayedPopups = new List<IPopup>();
		}

		#endregion

		#region PublicMethods

		public static void Show(string message, LogType logType = LogType.Log, MessagePopupUnicityTag unicityTag = MessagePopupUnicityTag.DUPLICATE_ALLOWED)
		{
			if (CanInsertInQueue(unicityTag))
			{
				var popupDescriptor = new MessagePopupMessageDescriptor()
				{
					UnicityTag = unicityTag,
					Message = message,
					LogType = logType,
					PopupDisplayDuration = Instance.MessageDuration,
					PlaySoundOnShow = false,
				};

				Instance.CreateAndShow<MessagePopupMessage, MessagePopupMessageDescriptor>(
					Instance.MessagePrefab,
					Instance.Content,
					popupDescriptor);
			}
		}

		public static void ShowEditCollectionPopup(string logoImageUrl, string coverImageUrl, string description, Action<Models.CollectionDetails> onConfirm, Action onCancel)
		{
			var popupDescriptor = new EditCollectionDescriptor()
			{
				UnicityTag = MessagePopupUnicityTag.EDIT_COLLECTION,
				LogoImageUrl = logoImageUrl,
				CoverImageUrl = coverImageUrl,
				Description = description,
				OnConfirmAction = onConfirm,
				OnCancelAction = onCancel
			};

			Instance.CreateAndShow<EditCollectionPopup, EditCollectionDescriptor>(
				Instance.EditCollectionPopup,
				Instance.BlockingConfirmPanel,
				popupDescriptor);
		}

		public static void ShowConfirmationWalletPopup(UniTask<string> execute, Action<string> onCallback)
		{
			var popupDescriptor = new ConfirmationWalletDescriptor()
			{
				UnicityTag = MessagePopupUnicityTag.CONFIRMATION_WALLET,
				ActionToExecute = execute,
				OnActionSuccessful = onCallback,
				
			};

			Instance.CreateAndShow<ConfirmationWalletPopup, ConfirmationWalletDescriptor>(
				Instance.ConfirmatioWalletPopup,
				Instance.BlockingConfirmPanel,
				popupDescriptor);
		}

		public static void ShowConfirmationBlockchainPopup(string title, string description, string transactionId, Action<AbstractContractEvent> onSuccessCallback)
		{
			var popupDescriptor = new ConfirmationBlockchainDescriptor()
			{
				UnicityTag = MessagePopupUnicityTag.CONFIRMATION_BLOCKCHAIN,
				OnActionSuccessful = onSuccessCallback,
				Title = title,
				Description = description,
				TransactionId = transactionId
			};

			Instance.CreateAndShow<ConfirmationBlockchainPopup, ConfirmationBlockchainDescriptor>(
				Instance.ConfirmationBlockchainPopup,
				Instance.BlockingConfirmPanel,
				popupDescriptor);
		}

		public static void ShowOrUpdateCurrent(string message, LogType logType, MessagePopupUnicityTag unicityTag)
		{
			var curPopup = Instance.mDisplayedPopups.FirstOrDefault(t => t.UnicityTag == unicityTag);
			if (curPopup == null)
			{
				Show(message, logType, unicityTag);
			}
			else
			{
				curPopup.UpdateText(message);
			}
		}
		public static void ShowInfinite(string message, MessagePopupUnicityTag unicityTag = MessagePopupUnicityTag.DUPLICATE_ALLOWED, LogType logType = LogType.Log)
		{
			if (CanInsertInQueue(unicityTag))
			{
				var popupDescriptor = new MessagePopupMessageDescriptor()
				{
					UnicityTag = unicityTag,
					Message = message,
					LogType = logType
				};

				Instance.CreateAndShow<MessagePopupMessage, MessagePopupMessageDescriptor>(
					Instance.MessagePrefab,
					Instance.Content,
					popupDescriptor);
			}
		}

		public static void ShowConfirm(string message, Action onConfirm, Action onCancel = null, MessagePopupUnicityTag unicityTag = MessagePopupUnicityTag.DUPLICATE_ALLOWED, float duration = -1)
		{
			if (CanInsertInQueue(unicityTag))
			{
				var popupDescriptor = new MessagePopupMessageDescriptor()
				{
					UnicityTag = unicityTag,
					Message = message,

					Ok = LocalizationKeys.YES.Translate(),
					Cancel = LocalizationKeys.NO.Translate(),
					OnConfirm = onConfirm,
					OnCancel = onCancel,
					PopupDisplayDuration = duration,
					SetOffsetMessage = true
				};

				Instance.CreateAndShow<MessagePopupMessage, MessagePopupMessageDescriptor>(
					Instance.MessagePrefab,
					Instance.Content,
					popupDescriptor);
			}
		}

		public static void ShowInfoWithBlocking(string message, Action onConfirm = null)
		{
			ShowConfirmWithBlocking(message, "Ok", null, onConfirm, null);
		}

		public static void ShowConfirmWithBlocking(string message, Action onConfirm, Action onCancel = null)
		{
			ShowConfirmWithBlocking(message, LocalizationKeys.YES.Translate(), LocalizationKeys.NO.Translate(), onConfirm, onCancel);
		}

		public static void ShowConfirmWithBlocking(string message, string okText, string cancelText, Action onConfirm, Action onCancel = null)
		{
			var popupDescriptor = new MessagePopupMessageDescriptor()
			{
				Message = message,
				Ok = okText,
				Cancel = cancelText,
				OnConfirm = onConfirm,
				OnCancel = onCancel,
			};

			Instance.CreateAndShow<MessagePopupMessage, MessagePopupMessageDescriptor>(
				Instance.BlockingConfirmPopup,
				Instance.BlockingConfirmPanel,
				popupDescriptor);
		}

		public static void ShowConfirmDuration(string message, float duration, Action onConfirm, Action onDurationOver, Action onCancel = null, MessagePopupUnicityTag unicityTag = MessagePopupUnicityTag.DUPLICATE_ALLOWED)
		{
			if (CanInsertInQueue(unicityTag))
			{
				var popupDescriptor = new MessagePopupMessageDescriptor()
				{
					UnicityTag = unicityTag,
					Message = message,

					Ok = LocalizationKeys.YES.Translate(),
					Cancel = LocalizationKeys.NO.Translate(),
					OnConfirm = onConfirm,
					OnCancel = onCancel,
					OnDurationOver = onDurationOver,
					PopupDisplayDuration = duration
				};

				Instance.CreateAndShow<MessagePopupMessage, MessagePopupMessageDescriptor>(
					Instance.MessagePrefab,
					Instance.Content,
					popupDescriptor);
			}
		}

		public static void CleanDisplayPopup<TPopup, TDescriptor>(MessagePopupUnicityTag tag)
			where TDescriptor : IMessagePopupDescriptor
			where TPopup : MonoBehaviour, InitalizablePopup<TDescriptor>
		{
			foreach (TPopup popup in Instance.mDisplayedPopups.OfType<TPopup>().Where(t => t.UnicityTag == tag))
			{
				Destroy(popup.gameObject);
			}
			Instance.mDisplayedPopups.RemoveAll(t => t is TPopup && t.UnicityTag == tag);
		}

		public static void CleanDisplayPopup(MessagePopupUnicityTag tag)
		{
			foreach (var popup in Instance.mDisplayedPopups.Where(t => t.UnicityTag == tag))
			{
				if (popup is MonoBehaviour)
					Destroy((popup as MonoBehaviour).gameObject);
			}
			Instance.mDisplayedPopups.RemoveAll(t => t.UnicityTag == tag);
		}

		public void OnPopupDestroyed(IPopup destroyedPopup)
		{
			mDisplayedPopups.Remove(destroyedPopup);
			if (mDisplayedPopups.Count == 0)
			{
				DoClose();
			}
		}

		public bool IsAnyPopupOpen()
		{
			return mDisplayedPopups.Count > 0;
		}
		public int PopupCount()
		{
			return mDisplayedPopups.Count;
		}

		#endregion

		#region PrivateMethods

		private static bool CanInsertInQueue(MessagePopupUnicityTag unicityTag)
		{
			if (Instance == null || Instance.Content == null || Instance.mDisplayedPopups.Count >= 5)
			{
				return false;
			}

			if (unicityTag == MessagePopupUnicityTag.DUPLICATE_ALLOWED)
			{
				return true;
			}

			if (Instance.mDisplayedPopups.Any(p => p.UnicityTag == unicityTag))
			{
				return false;
			}

			return true;
		}

		private static void CleanDisplayPopup<TPopup, TDescriptor>()
			where TDescriptor : IMessagePopupDescriptor
			where TPopup : MonoBehaviour, InitalizablePopup<TDescriptor>
		{
			var popups = Instance.mDisplayedPopups.OfType<TPopup>();
			foreach (TPopup popup in popups)
			{
				//Destroy(popup.gameObject);
				popup.Hide(false);
			}
			Instance.mDisplayedPopups.RemoveAll(t => t is TPopup);
		}

		private void CreateAndShow<TPopup, TDescriptor>(GameObject prefab, Transform parent, TDescriptor descriptor)
			where TDescriptor : IMessagePopupDescriptor
			where TPopup : MonoBehaviour, InitalizablePopup<TDescriptor>
		{
			DoOpen();

			GameObject obj = Instantiate(prefab, parent);
			TPopup popup = obj.GetComponent<TPopup>();
			popup.transform.SetAsFirstSibling();

			mDisplayedPopups.Add(popup);

			popup.Init(descriptor);
			popup.Show();
		}


		private void DoOpen()
		{
			Content.gameObject.SetActive(true);
		}

		private void DoClose()
		{
			Content.gameObject.SetActive(false);
		}

		#endregion

		
	}
}
