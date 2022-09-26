// This allows faster debugging when need to simualte other platforms by commenting the custom define directive
#if UNITY_EDITOR_WIN
#define OSA_UNITY_EDITOR_WIN
#endif


using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using frame8.Logic.Misc.Other;
using frame8.Logic.Misc.Visual.UI.MonoBehaviours;
using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.Editor.OSAWizard.CustomAdapterConfigurators;

namespace Com.TheFallenGames.OSA.Editor.OSAWizard
{
	public class InitOSAWindow : BaseOSAWindow<InitOSAWindow.Parameters>
	{
		[SerializeField]
		State _State = State.NONE;


		protected override string CompilingScriptsText
		{
			get
			{
				return base.CompilingScriptsText + ((_WindowParams != null && _WindowParams.indexOfExistingImplementationToUse == 0) ? 
							"\n(Unity could briefly switch to the code editor and back. This is normal)" : "");
			}
		}

		Dictionary<Type, Action<BaseParams, RectTransform>> _MapParamBaseTypeToPrefabSetter;
		//bool _VSSolutionReloaded;

#if OSA_PLAYMAKER
		Dictionary<string, string[]> _Playmaker_MapControllerToSupportedItemPrefabs = new Dictionary<string, string[]>
			{
				{
					"PMPlainArrayController",
					new string[]
					{
						"PMGridPlainArrayItem",
						"PMListPlainArrayItem",
					}
				},

				{
					"PMLazyDataHelperController",
					new string[]
					{
						"PMGridLazyDataHelperItem",
						"PMListLazyDataHelperItem",
						"PMListLazyDataHelperItem_ContentSizeFitter"
					}
				},

				{
					"PMLazyDataHelperXMLController",
					new string[]
					{
						"PMGridLazyDataHelperXMLItem",
						"PMListLazyDataHelperXMLItem"
					}
				}
			};
#endif


		#region Visual studio solution reload code for windows
		// This prevents another visual studio instance from being opened when the solution was externally modified
		// by automatically presing the 'Reload' button.
		// Some changes were made
		// Original source https://gamedev.stackexchange.com/questions/124320/force-reload-vs-soution-explorer-when-adding-new-c-script-via-unity3d
#if OSA_UNITY_EDITOR_WIN


		class NativeMethods
		{
			internal enum ShowWindowEnum
			{
				Hide = 0,
				ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
				Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
				Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
				Restore = 9, ShowDefault = 10, ForceMinimized = 11
			};


			// = Is minimized
			[System.Runtime.InteropServices.DllImport("user32.dll")]
			internal static extern bool IsIconic(IntPtr handle);

			//[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
			//private static extern IntPtr GetForegroundWindow();

			[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, EntryPoint = "FindWindow", SetLastError = true)]
			internal static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

