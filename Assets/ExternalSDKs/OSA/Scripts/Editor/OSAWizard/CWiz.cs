using System;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using Com.TheFallenGames.OSA.Core;
using System.Collections.Generic;

namespace Com.TheFallenGames.OSA.Editor.OSAWizard
{
	public static class CWiz
	{
		public const string HOR_SCROLLBAR_RESPATH = "OSA/Templates/Scrollbars/HorizontalScrollbar";
		public const string VERT_SCROLLBAR_RESPATH = "OSA/Templates/Scrollbars/VerticalScrollbar";
		public const string TEMPLATE_TEXT_CLASSNAME_PREFIX = "public class ";
		public const string TEMPLATE_DEFAULT_NAMESPACE = "Your.Namespace.Here";

		public const string TEMPLATES_RESPATH = "OSA/Templates";
		public const string TEMPLATE_SCRIPTS_RESPATH = TEMPLATES_RESPATH + "/Scripts";
		public const string TEMPLATE_SCRIPTS_HEADERCOMMENT_RESPATH = TEMPLATES_RESPATH + "/ScriptsChunks/HeaderComment";
		public const string TEMPLATE_ITEM_PREFABS_DIR_RESPATH = TEMPLATES_RESPATH + "/ExampleItemPrefabs";
		public const string TEMPLATE_SCROLLVIEW_PREFABS_DIR_RESPATH = TEMPLATES_RESPATH + "/ScrollViews";
#if OSA_PLAYMAKER
		public const string TEMPLATES_PLAYMAKER_CONTROLLER_PREFABS_RESPATH = TEMPLATES_RESPATH + "/ExampleControllers/Playmaker";
		public const string TEMPLATES_PLAYMAKER_ITEM_PREFABS_RESPATH = TEMPLATES_RESPATH + "/ExampleItemPrefabs/Playmaker";
		public const string TEMPLATES_PLAYMAKER_GRID_ITEM_PREFABS_RESPATH = TEMPLATES_PLAYMAKER_ITEM_PREFABS_RESPATH + "/Grid";
		public const string TEMPLATES_PLAYMAKER_LIST_ITEM_PREFABS_RESPATH = TEMPLATES_PLAYMAKER_ITEM_PREFABS_RESPATH + "/List";
#endif
		public const int SLOW_UPDATE_SKIPPED_FRAMES = 5;
		public const float SPACE_FOR_SCROLLBAR = 27 + 20;
		public const float VALUE_WIDTH_FLOAT = 200f;
		public const float VALUE_WIDTH2_FLOAT = 250f;
		public static GUILayoutOption LABEL_WIDTH = GUILayout.Width(150f);
		public static GUILayoutOption VALUE_WIDTH = GUILayout.Width(VALUE_WIDTH_FLOAT);


		public static string GetExampleItemPrefabResPath(string templateName)
		{
			//templateName = templateName.Replace("Adapter", "");
			string prefabName = templateName + "Item";
			return TEMPLATE_ITEM_PREFABS_DIR_RESPATH + "/" + prefabName;
		}

		public static EditorWindow GetBestEditorWindowToShowNotification(bool allowFocusedWindow = true)
		{
			EditorWindow editorWindow = EditorWindow.focusedWindow;
			if (!editorWindow)
			{
				editorWindow = EditorWindow.mouseOverWindow;
				if (!editorWindow)
					editorWindow = EditorWindow.GetWindow<SceneView>();
			}

			return editorWindow;
		}

		public static void ShowNotification(string msg, bool beep, bool allowFocusedWindow) { ShowNotification(msg, beep, null, allowFocusedWindow); }

		public static void ShowNotification(string msg, bool beep, EditorWindow editorWindow, bool allowFocusedWindow)
		{
			if (!editorWindow)
				editorWindow = GetBestEditorWindowToShowNotification(allowFocusedWindow);

			if (!editorWindow)
				return;

			try { editorWindow.ShowNotification(new GUIContent(msg)); } catch { }

			if (beep)
				try { EditorApplication.Beep(); } catch { }
		}

		public static void ShowCouldNotExecuteCommandNotification(EditorWindow editorWindow) { ShowNotification("Invalid state. Check console for details", true, editorWindow); }

		public static bool IsSubclassOfRawGeneric(Type baseType, Type derivedType)
		{
			while (derivedType != null && derivedType != typeof(object))
			{
				var currentType = derivedType.IsGenericType ? derivedType.GetGenericTypeDefinition() : derivedType;
				if (baseType == currentType)
					return true;

				derivedType = derivedType.BaseType;
			}
			return false;
		}

		public static bool IsSubclassOfOSA(Type derivedType) { return IsSubclassOfRawGeneric(typeof(OSA<,>), derivedType); }

		public static void ReplaceTemplateDefaultNamespaceWithUnique(ref string template)
		{
			//template = template.Replace(TEMPLATE_DEFAULT_NAMESPACE, TEMPLATE_DEFAULT_NAMESPACE + DateTime.Now.ToString("yyMMMddhhmmssfff", CultureInfo.InvariantCulture));
			template = template.Replace(TEMPLATE_DEFAULT_NAMESPACE, TEMPLATE_DEFAULT_NAMESPACE + ".UniqueStringHereToAvoidNamespaceConflicts");
		}

