using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using frame8.Logic.Misc.Visual.UI.MonoBehaviours;
using Com.TheFallenGames.OSA.Core;

namespace Com.TheFallenGames.OSA.Editor.OSAWizard.CustomAdapterConfigurators
{
	public interface ICustomAdapterConfigurator
	{
		void ConfigureNewAdapter(IOSA newAdapter);
	}
}