			[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
			internal static extern IntPtr FindWindow(String ClassName, String WindowName);

			[System.Runtime.InteropServices.DllImport("user32.dll")]
			[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
			internal static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

			[System.Runtime.InteropServices.DllImport("user32.dll")]
			internal static extern int SetForegroundWindow(IntPtr hwnd);

			// TFG: method added
			[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
			internal static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

			[System.Runtime.InteropServices.DllImport("user32.dll")]
			internal static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

			[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
			internal static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

			[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
			internal static extern IntPtr SendMessage(IntPtr hwnd, uint Msg, IntPtr wParam, IntPtr lParam);

			// Delegate to filter which windows to include 
			internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

			[System.Runtime.InteropServices.DllImport("user32.dll")]
			internal static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, IntPtr dwExtraInfo);

			/// <summary> Find all windows that match the given filter </summary>
			/// <param name="filter"> A delegate that returns true for windows
			///    that should be returned and false for windows that should
			///    not be returned </param>
			internal static List<IntPtr> FindWindows(NativeMethods.EnumWindowsProc filter)
			{
				List<IntPtr> windows = new List<IntPtr>();

				NativeMethods.EnumWindows(delegate (IntPtr wnd, IntPtr param)
				{
					if (filter(wnd, param))
					{
						// only add the windows that pass the filter
						windows.Add(wnd);
					}

					// but return true here so that we iterate all windows
					return true;
				}, IntPtr.Zero);

				return windows;
			}

			internal static IntPtr FindWindow(EnumWindowsProc filter)
			{
				var list = FindWindows(filter);
				return list.Count > 0 ? list[0] : IntPtr.Zero;
			}
		}


		const string WINDOW_CAPTION = "File Modification Detected";


		static string GetWindowText(IntPtr hwnd)
		{
			int charCount = 512;
			var sb = new System.Text.StringBuilder(charCount);
			NativeMethods.GetWindowText(hwnd, sb, charCount);
			return sb.ToString();
		}

		static string GetProjectName()
		{
			string[] s = Application.dataPath.Split('/');
			return s[s.Length - 2];
		}
		static string[] GetTargetVSWindowNames(string projectName)
		{
			return new string[]
				{
				"UnityVS." + projectName + "-csharp - Microsoft Visual Studio",
				"UnityVS." + projectName + " - Microsoft Visual Studio",
				projectName + " - Microsoft Visual Studio",
				projectName + "-csharp - Microsoft Visual Studio",
				}
			;
		}

		static bool ContainsTargeVSWindowName(string title)
		{
			string projectName = GetProjectName();
			return Array.Exists(GetTargetVSWindowNames(projectName), pName => title.Contains(pName));
		}

		static IntPtr GetVisualStudioHWNDIfOpenedWithCurrentProject()
		{
			return NativeMethods.FindWindow((hwnd, __) => ContainsTargeVSWindowName(GetWindowText(hwnd)));
		}

		static bool IsVisualStudioOpenedWithCurrentProjectAndBusy()
		{
			if (GetVisualStudioHWNDIfOpenedWithCurrentProject() == IntPtr.Zero)
				return false;

			var vsProcesses = System.Diagnostics.Process.GetProcessesByName("devenv");

			// Exactly one visual studio instance is needed. Otherwise, we can't tell
			if (vsProcesses.Length != 1)
				return false;

			var process = vsProcesses[0];

			//return process.MainWindowHandle == IntPtr.Zero;
			return !process.Responding;
		}

		static bool ReloadVisualStudioSolutionIfOpened(out bool canOpenScript)
		{
			canOpenScript = false;

			string projectName = GetProjectName();
			IntPtr projectVisualStudioHWND = GetVisualStudioHWNDIfOpenedWithCurrentProject();
			if (projectVisualStudioHWND == IntPtr.Zero)
			{
				canOpenScript = true;
				return false;
			}

			if (NativeMethods.IsIconic(projectVisualStudioHWND))
			{
				var succ = NativeMethods.ShowWindow(projectVisualStudioHWND, NativeMethods.ShowWindowEnum.Restore);
				if (!succ)
					Debug.Log("ShowWindow(projectVisualStudioHWND) failed");
			}
			NativeMethods.SetForegroundWindow(projectVisualStudioHWND);

			int maxAttempts = 400;
			int ms = 5;
			int i = 0;
			IntPtr fileModificationDetectedHWND = IntPtr.Zero;
			do
			{
				fileModificationDetectedHWND = NativeMethods.FindWindowByCaption(IntPtr.Zero, WINDOW_CAPTION);
				System.Threading.Thread.Sleep(ms);
			}
			while (fileModificationDetectedHWND == IntPtr.Zero && ++i < maxAttempts);

			if (fileModificationDetectedHWND == IntPtr.Zero) // found no window modification => stay here to edit (since this is the final goal)
			{
				canOpenScript = true;
				return false;
			}

			NativeMethods.SetForegroundWindow(fileModificationDetectedHWND);

			IntPtr buttonPtr = IntPtr.Zero;
			int ii = 0;
			string label = null;
			bool found = false;
			do
			{
				buttonPtr = NativeMethods.FindWindowEx(fileModificationDetectedHWND, buttonPtr, "Button", null);
				label = GetWindowText(buttonPtr);
				found = label == "&Reload" || label.ToLower().Contains("reload");
			}
			while (!found && ++ii < 5 /*avoid potential infinite loop*/ && buttonPtr != IntPtr.Zero);

			if (found)
				NativeMethods.SendMessage(buttonPtr, 0x00F5 /*BM_CLICK*/, IntPtr.Zero, IntPtr.Zero);
			else
			{
				// shouldn't happen...
			}

			System.Threading.Thread.Sleep(100);

			string winText;
			var unityHWND = NativeMethods.FindWindow((win, _) => ((winText = GetWindowText(win)).Contains("Unity ")) && (winText.Contains(".unity - ") || winText.Contains("- Untitled -")/*the current scene is new & not saved*/) && winText.Contains("- " + projectName + " -"));
			if (unityHWND == IntPtr.Zero)
			{
				// TODO
			}
			else
			{
				if (NativeMethods.IsIconic(unityHWND))
					NativeMethods.ShowWindow(unityHWND, NativeMethods.ShowWindowEnum.Restore);
				NativeMethods.SetForegroundWindow(unityHWND);
			}

			System.Threading.Thread.Sleep(100);

			//// Send 'Enter'
			//keybd_event(0x0D, 0, 0, 0);
			canOpenScript = true;
			return true;

			//var vsProcesses = System.Diagnostics.Process.GetProcessesByName("devenv");

			//// Exactly one visual studio instance is needed
			//if (vsProcesses.Length != 1)
			//{
			//	Debug.Log("Len=" + vsProcesses.Length);
			//	canOpenScript = true;
			//	return false;
			//}

			//var visualStudioProcess = vsProcesses[0];
			//visualStudioProcess.Refresh();

			//if (visualStudioProcess.MainWindowHandle == IntPtr.Zero)
			//	return false;

			//visualStudioProcess.Refresh();

			//int i = 0;
			////for (; i < 20 && visualStudioProcess.MainWindowHandle == IntPtr.Zero; ++i)
			////{
			////	System.Threading.Thread.Sleep(100);
			////	visualStudioProcess.Refresh();
			////}
			//if (visualStudioProcess.MainWindowHandle != IntPtr.Zero)
			//	Debug.Log("i="+ i + ", " + visualStudioProcess.MainWindowHandle + ", " + visualStudioProcess.Handle);

			//if (visualStudioProcess.MainWindowHandle == IntPtr.Zero)
			//	return false;

			//bool windowShown = false;
			//if (IsIconic(visualStudioProcess.MainWindowHandle))
			//{
			//	// The window is minimized. try to restore it before setting focus
			//	ShowWindow(visualStudioProcess.MainWindowHandle, ShowWindowEnum.Restore);

			//	windowShown = true;
			//}

			//var unityProcess = System.Diagnostics.Process.GetCurrentProcess();

			//var sb = GetWindowText(visualStudioProcess.MainWindowHandle);
			//if (sb.Length <= 0)
			//{
			//	if (windowShown && (int)unityProcess.MainWindowHandle != 0)
			//		SetForegroundWindow(unityProcess.MainWindowHandle);

			//	canOpenScript = true;
			//	return false;
			//}
			//Debug.Log("LLL: " + sb + ", " + visualStudioProcess.MainWindowTitle);

			//Debug.Log(sb + "\n" + projectName);
			//if (!sb.Contains(projectName)) // this visual studio doesn't point to our solution => go back
			//{
			//	canOpenScript = true;
			//	if (windowShown)
			//	{
			//		if (unityProcess.MainWindowHandle == IntPtr.Zero) // hidden => show it
			//		{
			//			if (unityProcess.Handle == IntPtr.Zero)
			//				return false;

			//			ShowWindow(unityProcess.Handle, ShowWindowEnum.Restore);
			//		}

			//		if (unityProcess.MainWindowHandle != IntPtr.Zero)
			//			SetForegroundWindow(unityProcess.MainWindowHandle);
			//	}

			//	return false;
			//}

			//SetForegroundWindow(visualStudioProcess.MainWindowHandle);

			//var fileModificationDetectedHWND = FindWindowByCaption(IntPtr.Zero, WINDOW_CAPTION);
			//Debug.Log("fileModificationDetectedHWND="+fileModificationDetectedHWND);
			//if (fileModificationDetectedHWND == IntPtr.Zero) // found no window modification => stay here to edit (since this is the final goal)
			//{
			//	canOpenScript = true;
			//	// Switch back to unity
			//	//var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
			//	//if ((int)currentProcess.MainWindowHandle != 0)
			//	//{
			//	//	SetForegroundWindow(currentProcess.MainWindowHandle);
			//	//}
			//	return false;
			//}

			//SetForegroundWindow(fileModificationDetectedHWND);

			//// Send 'Enter'
			//keybd_event(0x0D, 0, 0, 0);
			//canOpenScript = true;
			//return true;
		}
#endif

		static bool IfPossible_ReloadVisualStudioSolutionIfOpened(out bool canOpenScript)
		{
			canOpenScript = true;
#if OSA_UNITY_EDITOR_WIN
			try { return ReloadVisualStudioSolutionIfOpened(out canOpenScript); }
			catch { }
#endif
			return false;
		}

		static bool CheckIfPossible_IsVisualStudioOpenedWithCurrentProjectAndBusy()
		{
#if OSA_UNITY_EDITOR_WIN
			try { return IsVisualStudioOpenedWithCurrentProjectAndBusy(); }
			catch { }
#endif
			return false;
		}

#endregion


		public static bool IsOpen() { return Resources.FindObjectsOfTypeAll(typeof(InitOSAWindow)).Length > 0; }

		public static void Open(Parameters windowParams)
		{
			InitOSAWindow windowInstance = GetWindow<InitOSAWindow>();
			windowInstance.InitWithNewParams(windowParams);
		}

		public static ValidationResult Validate(bool checkForWindows, ScrollRect scrollRect, bool allowMultipleScrollbars, Parameters parametersIfAlreadyCreated = null)
		{
			ValidationResult result = new ValidationResult();
			result.scrollRect = scrollRect;

			if (!BaseValidate(out result.reasonIfNotValid))
				return result;

			if (checkForWindows)
			{
				if (CreateOSAWindow.IsOpen())
				{
					result.reasonIfNotValid = "Creation window already opened";
					return result;
				}
				if (IsOpen())
				{
					result.reasonIfNotValid = "Initialization window already opened";
					return result;
				}
			}

			if (!scrollRect)
			{
				result.reasonIfNotValid = "The provided scrollrect is now null. Maybe it was destroyed meanwhile?";
				return result;
			}

			if (scrollRect.horizontal == scrollRect.vertical)
			{
				result.reasonIfNotValid = "Both 'horizontal' and 'vertical' properties are set to " + scrollRect.horizontal + ". Exactly one needs to be true.";
				return result;
			}

			var existingOSAComponents = scrollRect.GetComponents(typeof(IOSA));
			if (existingOSAComponents.Length > 0)
			{
				string[] s = DotNETCoreCompat.ConvertAllToArray(existingOSAComponents, c => " '" + c.GetType().Name + "' ");
				var sc = string.Concat(s);
				result.reasonIfNotValid = "ScrollRect contains " + existingOSAComponents.Length + " existing component(s) extending OSA (" + sc + "). Please remove any existing OSA component before proceeding";
				return result;
			}

			string requiresADirectViewportChild = "The ScrollRect requires a direct, active child named 'Viewport', which will contain the Content";

			var activeChildrenNamedViewport = new List<Transform>();
			foreach (Transform child in scrollRect.transform)
			{
				if (!child.gameObject.activeSelf)
					continue;
				if (child.name == "Viewport")
					activeChildrenNamedViewport.Add(child);
			}

			if (activeChildrenNamedViewport.Count == 0)
			{
				result.reasonIfNotValid = requiresADirectViewportChild;
				return result;
			}

			if (activeChildrenNamedViewport.Count > 1)
			{
				result.reasonIfNotValid = "The ScrollRect has more than one direct, active child named 'Viewport'";
				return result;
			}
			result.viewportRT = activeChildrenNamedViewport[0] as RectTransform;
			if (!result.viewportRT)
			{
				result.reasonIfNotValid = "The ScrollRect's child 'Viewport' does not have a RectTransform component";
				return result;
			}

			if (!scrollRect.content)
			{
				result.reasonIfNotValid = "The 'content' property is not set";
				return result;
			}

			if (scrollRect.content.parent != result.viewportRT)
			{
				result.reasonIfNotValid = "The 'content' property points to " + scrollRect.content + ", which is not a direct child of the ScrollRect";
				return result;
			}

			if (!scrollRect.content.gameObject.activeSelf)
			{
				result.reasonIfNotValid = "The 'content' property points to a game object that's not active";
				return result;
			}

			if (scrollRect.content.childCount > 0)
			{
				result.reasonIfNotValid = "The 'content' property points to a game object that has some children. The content should have none";
				return result;
			}

			var activeChildrenScrollbars = new List<Scrollbar>();
			foreach (Transform child in scrollRect.transform)
			{
				if (!child.gameObject.activeSelf)
					continue;
				var sb = child.GetComponent<Scrollbar>();
				if (sb)
					activeChildrenScrollbars.Add(sb);
			}

			if (activeChildrenScrollbars.Count > 0)
			{
				if (!allowMultipleScrollbars)
				{
					if (activeChildrenScrollbars.Count > 1)
					{
						result.reasonIfNotValid = "Found more than 1 Scrollbar among the ScrollRect's direct, active children";
						return result;
					}
				}

				result.scrollbar = activeChildrenScrollbars[0];
				bool sbIsHorizontal = result.scrollbar.direction == Scrollbar.Direction.LeftToRight || result.scrollbar.direction == Scrollbar.Direction.RightToLeft;
				if (sbIsHorizontal != scrollRect.horizontal)
				{
					// Only showing a warning, because the user may intentionally set it this way
					result.warning = "Init OSA: The scrollbar's direction is " + (sbIsHorizontal ? "horizontal" : "vertical") + ", while the ScrollRect is not. If this was intended, ignore this warning";
				}
			}

			if (parametersIfAlreadyCreated != null)
			{
#if OSA_PLAYMAKER
				if (parametersIfAlreadyCreated.playmakerSetupStarted)
				{
					if (!parametersIfAlreadyCreated.playmakerController)
					{
						result.reasonIfNotValid = "Controller not selected";
						return result;
					}

					if (!parametersIfAlreadyCreated.itemPrefab)
					{
						result.reasonIfNotValid = "itemPrefab not selected";
						return result;
					}

					if (!parametersIfAlreadyCreated.itemPrefab.GetComponent(typeof(PlayMakerFSM)))
					{
						result.reasonIfNotValid = "PlaymakerFSM not found on item prefab";
						return result;
					}
				}
#endif
			}

			result.isValid = true;
			return result;
		}



		protected override void InitWithNewParams(Parameters windowParams)
		{
			base.InitWithNewParams(windowParams);

			// Commented: alraedy done in the constructor with paramater
			//_WindowParams.ResetValues();
			InitializeAfterParamsSet();
			_WindowParams.UpdateAvailableOSAImplementations(true);
		}

		protected override void InitWithExistingParams()
		{
			//Debug.Log("InitWithExistingParams: _WindowParams.scrollRect=" + _WindowParams.scrollRect + "_WindowParams.Scrollbar=" + _WindowParams.Scrollbar);
			if (ScheduleCloseIfUndefinedState())
				return;

			base.InitWithExistingParams();
			_WindowParams.InitNonSerialized();
			InitializeAfterParamsSet();

			string scriptName = _WindowParams.generatedScriptNameToUse;
			string fullName;

			if (_State == State.ATTACH_EXISTING_OSA_PENDING)
			{
				// TODO if have time: create property only for keeping track of the selected template
			}
			else if (_State == State.RECOMPILATION_SELECT_GENERATED_IMPLEMENTATION_PENDING) // this represents the previous state, so now PRE changes to POST
			{
				string implementationsString = _WindowParams.availableImplementations.Count + ": ";
				foreach (var t in _WindowParams.availableImplementations)
				{
					implementationsString += t.Name + ", ";
				}
				implementationsString = implementationsString.Substring(0, implementationsString.Length - 2);

				if (string.IsNullOrEmpty(scriptName))
				{
					throw new OSAException("Internal error: _WindowParams.generatedScriptNameToUse is null after recompilation; " +
						"availableImplementations=" + implementationsString);
				}
				else if ((fullName = GetFullNameIfScriptExists(scriptName)) == null)
				{
					throw new OSAException("Internal error: Couldn't find the type's fullName for script '" + scriptName + "'. Did you delete the newly created script?\n " +
						"availableImplementations=" + implementationsString);
				}
				else
				{
					// Commented this is done in initInFirstOnGUI
					//_WindowParams.UpdateAvailableOSAImplementations();
					int index = _WindowParams.availableImplementations.FindIndex(t => t.FullName == fullName);
					if (index == -1)
					{
						throw new OSAException("Internal error: Couldn't find index of new implementation of '" + scriptName + "': " +
							"availableImplementations=" + _WindowParams.availableImplementations.Count + ", " +
							"given fullName=" + fullName);
					}

					_WindowParams.indexOfExistingImplementationToUse = index + 1; // skip the <generate> option
				}

				//_State = State.POST_RECOMPILATION_SELECT_GENERATED_IMPLEMENTATION_PENDING;
				//_State = State.POST_RECOMPILATION_RELOAD_SOLUTION_PENDING;
				_State = State.NONE;

				// Switch to visual studio to fake-press Reload due to solution being changed
				//if (_WindowParams.openForEdit)// && _WindowParams.indexOfExistingImplementationToUse > 0)
				//ReloadVisualStudioSolutionIfOpenedAndIfPossible();
				//bool b;
				//ReloadVisualStudioSolutionIfOpenedAndIfPossible(out b);

			}
		}

		protected override void GetErrorAndWarning(out string error, out string warning)
		{
			var vr = Validate(
				false, 
				_WindowParams == null ? null : _WindowParams.scrollRect, 
				_WindowParams == null ? false : _WindowParams.allowMultipleScrollbars, 
				_WindowParams
			);
			error = vr.reasonIfNotValid;
			warning = vr.warning;
			// TODO check if prefab is allowed and if the prefab is NOT the viewport, scrollrect content, scrollbar
		}

		protected override void UpdateImpl()
		{
			if (_State != State.CLOSE_PENDING)
				ScheduleCloseIfUndefinedState(); // do not return in case of true, since the close code is below

			switch (_State)
			{
				case State.CLOSE_PENDING:
					_State = State.NONE;
					Close();
					break;

				case State.RECOMPILATION_SELECT_GENERATED_IMPLEMENTATION_PENDING:
					// TODO think about if need to wait or something. maybe import/refresh assets
					break;

				//case State.POST_RECOMPILATION_RELOAD_SOLUTION_PENDING:
				//	//bool canOpenScript;
				//	//if (ReloadVisualStudioSolutionIfOpenedAndIfPossible(out canOpenScript) || canOpenScript)
				//	if (!CheckIfPossible_IsVisualStudioOpenedWithCurrentProjectAndBusy())
				//		_State = State.NONE;
				//	break;

				case State.ATTACH_EXISTING_OSA_PENDING:
					//case State.POST_RECOMPILATION_ATTACH_GENERATED_OSA_PENDING:
					if (!_WindowParams.ImplementationsInitialized)
						throw new OSAException("wat. it shold've been initialized in initwithexistingparams");

					// Don't disable the existing scrollbar, if it's to be reused
					if (_WindowParams.useScrollbar && !_WindowParams.GenerateDefaultScrollbar && _WindowParams.MiscScrollbarWasAlreadyPresent && _WindowParams.ScrollbarRT)
						ConfigureScrollView(_WindowParams.scrollRect, _WindowParams.viewportRT, _WindowParams.itemPrefab, _WindowParams.ScrollbarRT);
					else
						ConfigureScrollView(_WindowParams.scrollRect, _WindowParams.viewportRT, _WindowParams.itemPrefab);
					Canvas.ForceUpdateCanvases();
					if (_WindowParams.useScrollbar && _WindowParams.GenerateDefaultScrollbar)
						_WindowParams.scrollbar = InstantiateDefaultOSAScrollbar();
					var t = _WindowParams.ExistingImplementationToUse;
					//Debug.Log(t);
					_WindowParams.ScrollRectRT.gameObject.AddComponent(t);
#if OSA_PLAYMAKER
					if (_WindowParams.playmakerSetupStarted)
						(_WindowParams.ScrollRectRT.GetComponent(typeof(IOSA)) as MonoBehaviour).enabled = false; // need to start as disabled for playmaker
#endif
					// Selecting the game object is important. Unity starts the initial serialization of a script (and thus, setting a valid value to the OSA's _Params field)
					// only if its inspector is shown
					Selection.activeGameObject = _WindowParams.ScrollRectRT.gameObject;

					_State = State.POST_ATTACH_CONFIGURE_OSA_PENDING;
					break;

				case State.POST_ATTACH_CONFIGURE_OSA_PENDING:
					if (_WindowParams == null || !_WindowParams.ScrollRectRT)
					{
						_State = State.CLOSE_PENDING;
						break;
					}

					var iAdapter = _WindowParams.ScrollRectRT.GetComponent(typeof(IOSA)) as IOSA;
					if (iAdapter == null)
					{
						_State = State.CLOSE_PENDING;
						break;
					}

					if (iAdapter.BaseParameters == null)
						break;

					Type requiredInterfaceType = _WindowParams.GetOnlyAllowImplementationsHavingInterface();
					bool byConfigurator = false;
					if (requiredInterfaceType != null)
					{
						foreach (var type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()) // only from editor assembly
						{
							if (type.IsAbstract)
								continue;

							if (!OSAUtil.DotNETCoreCompat_IsAssignableFrom(typeof(ICustomAdapterConfigurator), type))
								continue;

							var attr = Attribute.GetCustomAttribute(type, typeof(CustomAdapterConfiguratorAttribute)) as CustomAdapterConfiguratorAttribute;
							if (attr == null)
								continue;

							if (attr.ConfiguredType != requiredInterfaceType)
								continue;

							var conf = Activator.CreateInstance(type) as ICustomAdapterConfigurator;
							conf.ConfigureNewAdapter(iAdapter);
							byConfigurator = true;
							break;
						}
					}

					if (!byConfigurator)
					{
						if (_WindowParams.scrollbar)
							ConfigureScrollbar(iAdapter);

#if OSA_PLAYMAKER
						if (_WindowParams.playmakerSetupStarted)
							PostAttachConfigurePlaymakerSetup();
					
#endif

						OnOSAParamsInitialized(iAdapter);
					}

					if (_WindowParams.openForEdit)
					{
						var monoScript = MonoScript.FromMonoBehaviour(iAdapter.AsMonoBehaviour);
						var success = AssetDatabase.OpenAsset(monoScript);
						if (success)
						{
							//	ReloadVisualStudioSolutionIfOpenedAndIfPossible();
						}
						else
							Debug.Log("OSA: Could not open '" + iAdapter.GetType().Name + "' in external code editor");
					}

					_State = State.PING_SCROLL_RECT_PENDING;
					break;

					//case State.POST_ATTACH_AND_POST_PING_CONFIGURE_OSA_PARAMS_PENDING:
					//	if (ConfigureOSAParamsPostAttachAndPostPing())
					//		_State = State.CLOSE_PENDING;
					//	break;
			}
		}

		protected override void OnGUIImpl()
		{
			if (_State != State.CLOSE_PENDING && ScheduleCloseIfUndefinedState())
				return;

			switch (_State)
			{
				case State.PING_SCROLL_RECT_PENDING: // can only be done in OnGUI because EditorStyles are used by EditorGUIUtility.PingObject
					if (_WindowParams.scrollRect)
					{
						PingAndSelect(_WindowParams.ScrollRectRT);
						//ShowNotification(new GUIContent("OSA: Initialized"));

						string msg = "OSA: Initialized";
						bool shownNotification = false;
						try
						{
							var inspectorWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
							var allInspectors = Resources.FindObjectsOfTypeAll(inspectorWindowType);
							if (allInspectors != null && allInspectors.Length == 1)
							{
								(allInspectors[0] as EditorWindow).ShowNotification(new GUIContent(msg));
								shownNotification = true;
							}
						}
						catch { }

						if (!shownNotification)
							Debug.Log(msg);

						if (_WindowParams.destroyScrollRectAfter)
						{
							GameObject.DestroyImmediate(_WindowParams.scrollRect);
						}
					}
					else
						Debug.Log("OSA: Unexpected state: the scrollrect was destroyed meanwhile. Did you delete it from the scene?");

					_State = State.CLOSE_PENDING;

					break;

				case State.RECOMPILATION_SELECT_GENERATED_IMPLEMENTATION_PENDING:
					EditorGUI.BeginDisabledGroup(true);
					{
						EditorGUILayout.BeginHorizontal(_BoxGUIStyle);
						{
							string scriptName = "(???)";
							if (_WindowParams != null && !string.IsNullOrEmpty(_WindowParams.generatedScriptNameToUse))
								scriptName = _WindowParams.generatedScriptNameToUse;
							scriptName = "'" + scriptName + "'";

							var style = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
							EditorGUILayout.LabelField("Waiting for script " + scriptName + " to be generated & attached...", style);
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUI.EndDisabledGroup();

					break;

				case State.POST_ATTACH_CONFIGURE_OSA_PENDING:
					EditorGUI.BeginDisabledGroup(true);
					{
						string s = _WindowParams != null && _WindowParams.scrollRect != null ? "(named '" + _WindowParams.scrollRect.name + "')" : "";
						EditorGUILayout.LabelField(
							"If this window stays open for too long, please select the newly initialized ScrollView in hierarchy " + s + "\n" +
							"This is done automatically, but it fails if you have locked the inspector window.\n" +
							"This also happens if your code editor is open with some pending changes and a 'Solution changed externally' " +
								"dialog box is shown - in this case, switch to it and select 'Reload solution'",
							GUILayout.Height(100f)
							);
					}
					EditorGUI.EndDisabledGroup();

					break;

				case State.PING_PREFAB_PENDING:
					if (_WindowParams != null && _WindowParams.itemPrefab)
						PingAndSelect(_WindowParams.itemPrefab);

					_State = State.NONE;
					goto case State.NONE;

				case State.NONE:
					if (_WindowParams.ImplementationsInitialized) // wait for params intialization
						DrawDefaultGUI();
					break; // continue drawing normally

				default:
					break;
			}

		}

		protected override void ConfigureScrollView(ScrollRect scrollRect, RectTransform viewport, params Transform[] objectsToSkipDisabling)
		{
			base.ConfigureScrollView(scrollRect, viewport, objectsToSkipDisabling);

			scrollRect.enabled = false;
			if (!_WindowParams.destroyScrollRectAfter)
				Debug.Log("OSA: Starting with v4.0, the ScrollRect component is not needed anymore. It was disabled and you should remove it to not interfere with OSA");
		}

		protected override void OnSubmitClicked()
		{
			// Commented: this is already checked and if there's and error, the submit button is disabled
			//// Validate again, to make sure the hierarchy wasn't modified
			//var validationRes = Validate(_WindowParams.scrollRect);
			//if (!validationRes.isValid)
			//{
			//	DemosUtil.ShowCouldNotExecuteCommandNotification(this);
			//	Debug.Log("OSA: Could not initialize (the hierarchy was probably modified): " + validationRes.reasonIfNotValid);
			//	return;
			//}

			bool generateNew = _WindowParams.ExistingImplementationToUse == null;
			if (generateNew)
			{
				if (string.IsNullOrEmpty(_WindowParams.generatedScriptNameToUse))
				{
					CWiz.ShowNotification("Invalid script name", true, this);
					return;
				}

				string alreadyExistingTypeFullName = GetFullNameIfScriptExists(_WindowParams.generatedScriptNameToUse);
				if (alreadyExistingTypeFullName != null)
				{
					CWiz.ShowNotification("Invalid script name. A script already exists as '" + alreadyExistingTypeFullName + "'", true, this);
					return;
				}

				string genScriptDirectoryPath = Application.dataPath + "/Scripts";
				string genScriptPath = genScriptDirectoryPath + "/" + _WindowParams.generatedScriptNameToUse + ".cs";

				if (File.Exists(genScriptPath))
				{
					CWiz.ShowNotification("A script named '" + _WindowParams.generatedScriptNameToUse + "' already exists", true, this);
					return;
				}

				if (!Directory.Exists(genScriptDirectoryPath))
				{
					try { Directory.CreateDirectory(genScriptDirectoryPath); }
					catch
					{
						Debug.LogError("OSA: Could not create directory: " + genScriptDirectoryPath);
						return;
					}
				}

				string templateText = _WindowParams.TemplateToUseForNewScript;

				// Replace the class name with the chosen one
				templateText = templateText.Replace(
					CWiz.TEMPLATE_TEXT_CLASSNAME_PREFIX + _WindowParams.availableTemplatesNames[_WindowParams.IndexOfTemplateToUseForNewScript],
					CWiz.TEMPLATE_TEXT_CLASSNAME_PREFIX + _WindowParams.generatedScriptNameToUse
				);

				// Add header
				templateText = _WindowParams.TemplateHeader + templateText;

				// Create unique namespace. Even if we're checking for any existing monobehaviour with the same name before creating a new one, 
				// the params, views holder and the model classes still have the same name
				CWiz.ReplaceTemplateDefaultNamespaceWithUnique(ref templateText);

				// Create, import and wait for recompilation
				try { File.WriteAllText(genScriptPath, templateText); }
				catch
				{
					CWiz.ShowCouldNotExecuteCommandNotification(this);
					Debug.LogError("OSA: Could not create file: " + genScriptPath);
					return;
				}
				// ImportAssetOptions
				//var v = AssetImporter.GetAtPath(FileUtil.GetProjectRelativePath(genScriptPath));
				//Debug.Log("v.GetInstanceID()" + v.GetInstanceID());
				//Debug.Log(FileUtil.GetProjectRelativePath(genScriptPath)+", " + genScriptPath);
				//AssetDatabase.ImportAsset(FileUtil.GetProjectRelativePath(genScriptPath));
				//AssetDatabase.ImportAsset(FileUtil.GetProjectRelativePath(genScriptPath), ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
				AssetDatabase.ImportAsset(FileUtil.GetProjectRelativePath(genScriptPath));
				//AssetDatabase.Refresh();
				// Will be executed in Update, but after re-compilation
				// TODO check
				_State = State.RECOMPILATION_SELECT_GENERATED_IMPLEMENTATION_PENDING;
			}
			else
				// Will be executed in the next Update
				_State = State.ATTACH_EXISTING_OSA_PENDING;
		}

		bool ScheduleCloseIfUndefinedState()
		{
			if (!_WindowParams.scrollRect)
			{
				if (_State != State.CLOSE_PENDING)
				{
					_State = State.CLOSE_PENDING;
					Debug.Log("OSA wizard closed because the ScrollRect was destroyed or the scene changed");
				}
				//DemosUtil.ShowNotification("OSA wizard closed because the ScrollRect was destroyed", false, false);
				//DestroyImmediate(this);
				return true;
			}

			return false;
		}

		void InitializeAfterParamsSet()
		{
			_MapParamBaseTypeToPrefabSetter = new Dictionary<Type, Action<BaseParams, RectTransform>>();
			_MapParamBaseTypeToPrefabSetter[typeof(GridParams)] = (parms, pref) => (parms as GridParams).Grid.CellPrefab = pref;
			_MapParamBaseTypeToPrefabSetter[typeof(BaseParamsWithPrefab)] = (parms, pref) => (parms as BaseParamsWithPrefab).ItemPrefab = pref;
		}

#if OSA_PLAYMAKER
		bool IsPlaymakerImplementation(out GameObject[] controllerPrefabs, out bool isGrid)
		{
			controllerPrefabs = null;
			isGrid = false;

			if (_WindowParams.ExistingImplementationToUse == null)
				return false;

			var nam = _WindowParams.ExistingImplementationToUse.Name;
			if (nam == typeof(Playmaker.Adapters.PlaymakerGridOSA).Name)
			{
				isGrid = true;
			}
			else if (nam == typeof(Playmaker.Adapters.PlaymakerListOSA).Name)
			{
			}
			else
				return false;

			string playmakerControllerPrefabsFolder = CWiz.TEMPLATES_PLAYMAKER_CONTROLLER_PREFABS_RESPATH;
			controllerPrefabs = Resources.LoadAll<GameObject>(playmakerControllerPrefabsFolder);
			
			return true;
		}
#endif

		RectTransform BasicTemplate_GetItemPrefabResourceForParamsBaseType(Type type)
		{
			string nameToUse;
			if (type == typeof(GridParams))
				nameToUse = Parameters.GRID_TEMPLATE_NAME;
			else if (type == typeof(BaseParamsWithPrefab))
				nameToUse = Parameters.LIST_TEMPLATE_NAME;
			//else if (type == typeof(TableParams))
			//	nameToUse = Parameters.TABLE_TEMPLATE_NAME;
			else 
				return null;

			//string prefabNameWithoutAdapter = nameToUse.Replace("Adapter", "");

			var go = Resources.Load<GameObject>(CWiz.GetExampleItemPrefabResPath(nameToUse));
			if (!go)
				return null;

			return go.transform as RectTransform;
		}

		void DrawDefaultGUI()
		{
			DrawSectionTitle("Implement OSA");

			// Game Object to initialize
			DrawObjectWithPath(_BoxGUIStyle, "ScrollRect to initialize", _WindowParams.scrollRect == null ? null : _WindowParams.scrollRect.gameObject);

			// Scrollbar
			EditorGUI.BeginDisabledGroup(!_WindowParams.canChangeScrollbars);
			EditorGUILayout.BeginVertical(_BoxGUIStyle);
			{
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("Scrollbar", EditorStyles.boldLabel, CWiz.LABEL_WIDTH);
					_WindowParams.useScrollbar = EditorGUILayout.Toggle(_WindowParams.useScrollbar, CWiz.VALUE_WIDTH);
				}
				EditorGUILayout.EndHorizontal();

				if (_WindowParams.useScrollbar)
				{
					EditorGUILayout.Space();

					if (_WindowParams.MiscScrollbarWasAlreadyPresent)
					{
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Generate scrollbar", CWiz.LABEL_WIDTH);
							_WindowParams.overrideMiscScrollbar = EditorGUILayout.Toggle("", _WindowParams.overrideMiscScrollbar, CWiz.VALUE_WIDTH);
						}
						EditorGUILayout.EndHorizontal();
						_WindowParams.scrollbar.gameObject.SetActive(!_WindowParams.overrideMiscScrollbar);
					}

					if (!_WindowParams.MiscScrollbarWasAlreadyPresent || _WindowParams.overrideMiscScrollbar)
					{
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Scrollbar position", CWiz.LABEL_WIDTH);
							_WindowParams.isScrollbarPosAtStart =
								GUILayout.SelectionGrid(
									_WindowParams.isScrollbarPosAtStart ? 0 : 1,
									_WindowParams.isHorizontal ? new string[] { "Top", "Bottom" } : new string[] { "Left", "Right" },
									2,
									CWiz.VALUE_WIDTH
								) == 0 ? true : false;
						}
						EditorGUILayout.EndHorizontal();
					}

					if (_WindowParams.canChangeScrollbars && _WindowParams.MiscScrollbarWasAlreadyPresent)
					{
						EditorGUILayout.HelpBox
						(
							_WindowParams.overrideMiscScrollbar ?
								"'" + _WindowParams.scrollbar.name + "' was disabled. The default scrollbar will be generated"
								:
								"An existing scrollbar was found ('" + _WindowParams.scrollbar.name + "') and it'll be automatically linked to OSA. " +
								"If you want to disable it & generate the default one instead, tick 'Generate scrollbar'", MessageType.Info
						);
					}
				}
			}
			EditorGUILayout.EndVertical();
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			// OSA implementation
			EditorGUILayout.BeginVertical(_BoxGUIStyle);
			{
				EditorGUILayout.LabelField("Script to use", EditorStyles.boldLabel, CWiz.LABEL_WIDTH);

				EditorGUILayout.Space();

				var indexOfExistingImplBefore = _WindowParams.indexOfExistingImplementationToUse;

				// Implementation to use
#if OSA_PLAYMAKER
				EditorGUI.BeginDisabledGroup(_WindowParams.playmakerSetupStarted);
#else
				EditorGUI.BeginDisabledGroup(false);
#endif
				{
					// Exclude examples/demos toggle
					EditorGUI.BeginDisabledGroup(!_WindowParams.allowChoosingExampleImplementations);
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Exclude examples/demos", CWiz.LABEL_WIDTH);
						var before = _WindowParams.excludeExampleImplementations;
						_WindowParams.excludeExampleImplementations = EditorGUILayout.Toggle(_WindowParams.excludeExampleImplementations, CWiz.VALUE_WIDTH);
						if (_WindowParams.excludeExampleImplementations != before)
							_WindowParams.UpdateAvailableOSAImplementations(true);
					}
					EditorGUILayout.EndHorizontal();
					EditorGUI.EndDisabledGroup();
					if (!_WindowParams.excludeExampleImplementations)
						EditorGUILayout.HelpBox("Using the provided example/demo scripts have no use in production. Intead, use them as a guide for implementing your own", MessageType.Warning);

					_WindowParams.indexOfExistingImplementationToUse =
						EditorGUILayout.Popup(_WindowParams.indexOfExistingImplementationToUse, _WindowParams.availableImplementationsStringsOptions, GUILayout.Width(CWiz.VALUE_WIDTH2_FLOAT));
				}
				EditorGUI.EndDisabledGroup();

				// When the user manually switches from generate to existing, don't keep the value of "openForEdit"
				if (indexOfExistingImplBefore != _WindowParams.indexOfExistingImplementationToUse && _WindowParams.indexOfExistingImplementationToUse > 0)
					_WindowParams.openForEdit = false;

				// OSA template to use if need to generate new implementation. 0 = <Create new>
				if (_WindowParams.indexOfExistingImplementationToUse == 0)
				{
					if (_WindowParams.availableTemplates.Length == 0)
						EditorGUILayout.HelpBox("There are no templates in */Resources/" + CWiz.TEMPLATE_SCRIPTS_RESPATH + ". Did you manually delete them? If not, this is a Unity bug and you can solve it by re-opening Unity", MessageType.Error);
					else
					{
						_WindowParams.IndexOfTemplateToUseForNewScript =
							GUILayout.SelectionGrid(_WindowParams.IndexOfTemplateToUseForNewScript, _WindowParams.availableTemplatesNames, 3, GUILayout.MinWidth(CWiz.VALUE_WIDTH_FLOAT));

						// Script name
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Generated script name", CWiz.LABEL_WIDTH);
							_WindowParams.generatedScriptNameToUse = EditorGUILayout.TextField(_WindowParams.generatedScriptNameToUse, CWiz.VALUE_WIDTH);

							// Name validation
							var filteredChars = new List<char>(_WindowParams.generatedScriptNameToUse.ToCharArray());
							filteredChars.RemoveAll(c => !char.IsLetterOrDigit(c));
							while (filteredChars.Count > 0 && char.IsDigit(filteredChars[0]))
								filteredChars.RemoveAt(0);
							_WindowParams.generatedScriptNameToUse = new string(filteredChars.ToArray());
						}
						EditorGUILayout.EndHorizontal();

						// Open for edit toggle
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Open for edit", CWiz.LABEL_WIDTH);
							_WindowParams.openForEdit = EditorGUILayout.Toggle(_WindowParams.openForEdit, CWiz.VALUE_WIDTH);
						}
						EditorGUILayout.EndHorizontal();
					}
				}
				else
				{
					if (_WindowParams.availableImplementations == null)
					{
						// TODO: shouldn't happen though
					}
					else
					{
						// Prefab, if applicable
						var implToUse = _WindowParams.ExistingImplementationToUse;
						Type paramsType = GetBaseTypeOfPrefabContainingParams(implToUse);
						if (paramsType == null)
						{
							// Having an interface enforced means there's a specific configurator that'll take care of prefab assigning and such
							if (_WindowParams.GetOnlyAllowImplementationsHavingInterface() == null)
							{
								EditorGUILayout.HelpBox(
									"Couldn't detect the params of '" + implToUse.Name + "' to set the prefab. Make sure to manually set it after, in inspector or (advanced) in code",
									MessageType.Warning
								);
							}
						}
						else
						{
							bool hidePrefabNotice = _WindowParams.itemPrefab != null;
#if OSA_PLAYMAKER
							GameObject[] playmakerControllerPrefabs;
							bool isGridPlaymaker;
							bool isPlaymakerImpl = IsPlaymakerImplementation(out playmakerControllerPrefabs, out isGridPlaymaker);
							hidePrefabNotice = hidePrefabNotice || isPlaymakerImpl;
#endif

							EditorGUILayout.HelpBox(
								"Params are of type '" + paramsType.Name + "', which contain a prefab property" +
								(hidePrefabNotice ? ":" : ". If you don't set it here, make sure to do it after, through inspector or (advanced) in code"),
								MessageType.Info
							);

							EditorGUILayout.BeginHorizontal();
							{
#if OSA_PLAYMAKER
								if (isPlaymakerImpl)
								{
									if (_WindowParams.playmakerSetupStarted)
									{
										if (!_WindowParams.playmakerController)
										{
											CWiz.ShowCouldNotExecuteCommandNotification(this);
											Debug.LogError("OSA: playmakerController externally deleted. Closing... ");
											Close();
											return;
										}

										EditorGUILayout.HelpBox(
											"Using Playmaker example controller '" + _WindowParams.playmakerController.name + "'", MessageType.Info);

										if (!_WindowParams.itemPrefab)
											DrawPlaymakerItemPrefabsForCurrentController(isGridPlaymaker);
									}
									else
									{
										DrawPlaymakerControllers(playmakerControllerPrefabs, isGridPlaymaker);
									}
								}
								else
#endif
								{
									EditorGUILayout.LabelField("Item prefab", CWiz.LABEL_WIDTH);
									_WindowParams.itemPrefab = EditorGUILayout.ObjectField(_WindowParams.itemPrefab, typeof(RectTransform), true, CWiz.VALUE_WIDTH) as RectTransform;

									if (!_WindowParams.itemPrefab)
									{
										var itemPreabRes = BasicTemplate_GetItemPrefabResourceForParamsBaseType(paramsType);
										if (itemPreabRes)
										{
											DrawItemPrefabs("Generate example for ", new GameObject[] { itemPreabRes.gameObject });
										}
									}
								}
							}
							EditorGUILayout.EndHorizontal();
						}
					}
				}
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			// Create button
			DrawSubmitButon(_WindowParams.ExistingImplementationToUse == null ? "Generate script" : "Initialize");
		}

#if OSA_PLAYMAKER
		void DrawPlaymakerControllers(GameObject[] controllerPrefabs, bool isGrid)
		{
			if (controllerPrefabs == null || controllerPrefabs.Length == 0)
			{
				EditorGUILayout.HelpBox("No controllers found for this Implementation. Choose another one.", MessageType.Warning);
				return;
			}

			EditorGUILayout.BeginVertical();
			int drawn = 0;
			for (int i = 0; i < controllerPrefabs.Length; i++)
			{
				var p = controllerPrefabs[i];

				var itemPrefabsAvailable = GetItemPrefabsAvailableForPlaymakerController(p.gameObject, isGrid);
				if (itemPrefabsAvailable.Count == 0)
					continue;

				string t = "Generate " + p.name.Replace("(Clone)", "");
				float w = GUI.skin.button.CalcSize(new GUIContent(t)).x + 10f;
				//float w = Mathf.Min(200f, Mathf.Max(350f, t.Length * 2))
				var buttonRect = EditorGUILayout.GetControlRect(GUILayout.Width(w), GUILayout.Height(20f));
				if (GUI.Button(buttonRect, t))
				{
					var instanceRT = (Instantiate(p) as GameObject).GetComponent<RectTransform>();
					instanceRT.name = instanceRT.name.Replace("(Clone)", "");
					instanceRT.SetParent(_WindowParams.ScrollRectRT.parent, false);
					instanceRT.SetAsLastSibling();
					instanceRT.SetSiblingIndex(_WindowParams.ScrollRectRT.GetSiblingIndex() + 1);
					_WindowParams.playmakerSetupStarted = true;
					_WindowParams.playmakerController = instanceRT;
				}
				++drawn;
			}
			EditorGUILayout.EndVertical();
			if (drawn == 0)
			{
				EditorGUILayout.HelpBox("Found controllers for this Implementation, but none of them has compatible item prefabs for it. Choose another one implementation.", MessageType.Warning);
			}
		}

