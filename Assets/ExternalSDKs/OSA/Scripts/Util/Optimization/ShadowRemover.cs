// Pre-2019.1 Unity versions have performance problems with the Shadow script,
// while 2019.{1,2,3,4} sometimes have problems with displaying an Image+Shadow
#if UNITY_2019_1_OR_NEWER && !(UNITY_2019_1_0 || UNITY_2019_1_1 || UNITY_2019_1_2 || UNITY_2019_1_3 || UNITY_2019_1_4)
#define ALLOW_SHADOWS
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.TheFallenGames.OSA.Util.Optimization
{
	/// <summary>
	/// Simple: If no Unity 2019, no shadow (the shadow will be destroyed). 
	/// Seems like a lot of Shadow components used cause significant FPS drops
	/// </summary>
	[ExecuteInEditMode]
	public class ShadowRemover : MonoBehaviour
	{
		// If not on a compatible unity version, destroy the Shadow script and this script
#if ALLOW_SHADOWS

#else

#if UNITY_EDITOR
		// With shortcut Ctrl/Cmd + Shift + Alt + Z
		//[UnityEditor.MenuItem("TFG/OSA/Optimization/AddShadowRemoverToShadowContainingGameObjectsInOpenScene %#&z")]
		// No shortcut
		[UnityEditor.MenuItem("TFG/OSA/Optimization/AddShadowRemoverToShadowContainingGameObjectsInOpenScene")]
		static void AddShadowRemoverToShadowContainingGameObjectsInOpenScene()
		{
			var scene = SceneManager.GetActiveScene();
			int r = 0;
			foreach (var rootGO in scene.GetRootGameObjects())
				AddRemoverRec(rootGO.transform, ref r);
			Debug.Log("Scene " + scene.name + ": " + r + " Shadow objects found");
		}

		static void AddRemoverRec(Transform tr, ref int count)
		{
			if (tr.GetComponent<Shadow>() && !tr.GetComponent<ShadowRemover>())
			{
				tr.gameObject.AddComponent<ShadowRemover>();
				++count;
			}

			foreach (Transform ch in tr)
			{
				AddRemoverRec(ch, ref count);
			}
		}
#endif

		[SerializeField]
		[HideInInspector]
		Shadow _Shadow;


		void Awake()
		{
			if (!_Shadow)
				_Shadow = GetComponent<Shadow>();

			if (!Application.isPlaying)
				return;

			// In play mode, destroy it
			if (_Shadow)
			{
				Destroy(_Shadow);
				_Shadow = null;
			}
			Destroy(this);
		}

		// Odd thing happens in Unity 5.6.1: if we use Start instead, when instantiating this game object the items are presend and disabled, instead of non-existing
		//void Start()
		//{
		//	if (!_Shadow)
		//		_Shadow = GetComponent<Shadow>();

		//	if (!Application.isPlaying)
		//		return;

		//	// In play mode, destroy it
		//	if (_Shadow)
		//	{
		//		Destroy(_Shadow);
		//		_Shadow = null;
		//	}
		//	Destroy(this);
		//}

#if UNITY_EDITOR
		void Update()
		{
			// No checks during play mode
			if (Application.isPlaying)
				return;

			if (!_Shadow)
				_Shadow = GetComponent<Shadow>();
		}
#endif

#endif
	}
}