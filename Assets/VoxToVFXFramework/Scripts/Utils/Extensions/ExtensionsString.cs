using System.Linq;

namespace VoxToVFXFramework.Scripts.Utils.Extensions
{
	public static class ExtensionsString
	{
		public static bool HasSpecialChars(this string yourString)
		{
			return yourString.Any(ch => !char.IsLetterOrDigit(ch));
		}
	}
}
