using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.Editor.OSAWizard;

namespace Com.TheFallenGames.OSA.Editor
{
    static class MenuItems
	{
		[MenuItem("Tools/OSA/Code reference")]
		public static void OpenOnlineCodeReference()
		{ Application.OpenURL("http://thefallengames.com/unityassetstore/optimizedscrollviewadapter/doc"); }

		[MenuItem("Tools/OSA/Manual and FAQ")]
		public static void OpenOnlineManual()
		{ Application.OpenURL("https://docs.google.com/document/d/1exc3hz9cER9fKx2m0rXxTG0-vMxEGdFrd1NYdDJuATk/edit?usp=sharing"); }

		[MenuItem("Tools/OSA/Quick start video (old, but still quite useful)")]
		public static void OpenTutorial()
		{ Application.OpenURL("https://youtu.be/rcgnF16JybY"); }

		[MenuItem("Tools/OSA/OSA wizard video")]
		public static void OpenWizardVideo()
		{ Application.OpenURL("https://youtu.be/BnA32GV13ws"); }

		[MenuItem("Tools/OSA/Changelog")]
		public static void OpenOnlineChangelog()
		{ Application.OpenURL("http://thefallengames.com/unityassetstore/optimizedscrollviewadapter/Changelog.txt"); }

		[MenuItem("Tools/OSA/Thank you!")]
		public static void OpenThankYou()
		{ Application.OpenURL("http://thefallengames.com/unityassetstore/optimizedscrollviewadapter/thankyou"); }

		[MenuItem("Tools/OSA/Ask us a question")]
		public static void AskQuestion()
		{ Application.OpenURL("https://forum.unity.com/threads/30-off-optimized-scrollview-adapter-listview-gridview.395224"); }

		[MenuItem("Tools/OSA/About")]
		public static void OpenAbout()
		{
			EditorUtility.DisplayDialog(
				"OSA " + OSAConst.OSA_VERSION_STRING,
				"May the result of our passion and hard work aid you along your journey in creating something marvellous!" +
				"\r\n\r\nOptimized ScrollView Adapter by The Fallen Games" +
				"\r\nlucian@thefallengames.com" +
				"\r\ngeorge@thefallengames.com",
				"Close"
			);
		}

		[MenuItem("CONTEXT/ScrollRect/Optimize with OSA")]
		static void OptimizeSelectedScrollRectWithOSA(MenuCommand command)
		{
			ScrollRect scrollRect = (ScrollRect)command.context;
			var validationResult = InitOSAWindow.Validate(true, scrollRect, false);
			// Manually checking for validation, as this provides richer info about the case when initialization is not possible
			if (!validationResult.isValid)
			{
				CWiz.ShowCouldNotExecuteCommandNotification(null);
				Debug.Log("OSA: Could not optimize '" + scrollRect.name + "': " + validationResult.reasonIfNotValid);
				return;
			}

			InitOSAWindow.Open(InitOSAWindow.Parameters.Create(validationResult, false, true, true, false));
		}

		[MenuItem("GameObject/UI/Optimized ScrollView (OSA)", false, 10)]
		static void CreateOSA(MenuCommand menuCommand)
		{
			if (!CheckForCreateOSAViaWizard())
				return;

			CreateOSAWindow.Open(new CreateOSAWindow.Parameters());
		}

		[MenuItem("GameObject/UI/Optimized TableView (OSA)", false, 11)]
		static void CreateTableViewOSA(MenuCommand menuCommand)
		{
			if (CWiz.GetTypeFromAllAssemblies(CWiz.TV.TABLE_ADAPTER_INTERFACE_FULL_NAME) == null)
			{
				CWiz.ShowCouldNotExecuteCommandNotification(null);
				Debug.Log("OSA: Import the TableView.unitypackage first");
				return;
			}
#if OSA_TV_TMPRO
			if (Resources.Load<GameObject>(CWiz.TV.Paths.INPUT__FLOATING_DROPDOWN) == null)
			{
				CWiz.ShowCouldNotExecuteCommandNotification(null);
				Debug.Log("OSA: Found 'OSA_TV_TMPRO' scripting define, but package 'TMPro/TableViewTMProSupport.unitypackage' wasn't imported. Please import the package");
				return;
			}
#endif

			if (!CheckForCreateOSAViaWizard())
				return;

			var resPath = CWiz.TV.Paths.SCROLL_VIEW;
			var tvPrefab = Resources.Load<GameObject>(resPath);
			var go = UnityEngine.Object.Instantiate(tvPrefab);
			go.name = go.name.Replace("(Clone)", "");
			Canvas.ForceUpdateCanvases();

			// Register the creation in the undo system
			Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
			Selection.activeObject = go;
			GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);

			// Adding this just so the validation will pass, but we don't need it
			var scrollRect = go.AddComponent<ScrollRect>();
			scrollRect.horizontal = false;
			scrollRect.vertical = true;
			scrollRect.viewport = go.transform.Find("Viewport") as RectTransform;
			scrollRect.content = scrollRect.viewport.Find("Content") as RectTransform;

			var validationResult = InitOSAWindow.Validate(true, scrollRect, true);
			// Manually checking for validation, as this provides richer info about the case when initialization is not possible
			if (!validationResult.isValid)
			{
				CWiz.ShowCouldNotExecuteCommandNotification(null);
				Debug.Log("OSA: Could not configure instantiated TableView '" + scrollRect.name + "': " + validationResult.reasonIfNotValid);
				return;
			}

			InitOSAWindow.Parameters p = InitOSAWindow.Parameters.Create(
				validationResult, 
				true, 
				false,
				false,
				true,
				false,
				CWiz.TV.IMPLEMENTATION_TEMPLATE_NAME,
				CWiz.GetTypeFromAllAssemblies(CWiz.TV.TABLE_ADAPTER_INTERFACE_FULL_NAME)
			);
			InitOSAWindow.Open(p);
		}

		static bool CheckForCreateOSAViaWizard()
		{
			string reasonIfNotValid;
			// Manually checking for validation, as this provides richer info about the case when creation is not possible
			if (!CreateOSAWindow.Validate(true, out reasonIfNotValid))
			{
				CWiz.ShowCouldNotExecuteCommandNotification(null);
				Debug.Log("OSA: Could not create ScrollView on the selected object: " + reasonIfNotValid);
				return false;
			}

			return true;
		}
	}
}
