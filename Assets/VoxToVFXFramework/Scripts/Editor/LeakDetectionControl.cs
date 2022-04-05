using Unity.Collections;
using UnityEditor;

public static class LeakDetectionControl
{
	[MenuItem("Tools/Jobs/Leak Detection")]
	private static void LeakDetection()
	{
		NativeLeakDetection.Mode = NativeLeakDetectionMode.Enabled;
	}

	[MenuItem("Tools/Jobs/Leak Detection With Stack Trace")]
	private static void LeakDetectionWithStackTrace()
	{
		NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
	}

	[MenuItem("Tools/Jobs/No Leak Detection")]
	private static void NoLeakDetection()
	{
		NativeLeakDetection.Mode = NativeLeakDetectionMode.Disabled;
	}
}