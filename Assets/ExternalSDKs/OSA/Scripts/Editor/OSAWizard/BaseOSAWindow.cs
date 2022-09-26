using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using frame8.Logic.Misc.Visual.UI.MonoBehaviours;
using Com.TheFallenGames.OSA.Core;

namespace Com.TheFallenGames.OSA.Editor.OSAWizard
{
	public abstract class BaseOSAWindow<TWindowParams> : EditorWindow where TWindowParams : BaseWindowParams
	{
		[SerializeField]
		protected TWindowParams _WindowParams = null;

		[NonSerialized]
		protected int _CurrentFrameInSlowUpdateCycle;
		[NonSerialized]
		protected GUIStyle _RootGUIStyle, _BoxGUIStyle;
		[NonSerialized]
		bool _GUIResourcesInitialized;
		[NonSerialized]
		Texture2D _Icon;
		[NonSerialized]
		Texture2D _TopToBottomGradient;

		Color _MainColor = new Color(219 / 255f, 195 / 255f, 166 / 255f, .2f);


		protected virtual string CompilingScriptsText { get { return "Compiling scripts..."; } }


		protected static bool BaseValidate(out string reasonIfNotValid)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				reasonIfNotValid = "OSA wizard closed: Cannot be used in play mode";
				return false;
			}