		void DrawPlaymakerItemPrefabsForCurrentController(bool isGrid)
		{
			var filtered = GetItemPrefabsAvailableForPlaymakerController(_WindowParams.playmakerController.gameObject, isGrid);

			DrawItemPrefabs("Generate example prefab: ", filtered);
		}

		List<GameObject> GetItemPrefabsAvailableForPlaymakerController(GameObject controllerPrefab, bool isGrid)
		{
			var filtered = new List<GameObject>();

			string[] itemPrefabsForThisController;
			if (!_Playmaker_MapControllerToSupportedItemPrefabs.TryGetValue(controllerPrefab.name, out itemPrefabsForThisController))
				return filtered;

			string toRemoveThoseNotContainingThis;
			string loadPath;

			if (isGrid)
			{
				toRemoveThoseNotContainingThis = "PMGrid";
				loadPath = CWiz.TEMPLATES_PLAYMAKER_GRID_ITEM_PREFABS_RESPATH;
			}
			else
			{
				loadPath = CWiz.TEMPLATES_PLAYMAKER_LIST_ITEM_PREFABS_RESPATH;
				toRemoveThoseNotContainingThis = "PMList";
			}
			filtered.AddRange(Resources.LoadAll<GameObject>(loadPath));
			filtered.RemoveAll(itemPref => Array.IndexOf(itemPrefabsForThisController, itemPref.name) == -1 || !itemPref.name.Contains(toRemoveThoseNotContainingThis));

			return filtered;
		}
#endif

