using System;
using System.Collections;
using System.Collections.Generic;
using MoralisUnity;
using MoralisUnity.Kits.AuthenticationKit;
using MoralisUnity.Platform.Objects;
using UnityEngine;
using VoxToVFXFramework.Scripts.Managers;
using VoxToVFXFramework.Scripts.Models;
using WalletConnectSharp.Core.Models;

namespace VoxToVFXFramework.Scripts.UI.Login
{
	public class LoginPanel : MonoBehaviour
	{
		#region ScriptParameters

		[SerializeField] private AuthenticationKit AuthenticationKit;
		#endregion

		#region Fields

		public static event Action OnWalletConnected; 

		#endregion

		#region UnityMethods

		private void Start()
		{
			AuthenticationKit.OnStateChanged.AddListener(OnStateChanged);
			AuthenticationKit.OnConnected.AddListener(OnConnected);
			OnStateChanged(AuthenticationKit.State);
		}


		private void OnEnable()
		{
			StartCoroutine(ConnectCo());
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
			OnWalletConnected?.Invoke();
		}

		private void OnStateChanged(AuthenticationKitState authenticationKitState)
		{
			switch (authenticationKitState)
			{
				case AuthenticationKitState.None:
					break;
				case AuthenticationKitState.PreInitialized:
					break;
				case AuthenticationKitState.Initializing:
					break;
				case AuthenticationKitState.Initialized:
					break;
				case AuthenticationKitState.WalletConnecting:
					break;
				case AuthenticationKitState.WalletConnected:
					break;
				case AuthenticationKitState.WalletSigning:
					break;
				case AuthenticationKitState.WalletSigned:
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
