#if UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
#define SCROLLRECT_HAS_VIEWPORT
#endif

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;

namespace Com.TheFallenGames.OSA.Editor.OSAWizard
{
	public class CreateOSAWindow : BaseOSAWindow<CreateOSAWindow.Parameters>
	{
		public static bool IsOpen() { return Resources.FindObjectsOfTypeAll(typeof(CreateOSAWindow)).Length > 0; }

		public static void Open(Parameters windowParams)
		{
			CreateOSAWindow windowInstance = GetWindow<CreateOSAWindow>();
			windowInstance.InitWithNewParams(windowParams);
		}

		public static bool Validate(bool forOpeningWindow, out string reasonIfNotValid)
		{
			if (!BaseValidate(out reasonIfNotValid))
				return false;

			if (forOpeningWindow)
			{
				// Commented: it's safe to re-open the create window
				//if (IsOpen())
				//{
				//	reasonIfNotValid = "Creation window already opened";
				//	return false;
				//}
				if (InitOSAWindow.IsOpen())
				{
					reasonIfNotValid = "Initialization window already opened";
					return false;
				}
			}

			if (Selection.gameObjects.Length != 1)
			{
				reasonIfNotValid = "One UI Game Object needs to be selected";
				return false;
			}

			var asRT = Selection.gameObjects[0].transform as RectTransform;
			if (!asRT)
			{
				reasonIfNotValid = "The selected Game Object doesn't have a RectTransform component";
				return false;
			}

			if (asRT.rect.height <= 0f)
			{
				reasonIfNotValid = "The selected Game Object has an invalid height";
				return false;
			}

			if (asRT.rect.width <= 0f)
			{
				reasonIfNotValid = "The selected Game Object has an invalid width";
				return false;
			}

			if (!GameObject.FindObjectOfType<EventSystem>())
			{
				reasonIfNotValid = "No EventSystem was found in the scene. Please add one";
				return false;
			}

			reasonIfNotValid = null;
			var tr = asRT as Transform;
			while (tr)
			{
				if (!(tr is RectTransform))
				{
					reasonIfNotValid =
							"Found a non-RectTransform intermediary parent before first Canvas ancestor. " +
							"Your hierarchy may be something like: ...Canvas->...->Transform->...-><SelectedObject>. " +
							"There should only be RectTransforms between the selected object and its most close Canvas ancestor";
					return false;
				}

				var c = tr.GetComponent<Canvas>();
				if (c)
					return true;

				tr = tr.parent;
			}

			reasonIfNotValid = "Couldn't find a Canvas in the parents of the selected Game Object";
			return false;
		}


		protected override void InitWithNewParams(Parameters windowParams)
		{
			base.InitWithNewParams(windowParams);

			_WindowParams.ResetValues();
		}

		protected override void OnGUIImpl()
		{
			DrawSectionTitle("Create ScrollView");

			string titleToSet;
			UnityEngine.Object obj = null;
			if (Selection.gameObjects.Length == 0)
			{
				titleToSet = "(Select a parent)";
			}
			else if (Selection.gameObjects.Length > 1)
			{
				titleToSet = "(Select only 1 parent)";
			}
			else
			{
				obj = Selection.gameObjects[0];
				titleToSet = "ScrollView's parent";
			}
			DrawObjectWithPath(_BoxGUIStyle, titleToSet, obj);

			EditorGUI.BeginDisabledGroup(!obj);
			{
				// Orientation
				EditorGUILayout.BeginVertical(_BoxGUIStyle);
				{
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Orientation:", CWiz.LABEL_WIDTH);
						_WindowParams.isHorizontal = GUILayout.SelectionGrid(_WindowParams.Hor0_Vert1, new string[] { "Horizontal", "Vertical" }, 2, CWiz.VALUE_WIDTH) == 0 ? true : false;
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.Space();

			// Create button
			DrawSubmitButon("Create");
		}

		protected override void GetErrorAndWarning(out string error, out string warning)
		{
			warning = null;
			Validate(false, out error);
		}

		protected override void OnSubmitClicked()
		{
			// Commented: this is already checked and if there's and error, the submit button is disabled
			//string reasonIfNotValid;
			//// Validate again, to make sure the hierarchy wasn't modified
			//if (!Validate(out reasonIfNotValid))
			//{
			//	DemosUtil.ShowCouldNotExecuteCommandNotification(this);
			//	Debug.Log("OSA: Could not create ScrollView on the selected object: " + reasonIfNotValid);
			//	return;
			//}
			var parentGO = Selection.gameObjects[0];
			GameObject go = new GameObject("OSA", typeof(RectTransform));
			var image = go.AddComponent<Image>();
			var c = Color.white;
			c.a = .13f;
			image.color = c;
			var scrollRect = go.AddComponent<ScrollRect>();
			var scrollRectRT = scrollRect.transform as RectTransform;
			var parentRT = parentGO.transform as RectTransform;
			scrollRectRT.anchorMin = new Vector2(Mathf.Clamp01(CWiz.SPACE_FOR_SCROLLBAR / parentRT.rect.width), Mathf.Clamp01(CWiz.SPACE_FOR_SCROLLBAR / parentRT.rect.height));
			scrollRectRT.anchorMax = Vector2.one - scrollRectRT.anchorMin;
			scrollRectRT.sizeDelta = Vector2.zero;

			GameObjectUtility.SetParentAndAlign(go, parentGO);
			var viewportRT = CreateRTAndSetParent("Viewport", go.transform);
			viewportRT.gameObject.AddComponent<Image>();
			viewportRT.gameObject.AddComponent<Mask>().showMaskGraphic = false;
			var contentRT = CreateRTAndSetParent("Content", viewportRT);

			scrollRect.content = contentRT;
#if SCROLLRECT_HAS_VIEWPORT
			scrollRect.viewport = viewportRT;
#endif
			Canvas.ForceUpdateCanvases();

			// Register the creation in the undo system
			Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
			Selection.activeObject = go;

			ConfigureScrollView(scrollRect, viewportRT);
			Close();

			var validationResult = InitOSAWindow.Validate(false, scrollRect, false); // checkForWindows=false, becase this windows is already opened
			if (!validationResult.isValid)
			{
				CWiz.ShowCouldNotExecuteCommandNotification(this);
				Debug.LogError("OSA: Unexpected internal error while trying to initialize. Details(next line):\n" + validationResult.reasonIfNotValid + "\n" + validationResult.ToString());
				return;
			}

			InitOSAWindow.Open(InitOSAWindow.Parameters.Create(validationResult, false, true, true, true));
		}


		[Serializable]
		public class Parameters : BaseWindowParams
		{

			public override void ResetValues()
			{
				base.ResetValues();

			}
		}
	}
}