		void DrawItemPrefabs(string headline, IList<GameObject> itemPrefabs)
		{
			EditorGUILayout.BeginVertical();
			for (int i = 0; i < itemPrefabs.Count; i++)
			{
				var itemPrefab = itemPrefabs[i];

				string t = headline + itemPrefab.name.Replace("(Clone)", "");
				float w = GUI.skin.button.CalcSize(new GUIContent(t)).x + 10f;
				//float w = Mathf.Min(200f, Mathf.Max(350f, t.Length * 2))
				var buttonRect = EditorGUILayout.GetControlRect(GUILayout.Width(w), GUILayout.Height(20f));
				if (GUI.Button(buttonRect, t))
				{
					var instanceRT = (Instantiate(itemPrefab) as GameObject).GetComponent<RectTransform>();
					instanceRT.name = instanceRT.name.Replace("(Clone)", "");
					instanceRT.SetParent(_WindowParams.ScrollRectRT, false);
					instanceRT.SetAsLastSibling();
					_WindowParams.itemPrefab = instanceRT;
					_State = State.PING_PREFAB_PENDING;
				}
			}
			EditorGUILayout.EndVertical();
		}

		string GetFullNameIfScriptExists(string scriptName, bool fullnameProvided = false)
		{
			//var scriptNameOrig = scriptName;
			scriptName = scriptName.ToLower();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in assembly.GetTypes())
				{
					if (type.IsAbstract)
						continue;
					if (!type.IsClass)
						continue;
					if (type.IsGenericType)
						continue;
					if (type.IsNotPublic)
						continue;
					if (!OSAUtil.DotNETCoreCompat_IsAssignableFrom(typeof(MonoBehaviour), type))
						continue;

					if (fullnameProvided)
					{
						if (type.FullName.ToLower() == scriptName)
							return type.FullName;
					}
					else
					{
						if (type.Name.ToLower() == scriptName)
							return type.FullName;
					}
				}
			}