		public static class TV
		{
			public const string TABLE_ADAPTER_INTERFACE_FULL_NAME = "Com.TheFallenGames.OSA.CustomAdapters.TableView.ITableAdapter";
			public const string IMPLEMENTATION_TEMPLATE_NAME = "BasicTableAdapter";
			public const string IMPLEMENTATION_TEMPLATE_NAME_WITH_EXTENSION = IMPLEMENTATION_TEMPLATE_NAME + ".txt";
			public const string TABLE_VIEW_NAME = "TableView";
			public const string DIR_RESPATH = TEMPLATE_SCROLLVIEW_PREFABS_DIR_RESPATH + "/" + TABLE_VIEW_NAME;

			public const string TUPLE_PREFAB_SIMPLE_NAME = "TuplePrefab";
			public const string TUPLE_VALUE_PREFAB_SIMPLE_NAME = "TupleValuePrefab";
			public const string COLUMNS_TUPLE_PREFAB_SIMPLE_NAME = "ColumnsTuplePrefab";
			public const string COLUMNS_TUPLE_VALUE_PREFAB_SIMPLE_NAME = "ColumnsTupleValuePrefab";

			public const string FLOATING_DROPDOWN_SIMPLE_NAME = "TVFloatingDropdown";
			public const string FLOATING_TEXT_INPUT_CONTROLLER_SIMPLE_NAME = "TVTextInputController";

#if OSA_TV_TMPRO
			public static TemplatePaths Paths = new TemplatePaths(DIR_RESPATH, true)
			{

			};
#else
			public static TemplatePaths Paths = new TemplatePaths(DIR_RESPATH, false)
			{

			};
#endif


			public class TemplatePaths
			{
				public readonly string SCROLL_VIEW;
				public readonly string IMPLEMENTATION_TEMPLATE_FILE;

				public readonly string TUPLE_PREFAB;
				public readonly string TUPLE_VALUE_PREFAB;
				public readonly string COLUMNS_TUPLE_PREFAB;
				public readonly string COLUMNS_TUPLE_VALUE_PREFAB;

				public readonly string INPUT__FLOATING_DROPDOWN;
				public readonly string INPUT__FLOATING_TEXT;

				string BASE_PATH;


				public TemplatePaths(string basePath, bool isTMPro)
				{
					BASE_PATH = basePath;
					string itemPrefabsSubPathOriginal = "ItemPrefabs";
					string itemPrefabsSubPath = itemPrefabsSubPathOriginal;
					string inputPrefabsSubPath = "Input";
					string prefabsSuffix = "";

					if (isTMPro)
					{
						prefabsSuffix = "-TMPro";
						itemPrefabsSubPath += "/TMPro";
						inputPrefabsSubPath += "/TMPro";
					}

					SCROLL_VIEW						= BASE_PATH + "/" + TABLE_VIEW_NAME;
					IMPLEMENTATION_TEMPLATE_FILE	= TEMPLATE_SCRIPTS_RESPATH + "/" + IMPLEMENTATION_TEMPLATE_NAME;

					TUPLE_PREFAB					= BASE_PATH + "/" + itemPrefabsSubPath + "/" + TUPLE_PREFAB_SIMPLE_NAME + prefabsSuffix;
					TUPLE_VALUE_PREFAB				= BASE_PATH + "/" + itemPrefabsSubPath + "/" + TUPLE_VALUE_PREFAB_SIMPLE_NAME + prefabsSuffix;
					// COLUMNS_PREFAB is the same for tmpro and non-tmpro setups
					COLUMNS_TUPLE_PREFAB					= BASE_PATH + "/" + itemPrefabsSubPathOriginal + "/" + COLUMNS_TUPLE_PREFAB_SIMPLE_NAME;
					COLUMNS_TUPLE_VALUE_PREFAB			= BASE_PATH + "/" + itemPrefabsSubPath + "/" + COLUMNS_TUPLE_VALUE_PREFAB_SIMPLE_NAME + prefabsSuffix;

					INPUT__FLOATING_DROPDOWN		= BASE_PATH + "/" + inputPrefabsSubPath + "/" + FLOATING_DROPDOWN_SIMPLE_NAME + prefabsSuffix;
					INPUT__FLOATING_TEXT			= BASE_PATH + "/" + inputPrefabsSubPath + "/" + FLOATING_TEXT_INPUT_CONTROLLER_SIMPLE_NAME + prefabsSuffix;
				}
			}
		}

		public static void GetAvailableOSAImplementations(List<Type> list, bool excludeExampleImplementations, Type implementedInterface = null, Type withoutImplementedInterface = null)
		{
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
					if (type.IsNested)
						continue;
					if (type.IsNotPublic)
						continue;
					if (!CWiz.IsSubclassOfOSA(type))
						continue;
					if (excludeExampleImplementations
						&& (
							type.Name.ToLower().Contains("example")
							////|| type.Name == typeof(Demos.SimpleExample.SimpleTutorial).Name
							//|| type.Name == "SimpleExample"
							//|| type.Name == typeof(CustomAdapters.DateTimePicker.DateTimePickerAdapter).Name
							|| type.Name == "DateTimePickerAdapter"
						))
						continue;

					// Excluding TableView's base classes, which will be used automatically
					if (type.Name == "BasicHeaderTupleAdapter"
						|| type.Name == "BasicTupleAdapter")
						continue;

					if (implementedInterface != null)
					{
						if (!OSAUtil.DotNETCoreCompat_IsAssignableFrom(implementedInterface, type))
							continue;
					}
					if (withoutImplementedInterface != null)
					{
						if (OSAUtil.DotNETCoreCompat_IsAssignableFrom(withoutImplementedInterface, type))
							continue;
					}

					list.Add(type);
				}
			}
		}

		public static Type GetTypeFromAllAssemblies(string typeFullName)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var type = assembly.GetType(typeFullName);
				if (type != null)
					return type;
			}

			return null;
		}
	}
}
