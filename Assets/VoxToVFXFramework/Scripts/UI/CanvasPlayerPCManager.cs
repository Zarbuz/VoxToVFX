using System;
using System.Collections;
using System.Collections.Generic;
using MoralisUnity.Web3Api.Models;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using VoxToVFXFramework.Scripts.Models.ContractEvent;
using VoxToVFXFramework.Scripts.Singleton;
using VoxToVFXFramework.Scripts.UI.Collection;
using VoxToVFXFramework.Scripts.UI.Creation;
using VoxToVFXFramework.Scripts.UI.EditProfile;
using VoxToVFXFramework.Scripts.UI.ImportScene;
using VoxToVFXFramework.Scripts.UI.Loading;
using VoxToVFXFramework.Scripts.UI.Login;
using VoxToVFXFramework.Scripts.UI.NFTDetails;
using VoxToVFXFramework.Scripts.UI.Photo;
using VoxToVFXFramework.Scripts.UI.Preview;
using VoxToVFXFramework.Scripts.UI.Profile;
using VoxToVFXFramework.Scripts.UI.Settings;
using VoxToVFXFramework.Scripts.UI.Topbar;
using VoxToVFXFramework.Scripts.UI.Weather;
using VoxToVFXFramework.Scripts.Utils.Extensions;
using Cursor = UnityEngine.Cursor;

namespace VoxToVFXFramework.Scripts.UI
{
	public enum CanvasPlayerPCState
	{
		Closed,
		Empty,
		Pause,
		ImportScene,
		Settings,
		Weather,
		Photo,
		Login,
		EditProfile,
		Collection,
		Profile,
		Creation,
		Loading,
		NftDetails
	}

	public class CanvasPlayerPCManager : ModuleSingleton<CanvasPlayerPCManager>
	{
		#region ScriptParameters

		[SerializeField] private Material BlurMat;
		[SerializeField] private Image BackgroundBlurImage;

		[SerializeField] private TopbarPanel TopbarPanel;
		[SerializeField] private LoginPanel LoginPanel;
		[SerializeField] private PausePanel PausePanel;
		[SerializeField] private ImportScenePanel ImportScenePanel;
		[SerializeField] private SettingsPanel SettingsPanel;
		[SerializeField] private WeatherPanel WeatherPanel;
		[SerializeField] private PhotoPanel PhotoPanel;
		[SerializeField] private EditProfilePanel EditProfilePanel;
		[SerializeField] private CollectionPanel CollectionPanel;
		[SerializeField] private ProfilePanel ProfilePanel;
		[SerializeField] private CreationPanel CreationPanel;
		[SerializeField] private LoadingPanel LoadingPanel;
		[SerializeField] private PreviewPanel PreviewPanel;
		[SerializeField] private NFTDetailsPanel NFTDetailsPanel;
		#endregion

		#region Fields

		private CanvasPlayerPCState mCanvasPlayerPcState;