			return null;
		}

		Type GetBaseTypeOfPrefabContainingParams(Type derivedType)
		{
			var curDerivedType = derivedType;
			var prefabContainingParamsTypes = new List<Type>(_MapParamBaseTypeToPrefabSetter.Keys);
			while (curDerivedType != null && curDerivedType != typeof(object))
			{
				var genericArguments = curDerivedType.GetGenericArguments();
				var tParams = new List<Type>(genericArguments).Find(t => OSAUtil.DotNETCoreCompat_IsAssignableFrom(typeof(BaseParams), t));

				if (tParams != null)
				{
					Type type = prefabContainingParamsTypes.Find(t => CWiz.IsSubclassOfRawGeneric(t, tParams));
					if (type != null)
						return type;
				}

				curDerivedType = curDerivedType.BaseType;
			}

			return null;
		}

		bool SetPrefab(IOSA iAdapter, RectTransform prefab)
		{
			var type = GetBaseTypeOfPrefabContainingParams(iAdapter.GetType());
			if (type == null)
				return false;

			_MapParamBaseTypeToPrefabSetter[type](iAdapter.BaseParameters, prefab);

			return true;
		}

		Scrollbar InstantiateDefaultOSAScrollbar()
		{
			var respath = _WindowParams.isHorizontal ? CWiz.HOR_SCROLLBAR_RESPATH : CWiz.VERT_SCROLLBAR_RESPATH;
			var sbPrefab = Resources.Load<GameObject>(respath);
			var sbInstanceRT = (GameObject.Instantiate(sbPrefab) as GameObject).transform as RectTransform;
			sbInstanceRT.name = sbInstanceRT.name.Replace("(Clone)", "");
			sbInstanceRT.SetParent(_WindowParams.ScrollRectRT, false);

			return sbInstanceRT.GetComponent<Scrollbar>();
		}
		
		void ConfigureScrollbar(IOSA iAdapter)
		{
			OSAUtil.ConfigureDinamicallyCreatedScrollbar(_WindowParams.scrollbar, iAdapter, _WindowParams.viewportRT);

			if (_WindowParams.checkForMiscComponents)
				DisableOrNotifyAboutMiscComponents(_WindowParams.scrollbar.gameObject, "scrollbar", typeof(ScrollbarFixer8), typeof(Scrollbar));

			//if (_WindowParams.ScrollbarIsFromOSAPrefab)
			if (_WindowParams.GenerateDefaultScrollbar)
			{
				// The scrollbar is initially placed at end if it's from the default scrollbar prefab 
				if (_WindowParams.isScrollbarPosAtStart)
				{
					var sbInstanceRT = _WindowParams.ScrollbarRT;
					var newAnchPos = sbInstanceRT.anchoredPosition;
					int i = 1 - _WindowParams.Hor0_Vert1;
					var v = sbInstanceRT.anchorMin;
					v[i] = 1f - v[i];
					sbInstanceRT.anchorMin = v;
					v = sbInstanceRT.anchorMax;
					v[i] = 1f - v[i];
					sbInstanceRT.anchorMax = v;
					v = sbInstanceRT.pivot;
					v[i] = 1f - v[i];
					sbInstanceRT.pivot = v;
					newAnchPos[i] = -newAnchPos[i];
					sbInstanceRT.anchoredPosition = newAnchPos;
				}
			}
		}

		void PingAndSelect(Component c)
		{
			Selection.activeGameObject = c.gameObject;
			EditorGUIUtility.PingObject(c);
		}

		void OnOSAParamsInitialized(IOSA iAdapter)
		{
			//if (_WindowParams == null || _WindowParams.scrollRect == null)
			//	return true;

			//var iAdapter = _WindowParams.scrollRect.GetComponent(typeof(IOSA)) as IOSA;
			//if (iAdapter == null)
			//	return true; // shouldn't happen

			var baseParams = iAdapter.BaseParameters;
			//if (baseParams == null)
			//	return false; // wait until params initialized

			baseParams.ContentSpacing = 10f;
			baseParams.ContentPadding = new RectOffset(10, 10, 10, 10);

			baseParams.Content = _WindowParams.scrollRect.content;
			baseParams.Orientation = _WindowParams.isHorizontal ? BaseParams.OrientationEnum.HORIZONTAL : BaseParams.OrientationEnum.VERTICAL;
			baseParams.Scrollbar = _WindowParams.scrollbar;

			var gridParams = baseParams as GridParams;
			if (gridParams != null)
			{
				gridParams.Grid.GroupPadding = baseParams.IsHorizontal ? new RectOffset(0, 0, 10, 10) : new RectOffset(10, 10, 0, 0);
				gridParams.Grid.AlignmentOfCellsInGroup = TextAnchor.MiddleCenter;
			}

			baseParams.Viewport = _WindowParams.viewportRT;
			if (_WindowParams.itemPrefab)
			{
				var success = SetPrefab(iAdapter, _WindowParams.itemPrefab);
				if (!success)
					Debug.Log("OSA: Could not set the item prefab for '" + iAdapter.GetType().Name + "'. Make sure to manually set it through inspector or (advanced) in code");
			}
		}

