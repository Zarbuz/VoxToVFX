using System.Linq;

namespace VoxToVFXFramework.Scripts.Utils.Extensions
{
	public static class ExtensionsString
	{
		public static bool HasSpecialChars(this string yourString)
		{
			return yourString.Any(ch => !char.IsLetterOrDigit(ch));
		}

		public static string FormatEthAddress(this string address, int count)
		{
			return address.Substring(0, count) + "..." + address.Substring(address.Length - count);
		}
	}
}