		public CanvasPlayerPCState CanvasPlayerPcState
		{
			get => mCanvasPlayerPcState;
			set
			{
				mCanvasPlayerPcState = value;
				PausePanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.Pause);
				ImportScenePanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.ImportScene);
				SettingsPanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.Settings);
				WeatherPanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.Weather);
				PhotoPanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.Photo);
				LoginPanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.Login);
				EditProfilePanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.EditProfile);
				CollectionPanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.Collection);
				ProfilePanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.Profile);
				CreationPanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.Creation);
				LoadingPanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.Loading);
				NFTDetailsPanel.gameObject.SetActiveSafe(mCanvasPlayerPcState == CanvasPlayerPCState.NftDetails);

				CheckBlurImage();
				RefreshCursorState();
			}
		}



		public bool PauseLockedState { get; set; }

		private RenderTexture mRenderTexture;
		private UnityEngine.Camera mNewCamera;
		private static readonly int mAltTexture = Shader.PropertyToID("_AltTexture");

		#endregion

		#region UnityMethods

		protected override void OnStart()
		{
			CreateCameraRenderTexture();
			CanvasPlayerPcState = CanvasPlayerPCState.Closed;
		}

		private void Update()
		{
			if (Keyboard.current.escapeKey.wasPressedThisFrame && !PauseLockedState)
			{
				GenericTogglePanel(PreviewPanel.gameObject.activeSelf ? CanvasPlayerPCState.Empty : CanvasPlayerPCState.Pause);
			}
			else if (Keyboard.current.tabKey.wasPressedThisFrame && (CanvasPlayerPcState == CanvasPlayerPCState.Photo || CanvasPlayerPcState == CanvasPlayerPCState.Closed))
			{
				if (RuntimeVoxManager.Instance.IsReady)
				{
					GenericTogglePanel(CanvasPlayerPCState.Photo);
				}
			}
			else if (Mouse.current.leftButton.wasPressedThisFrame)
			{
				RefreshCursorState();
			}
		}


		private void OnApplicationFocus(bool hasFocus)
		{
			RefreshCursorState();
		}

		#endregion

		#region PublicMethods

		public void SetCanvasPlayerState(CanvasPlayerPCState state)
		{
			CanvasPlayerPcState = state;
		}

		public void GenericTogglePanel(CanvasPlayerPCState state)
		{
			CanvasPlayerPcState = CanvasPlayerPcState == state ? CanvasPlayerPCState.Closed : state;
		}

		public void GenericClosePanel()
		{
			CanvasPlayerPcState = CanvasPlayerPCState.Closed;
			PauseLockedState = false;
		}

		public void OpenImportScenePanel(ImportScenePanel.EDataImportType dataImportType)
		{
			SetCanvasPlayerState(CanvasPlayerPCState.ImportScene);
			ImportScenePanel.Initialize(dataImportType);
		}

		public void OpenProfilePanel(CustomUser user)
		{
			SetCanvasPlayerState(CanvasPlayerPCState.Profile);
			ProfilePanel.Initialize(user);
		}

		public void OpenNftDetailsPanel(CollectionMintedEvent collectionMinted, Nft metadata)
		{
			SetCanvasPlayerState(CanvasPlayerPCState.NftDetails);
			NFTDetailsPanel.Initialize(collectionMinted, metadata);
		}

		public void OpenCreationPanel(CollectionCreatedEvent collectionCreated)
		{
			CreationPanel.Initialize(collectionCreated);
		}

		public void OpenLoadingPanel(string description, Action onLoadFinished)
		{
			LoadingPanel.Initialize(description, onLoadFinished);
		}

		public void OpenLoadingPanel(string description)
		{
			LoadingPanel.Initialize(description);
		}

		public void OpenPreviewPanel(string title, string description, Action onBackCallback)
		{
			PreviewPanel.Initialize(title, description, onBackCallback);
		}

		#endregion

		#region PrivateMethods

		private void CreateCameraRenderTexture()
		{
			UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
			Transform transform1 = mainCamera!.transform;
			mNewCamera = Instantiate(mainCamera, transform1.parent, true);

			mNewCamera.transform.localPosition = transform1.localPosition;
			mNewCamera.transform.localRotation = transform1.localRotation;
			mNewCamera.transform.localScale = transform1.localScale;

			Destroy(mNewCamera.GetComponent<AudioListener>());

			mRenderTexture = new RenderTexture(Screen.width, Screen.height, 24);
			mNewCamera.targetTexture = mRenderTexture;
			mNewCamera.name = "Camera_RenderTexture";
			BlurMat.SetTexture(mAltTexture, mRenderTexture);
		}

		private void CheckBlurImage()
		{
			bool active = mCanvasPlayerPcState != CanvasPlayerPCState.Closed &&
			              mCanvasPlayerPcState != CanvasPlayerPCState.Photo &&
			              mCanvasPlayerPcState != CanvasPlayerPCState.Weather;
			mNewCamera.gameObject.SetActive(active);
			BackgroundBlurImage.gameObject.SetActive(active);

			if (mNewCamera.gameObject.activeSelf)
			{
				//Just enable the camera one frame
				StartCoroutine(DisableBlurCameraCo());
			}
		}

		private IEnumerator DisableBlurCameraCo()
		{
			yield return new WaitForEndOfFrame();
			mNewCamera.gameObject.SetActive(false);
		}

		private void RefreshCursorState()
		{
			Cursor.visible = !RuntimeVoxManager.Instance.IsReady || mCanvasPlayerPcState != CanvasPlayerPCState.Closed;
			Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
		}

		#endregion
	}
}