#if OSA_PLAYMAKER
		void PostAttachConfigurePlaymakerSetup()
		{
			var osaProxy = _WindowParams.ScrollRectRT.gameObject.AddComponent<Playmaker.PlaymakerOSAProxy>();
			var controllerFSM = _WindowParams.playmakerController.GetComponent<PlayMakerFSM>();
			var itemPrefabFSM = _WindowParams.itemPrefab.GetComponent<PlayMakerFSM>();

			var itemPrefabFSMVar__config_osa_controller = itemPrefabFSM.FsmVariables.FindFsmGameObject("config_osa_controller");
			if (itemPrefabFSMVar__config_osa_controller != null)
				itemPrefabFSMVar__config_osa_controller.Value = controllerFSM.gameObject;

			var controllerFSMVar__config_osa = controllerFSM.FsmVariables.FindFsmObject("config_osa");
			if (controllerFSMVar__config_osa != null)
				controllerFSMVar__config_osa.Value = osaProxy;

			var lazyDataHelper = controllerFSM.GetComponent<Playmaker.PlaymakerOSALazyDataHelperProxy>();
			if (lazyDataHelper)
				lazyDataHelper.InitWithNewOSAProxy(osaProxy);

			// In case of CSF, override the default sample titles to have larger strings
			if (_WindowParams.itemPrefab.name.Contains("ContentSizeFitter"))
			{
				var controllerFSMVar__sample_titles = controllerFSM.FsmVariables.FindFsmArray("sample_titles");
				if (controllerFSMVar__sample_titles != null)
				{
					var len = Demos.Common.DemosUtil.LOREM_IPSUM.Length;
					for (int i = 0; i < controllerFSMVar__sample_titles.Length; i++)
						controllerFSMVar__sample_titles.Set(i, Demos.Common.DemosUtil.GetRandomTextBody(len / 10 + 1, len));
					controllerFSMVar__sample_titles.SaveChanges();
				}
			}
		}
