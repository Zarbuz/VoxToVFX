using MoralisUnity;
using MoralisUnity.Kits.AuthenticationKit;
using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using VoxToVFXFramework.Scripts.Localization;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;

namespace VoxToVFXFramework.Scripts.UI.Login
{
	public class LoginPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private AuthenticationKit AuthenticationKit;
		[SerializeField] private GameObject WalletConnectPlatform;
		[SerializeField] private GameObject Spinner;
		[SerializeField] private TextMeshProUGUI StatusText;
		#endregion

		#region Fields

		public static event Action OnWalletConnected;
		private bool mIsConnected;

		#endregion

		#region UnityMethods

		private void Start()
		{
			AuthenticationKit.OnStateChanged.AddListener(OnStateChanged);
			AuthenticationKit.OnConnected.AddListener(OnConnected);
		}

		private void OnEnable()
		{
			OnStateChanged(AuthenticationKit.State);
			StartCoroutine(ConnectCo());
		}

		#endregion

		#region PublicMethods

		public async UniTask Initialize()
		{
			mIsConnected = false;
			await UniTask.WaitUntil(() => mIsConnected);
		}

		public void Disconnect()
		{
			AuthenticationKit.Disconnect();
		}

		#endregion

		#region PrivateMethods

		private IEnumerator ConnectCo()
		{
			yield return new WaitForEndOfFrame();
			AuthenticationKit.Connect();
		}

		private async void OnConnected()
		{
			Debug.Log("LoginPanel: OnConnected");
			CustomUser user = await UserManager.Instance.LoadCurrentUser();

			if (user == null || string.IsNullOrEmpty(user.UserName))
			{
				CanvasPlayerPCManager.Instance.GenericTogglePanel(CanvasPlayerPCState.EditProfile);
			}
			else
			{
				CanvasPlayerPCManager.Instance.GenericClosePanel();
			}

			mIsConnected = true;
			Debug.Log("CurrentChainId: " + Moralis.CurrentChain.ChainId);
			OnWalletConnected?.Invoke();
		}

		private void OnStateChanged(AuthenticationKitState authenticationKitState)
		{
			Debug.Log("LoginPanel: OnStateChanged -> " + authenticationKitState);
			switch (authenticationKitState)
			{
				case AuthenticationKitState.None:
					break;
				case AuthenticationKitState.PreInitialized:
					break;
				case AuthenticationKitState.Initializing:
					WalletConnectPlatform.gameObject.SetActive(false);
					StatusText.gameObject.SetActive(false);
					Spinner.gameObject.SetActive(false);
					break;
				case AuthenticationKitState.Initialized:
					break;
				case AuthenticationKitState.WalletConnecting:
					WalletConnectPlatform.gameObject.SetActive(true);
					StatusText.gameObject.SetActive(false);
					Spinner.gameObject.SetActive(false);

					break;
				case AuthenticationKitState.WalletConnected:
					StatusText.gameObject.SetActive(false);

					break;
				case AuthenticationKitState.WalletSigning:
					Spinner.gameObject.SetActive(true);
					WalletConnectPlatform.gameObject.SetActive(false);
					StatusText.gameObject.SetActive(true);
					StatusText.text = LocalizationKeys.WALLET_SIGNING.Translate();
					break;
				case AuthenticationKitState.WalletSigned:
					Spinner.gameObject.SetActive(false);
					break;
				case AuthenticationKitState.MoralisLoggingIn:
					break;
				case AuthenticationKitState.MoralisLoggedIn:
					break;
				case AuthenticationKitState.Disconnecting:
					break;
				case AuthenticationKitState.Disconnected:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(authenticationKitState), authenticationKitState, null);
			}
		}

		#endregion
	}
}
