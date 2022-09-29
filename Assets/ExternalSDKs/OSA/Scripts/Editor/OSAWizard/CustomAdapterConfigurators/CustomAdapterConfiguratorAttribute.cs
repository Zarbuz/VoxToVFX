using System;

namespace Com.TheFallenGames.OSA.Editor.OSAWizard.CustomAdapterConfigurators
{
	public class CustomAdapterConfiguratorAttribute : Attribute
	{
		public readonly Type ConfiguredType;


		public CustomAdapterConfiguratorAttribute(Type configuredType)
		{
			ConfiguredType = configuredType;
		}
	}
}