#endif

		[Serializable]
		public class Parameters : BaseWindowParams, ISerializationCallbackReceiver
		{
#region Serialization
			public ScrollRect scrollRect;
			public RectTransform viewportRT;
			public Scrollbar scrollbar;

			// View state
			public bool useScrollbar, isScrollbarPosAtStart, overrideMiscScrollbar, allowMultipleScrollbars, canChangeScrollbars;
			public bool excludeExampleImplementations;
			public bool allowChoosingExampleImplementations;
			public string onlyAllowSpecificTemplate;
			public string onlyAllowImplementationsHavingInterface; // full name of the interface or null
			public int indexOfExistingImplementationToUse;
			public string generatedScriptNameToUse;
			public RectTransform itemPrefab;
			public bool destroyScrollRectAfter;
#if OSA_PLAYMAKER
			public RectTransform playmakerController;
			public bool playmakerSetupStarted;
#endif
			public bool openForEdit;

			[SerializeField]
			int _IndexOfTemplateToUseForNewScript = 0;
			[SerializeField]
			bool _MiscScrollbarWasAlreadyPresent = false;
#endregion

			[NonSerialized]
			public string[] availableTemplates;
			[NonSerialized]
			public string[] availableTemplatesNames;

			public string TemplateHeader
			{
				get
				{
					if (_TemplateHeader == null)
					{
						var headerComment = Resources.Load<TextAsset>(CWiz.TEMPLATE_SCRIPTS_HEADERCOMMENT_RESPATH);
						_TemplateHeader = headerComment.text;
					}

					return _TemplateHeader;
				}
			}
			[NonSerialized]
			public List<Type> availableImplementations;
			[NonSerialized]
			public string[] availableImplementationsStringsOptions;

			public override Vector2 MinSize { get { return new Vector2(700f, 500f); } }
			public bool ImplementationsInitialized { get { return availableImplementations != null; } }
			public bool MiscScrollbarWasAlreadyPresent { get { return _MiscScrollbarWasAlreadyPresent; } }

			public int IndexOfTemplateToUseForNewScript
			{
				get { return _IndexOfTemplateToUseForNewScript; }
				set
				{
					if (_IndexOfTemplateToUseForNewScript != value)
					{
						_IndexOfTemplateToUseForNewScript = value;
						generatedScriptNameToUse = null;
					}

					if (generatedScriptNameToUse == null && value >= 0)
						generatedScriptNameToUse = availableTemplatesNames[value];
				}
			}
			public string TemplateToUseForNewScript { get { return IndexOfTemplateToUseForNewScript < 0 ? null : availableTemplates[IndexOfTemplateToUseForNewScript]; } }
			//public string TemplateNameToUse { get { return indexOfTemplateToUse < 1 ? null : availableTemplatesNames[indexOfTemplateToUse - 1]; } }
			public Type ExistingImplementationToUse { get { return indexOfExistingImplementationToUse < 1 ? null : availableImplementations[indexOfExistingImplementationToUse - 1]; } }
			public bool GenerateDefaultScrollbar { get { return !MiscScrollbarWasAlreadyPresent || overrideMiscScrollbar; } }
			public RectTransform ScrollRectRT { get { return scrollRect.transform as RectTransform; } }
			public RectTransform ScrollbarRT { get { return scrollbar.transform as RectTransform; } }

			public const string LIST_TEMPLATE_NAME = "BasicListAdapter";
			public const string GRID_TEMPLATE_NAME = "BasicGridAdapter";
			public const string TABLE_TEMPLATE_NAME = "BasicTableAdapter";

			const string DEFAULT_TEMPLATE_TO_USE_FOR_NEW_SCRIPT_IF_EXISTS = LIST_TEMPLATE_NAME;

			string _TemplateHeader;


			public Parameters() { } // For unity serialization

			public static Parameters Create(
				ValidationResult validationResult, 
				bool allowMultipleScrollbars, 
				bool canChangeScrollbars, 
				bool allowChoosingExampleImplementations, 
				bool destroyScrollRectAfter = false,
				bool checkForMiscComponents = true,
				string onlyAllowSpecificTemplate = null, 
				Type onlyAllowImplementationsHavingInterface = null
			)
			{
				var p = new Parameters();
				p.scrollRect = validationResult.scrollRect;
				p.viewportRT = validationResult.viewportRT;
				p.scrollbar = validationResult.scrollbar;
				p._MiscScrollbarWasAlreadyPresent = p.scrollbar != null;

				p.ResetValues();

				p.destroyScrollRectAfter = destroyScrollRectAfter;
				p.checkForMiscComponents = checkForMiscComponents;
				p.allowMultipleScrollbars = allowMultipleScrollbars;
				p.canChangeScrollbars = canChangeScrollbars;
				p.onlyAllowSpecificTemplate = onlyAllowSpecificTemplate;
				p.onlyAllowImplementationsHavingInterface = onlyAllowImplementationsHavingInterface == null ? null : onlyAllowImplementationsHavingInterface.FullName;
				p.allowChoosingExampleImplementations = allowChoosingExampleImplementations;
				if (!p.allowChoosingExampleImplementations)
					p.excludeExampleImplementations = true;

				p.InitNonSerialized();

				return p;
			}


#region ISerializationCallbackReceiver implementation
			public void OnBeforeSerialize() { }
			//public void OnAfterDeserialize() { InitNonSerialized(); }
			// Commented: "Load is not allowed to be called durng serialization"
			public void OnAfterDeserialize() { }
#endregion

			public void InitNonSerialized()
			{
				var allTemplatesTextAssets = Resources.LoadAll<TextAsset>(CWiz.TEMPLATE_SCRIPTS_RESPATH);
				availableTemplatesNames = new string[allTemplatesTextAssets.Length];
				availableTemplates = new string[allTemplatesTextAssets.Length];
				for (int i = 0; i < allTemplatesTextAssets.Length; i++)
				{
					var ta = allTemplatesTextAssets[i];
					availableTemplatesNames[i] = ta.name;
					availableTemplates[i] = ta.text;
				}
				if (!string.IsNullOrEmpty(onlyAllowSpecificTemplate))
				{
					int index = Array.IndexOf(availableTemplatesNames, onlyAllowSpecificTemplate);
					if (index == -1)
					{
						availableTemplatesNames = new string[0];
						availableTemplates = new string[0];
					}
					else
					{
						availableTemplatesNames = new string[] { availableTemplatesNames[index] };
						availableTemplates = new string[] { availableTemplates[index] };
					}
				}
				else
				{
					var list = new List<string>(availableTemplatesNames);
					string tbTempl = TABLE_TEMPLATE_NAME;
					int idx = list.IndexOf(tbTempl);
					if (idx != -1)
					{
						// Table adapter can only be created by specifying it when initializing window
						list.RemoveAt(idx);
						availableTemplatesNames = list.ToArray();
						list = new List<string>(availableTemplates);
						list.RemoveAt(idx);
						availableTemplates = list.ToArray();
					}
				}

				if (_IndexOfTemplateToUseForNewScript >= availableTemplatesNames.Length)
					_IndexOfTemplateToUseForNewScript = availableTemplatesNames.Length - 1;

				UpdateAvailableOSAImplementations(false);
				if (indexOfExistingImplementationToUse >= availableImplementationsStringsOptions.Length)
					indexOfExistingImplementationToUse = availableImplementationsStringsOptions.Length - 1;
			}

			public Type GetOnlyAllowImplementationsHavingInterface()
			{
				if (string.IsNullOrEmpty(onlyAllowImplementationsHavingInterface))
					return null;

				var requiredInterfaceType = CWiz.GetTypeFromAllAssemblies(onlyAllowImplementationsHavingInterface);
				if (requiredInterfaceType != null)
					return requiredInterfaceType;

				return null;
			}

			public override void ResetValues()
			{
				base.ResetValues();

				isHorizontal = scrollRect.horizontal;
				useScrollbar = MiscScrollbarWasAlreadyPresent;
				overrideMiscScrollbar = false;
				isScrollbarPosAtStart = false;

				// OSA implementation
				excludeExampleImplementations = true;
				allowChoosingExampleImplementations = true;
				indexOfExistingImplementationToUse = 0; // create new
				//ResetIndexOfTemplateToUse();
				_IndexOfTemplateToUseForNewScript = -1;
				//itemPrefab = null;
				openForEdit = true;
				allowMultipleScrollbars = false;
				canChangeScrollbars = true;
				onlyAllowSpecificTemplate = null;
				onlyAllowImplementationsHavingInterface = null;
				destroyScrollRectAfter = false;
			}

			public void UpdateAvailableOSAImplementations(bool resetSelectedTemplateAndImplementation)
			{
				if (availableImplementations == null)
					availableImplementations = new List<Type>();
				else
					availableImplementations.Clear();

				Type requiredInterfaceType = GetOnlyAllowImplementationsHavingInterface();
				Type requiredNoInterfaceType = null;
				if (requiredInterfaceType == null)
				{
					// Table adapter can only be created by specifying it when initializing window
					//requiredNoInterfaceType = typeof(ITableAdapter);
					// Using direct string, as TableView package may not be imported
					requiredNoInterfaceType = CWiz.GetTypeFromAllAssemblies(CWiz.TV.TABLE_ADAPTER_INTERFACE_FULL_NAME);
				}
				CWiz.GetAvailableOSAImplementations(availableImplementations, excludeExampleImplementations, requiredInterfaceType, requiredNoInterfaceType);

				availableImplementationsStringsOptions = new string[availableImplementations.Count + 1];
				availableImplementationsStringsOptions[0] = "<Generate new from template>";
				for (int i = 0; i < availableImplementations.Count; i++)
					availableImplementationsStringsOptions[i + 1] = availableImplementations[i].Name;

				if (resetSelectedTemplateAndImplementation)
				{
					indexOfExistingImplementationToUse = 0; // default to create new
					ResetIndexOfTemplateToUse();
				}
			}

			void ResetIndexOfTemplateToUse()
			{
				int index = 0;
				if (availableTemplates != null)
					index = Array.IndexOf(availableTemplates, DEFAULT_TEMPLATE_TO_USE_FOR_NEW_SCRIPT_IF_EXISTS); // -1 if not exists
				if (index == -1 && availableTemplates.Length > 0) // ..but 0 if there are others
					index = 0;
				IndexOfTemplateToUseForNewScript = index;
			}

		}


		public class ValidationResult
		{
			public bool isValid;
			public string reasonIfNotValid;
			public string warning;
			public RectTransform viewportRT;
			public Scrollbar scrollbar;
			public ScrollRect scrollRect;

			public override string ToString()
			{
				return "isValid = " + isValid + "\n" +
						"viewportRT = " + (viewportRT == null ? "(null)" : viewportRT.name) + "\n" +
						"scrollbar = " + (scrollbar == null ? "(null)" : scrollbar.name) + "\n" +
						"scrollRect = " + (scrollRect == null ? "(null)" : scrollRect.name) + "\n";
			}
		}


		enum State
		{
			NONE,
			//POST_RECOMPILATION_SELECT_GENERATED_IMPLEMENTATION_PENDING,
			RECOMPILATION_SELECT_GENERATED_IMPLEMENTATION_PENDING,
			//POST_RECOMPILATION_RELOAD_SOLUTION_PENDING,
			ATTACH_EXISTING_OSA_PENDING,
			POST_ATTACH_CONFIGURE_OSA_PENDING,
			PING_SCROLL_RECT_PENDING,
			//POST_ATTACH_AND_POST_PING_CONFIGURE_OSA_PARAMS_PENDING,
			PING_PREFAB_PENDING,
			//PING_PREFAB_PENDING_STEP_2,
			CLOSE_PENDING
		}
	}
}