			reasonIfNotValid = null;
			return true;
		}


		#region Unity methods
		protected void OnEnable()
		{
			if (_WindowParams != null) // most probably, after a script re-compilation
				InitWithExistingParams();
		}

		protected void OnDisable()
		{
			ReleaseOnGUIResources();
		}

		protected void Update()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				// Not allowed in play mode
				CWiz.ShowNotification("Cannot use OSA wizard during play mode", false, false);
				Close();
				return;
			}

			UpdateImplWithoutChecks();

			// Wait for scripts recompilation
			if (EditorApplication.isCompiling)
				return;

			// It's ok to delay the starting of updates until the gui resources are initialized, in order to have averything prepared
			if (_GUIResourcesInitialized)
				UpdateImpl();

			// SlowUpdate calling
			if (_CurrentFrameInSlowUpdateCycle % CWiz.SLOW_UPDATE_SKIPPED_FRAMES == 0)
			{
				_CurrentFrameInSlowUpdateCycle = 0;
				SlowUpdate();
			}
			else
				++_CurrentFrameInSlowUpdateCycle;
		}

		protected void OnGUI()
		{
			if (!_GUIResourcesInitialized)
			{
				InitOnGUIResources();
				_GUIResourcesInitialized = true;
			}

			var prevColor = GUI.color;
			GUI.color = _MainColor;
			var r = position;
			r.position = Vector2.zero;
			GUI.DrawTexture(r, EditorGUIUtility.whiteTexture);
			GUI.color = prevColor;

			DrawIcon();

			// Wait for scripts recompilation
			if (EditorApplication.isCompiling)
			{
				var style = new GUIStyle();
				style.alignment = TextAnchor.MiddleCenter;
				style.normal = new GUIStyleState();
				style.normal.textColor = Color.gray;
				EditorGUILayout.LabelField(CompilingScriptsText, style, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
				return;
			}

			EditorGUILayout.BeginVertical(_RootGUIStyle);
			{
				OnGUIImpl();
			}
			EditorGUILayout.EndVertical();
		}
		#endregion

		protected virtual void InitWithNewParams(TWindowParams windowParams)
		{
			//title = GetType().Name.Replace("OSAWindow", " OSA");
			string titleString = "OSA Wizard";
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
			titleContent = new GUIContent(titleString);
#else
			title = titleString;
#endif
			minSize = windowParams.MinSize;
			_WindowParams = windowParams;
		}

		protected virtual void InitWithExistingParams()
		{

		}

		protected virtual void InitOnGUIResources()
		{
			_Icon = AssetDatabase.LoadAssetAtPath("Assets/OSA/Textures/EditorOnly/osa-icon.png", typeof(Texture2D)) as Texture2D;
			_TopToBottomGradient = AssetDatabase.LoadAssetAtPath("Assets/OSA/Textures/EditorOnly/gradient.png", typeof(Texture2D)) as Texture2D;

			_RootGUIStyle = new GUIStyle();
			_RootGUIStyle.padding = new RectOffset(20, 20, 15, 25);
			_RootGUIStyle.alignment = TextAnchor.UpperCenter;

			_BoxGUIStyle = new GUIStyle(EditorStyles.textArea);
			_BoxGUIStyle.normal = new GUIStyleState();
			_BoxGUIStyle.normal.background = _TopToBottomGradient;
			_BoxGUIStyle.padding = new RectOffset(5, 5, 5, 5);
		}

		protected virtual void ReleaseOnGUIResources()
		{
			if (_Icon)
			{
				Resources.UnloadAsset(_Icon);
				_Icon = null;
			}
			if (_TopToBottomGradient)
			{
				Resources.UnloadAsset(_TopToBottomGradient);
				_TopToBottomGradient = null;
			}
		}

		protected virtual void UpdateImplWithoutChecks() { }

		protected virtual void UpdateImpl() { }

		protected abstract void OnGUIImpl();

		protected virtual void ConfigureScrollView(ScrollRect scrollRect, RectTransform viewport, params Transform[] objectsToSkipDisabling)
		{
			scrollRect.horizontal = _WindowParams.isHorizontal;
			scrollRect.vertical = !scrollRect.horizontal;
			scrollRect.verticalScrollbar = scrollRect.horizontalScrollbar = null;

			if (!_WindowParams.checkForMiscComponents)
				return;

			DisableOrNotifyAboutMiscComponents(scrollRect.gameObject, "ScrollRect", typeof(ScrollRect));
			foreach (Transform child in scrollRect.transform)
			{
				if (child.name == "Viewport")
					continue;
				if (child.GetComponent<ScrollbarFixer8>())
					continue;

				if (child.gameObject.activeSelf)
				{
					if (Array.IndexOf(objectsToSkipDisabling, child) != -1)
						continue;

					string scrollbarFixer = typeof(ScrollbarFixer8).Name;
					bool isScrollbar = child.name.ToLower().Contains("scrollbar") && child.GetComponent<Scrollbar>();
					string suffix = !isScrollbar ? "You can activate it back if it doesn't interfere with OSA"
						:
						("This appears to be a Scrollbar, but it wasn't added by the OSA wizard. If you want to use it, activate it back " +
								(child.GetComponent<ScrollbarFixer8>() != null ?
									(" and make sure its " + scrollbarFixer + " component is properly configured in inspector")
										: (", add a " + scrollbarFixer + " component and make sure it's properly configured in inspector")
								)
						);
					Debug.Log("OSA: De-activating ScrollRect's unknown child '" + child.name + "'. " + suffix);
					child.gameObject.SetActive(false);
				}
			}

			DisableOrNotifyAboutMiscComponents(viewport.gameObject, "Viewport", typeof(Mask));
			foreach (Transform child in viewport.transform)
			{
				if (child == scrollRect.content)
					continue;

				if (child.gameObject.activeSelf)
				{
					if (Array.IndexOf(objectsToSkipDisabling, child) != -1)
						continue;

					Debug.Log("OSA: De-activating Viewport's unknown child '" + child.name + "'. You can activate it back if it doesn't interfere with OSA");
					child.gameObject.SetActive(false);
				}
			}

			DisableOrNotifyAboutMiscComponents(scrollRect.content.gameObject, "Content");
			foreach (Transform child in scrollRect.content)
			{
				if (child.gameObject.activeSelf)
				{
					if (Array.IndexOf(objectsToSkipDisabling, child) != -1)
						continue;

					Debug.Log("OSA: De-activating Content's unknown child '" + child.name + "'. You can activate it back if it doesn't interfere with OSA");
					child.gameObject.SetActive(false);
				}
			}
		}

		protected abstract void GetErrorAndWarning(out string error, out string warning);

		protected virtual void OnSubmitClicked()
		{

		}

		protected void SlowUpdate()
		{
			//if (FullyInitialized)
			Repaint();
		}

		protected void DrawIcon()
		{
			float iconSize = 50f;
			float padding = 3f;
			var r = new Rect();
			float labelHeight = 15f;
			r.width = r.height = iconSize;
			r.position = new Vector2(position.width - iconSize - padding, position.height - iconSize - labelHeight - padding);
			var prevColor = GUI.color;
			var newColor = Color.white;
			newColor.a = .6f;
			GUI.color = newColor;
			GUI.DrawTexture(r, _Icon);
			r.position = new Vector3(r.position.x, r.position.y + r.height + padding);
			r.height = labelHeight;
			var style = new GUIStyle();
			style.fontSize = 9;
			style.fontStyle = FontStyle.Bold;
			style.normal = new GUIStyleState();
			style.normal.textColor = Color.white;
			newColor.a = .95f;
			GUI.color = newColor;
			GUI.Label(r, "OSA v" + OSAConst.OSA_VERSION_STRING, style);
			GUI.color = prevColor;
		}

		protected void DrawSectionTitle(string title)
		{
			var titleStyle = new GUIStyle(EditorStyles.label);
			titleStyle.alignment = TextAnchor.MiddleCenter;
			titleStyle.fontStyle = FontStyle.Bold;
			titleStyle.fontSize = 20;
			EditorGUILayout.LabelField(title, titleStyle, GUILayout.Height(25f));

			EditorGUILayout.Space();
			EditorGUILayout.Space();
		}

		protected void DrawObjectWithPath<T>(GUIStyle style, string title, T objectToDraw) where T : UnityEngine.Object
		{
			EditorGUILayout.BeginVertical(style);
			{
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField(title, EditorStyles.boldLabel, CWiz.LABEL_WIDTH);

					if (objectToDraw)
					{
						EditorGUI.BeginDisabledGroup(true);
						EditorGUILayout.ObjectField(objectToDraw, typeof(T), true, CWiz.VALUE_WIDTH);
						EditorGUILayout.LabelField("(ReadOnly)");
						EditorGUI.EndDisabledGroup();
					}
				}
				EditorGUILayout.EndHorizontal();

				if (objectToDraw)
				{
					string s = "";
					s = objectToDraw.name;
					var asGO = objectToDraw as GameObject;
					var asComponent = objectToDraw as Component;
					var tr = asGO == null ? asComponent.transform : asGO.transform;
					while (tr = tr.parent)
						s = tr.name + "/" + s;
					s = "Path: " + s;

					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.TextArea(s, EditorStyles.label);
					EditorGUI.EndDisabledGroup();
				}
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.Space();
			EditorGUILayout.Space();
		}

		protected void DrawSubmitButon(string title)
		{
			Rect buttonCRect = new Rect();
			buttonCRect.width = 170f;
			buttonCRect.height = 30f;
			buttonCRect.x = (position.width - buttonCRect.width) / 2;
			buttonCRect.y = position.height - buttonCRect.height - _RootGUIStyle.padding.bottom;
			string error, warning;
			GetErrorAndWarning(out error, out warning);
			bool isError = !string.IsNullOrEmpty(error);
			if (isError)
				EditorGUILayout.HelpBox(error, MessageType.Error);
			if (!string.IsNullOrEmpty(warning))
				EditorGUILayout.HelpBox(warning, MessageType.Warning);
			EditorGUI.BeginDisabledGroup(isError);
			{
				if (GUI.Button(buttonCRect, title))
					OnSubmitClicked();
			}
			EditorGUI.EndDisabledGroup();
		}

		protected RectTransform CreateRTAndSetParent(string name, Transform parent)
		{
			var go = new GameObject(name, typeof(RectTransform));
			var rt = go.transform as RectTransform;
			rt.SetParent(parent, false);
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.sizeDelta = Vector2.zero;

			return rt;
		}

		protected void DisableOrNotifyAboutMiscComponents(GameObject go, string nameToUse, params System.Type[] typesToIgnore)
		{
			var components = go.GetComponents<Component>();
			foreach (var comp in components)
			{
				if (comp is Transform)
					continue;
				if (comp is CanvasRenderer)
					continue;
				if (comp is Image)
					continue;
				if (System.Array.Exists(typesToIgnore, t => OSAUtil.DotNETCoreCompat_IsAssignableFrom(t, comp.GetType())))
					continue;
				var mb = comp as MonoBehaviour;
				if (mb)
				{
					if (mb.enabled)
					{
						Debug.Log("OSA: Disabling unknown component " + mb.GetType().Name + " on " + nameToUse + ". You can enable it back if it doesn't interfere with OSA");
						mb.enabled = false;
					}
					continue;
				}

				Debug.Log("OSA: Found unknown component '" + comp.GetType().Name + "' on " + nameToUse + ". Make sure it doesn't interfere with OSA");
			}
		}
	}

	[Serializable]
	public class BaseWindowParams
	{
		public bool isHorizontal;
		public bool checkForMiscComponents;

		public virtual Vector2 MinSize { get { return new Vector2(480f, 300f); } }

		public int Hor0_Vert1 { get { return isHorizontal ? 0 : 1; } }


		public virtual void ResetValues()
		{
			isHorizontal = false;
			checkForMiscComponents = true;
		}
	}
}
