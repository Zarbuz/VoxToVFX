using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using frame8.Logic.Misc.Other;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI;

namespace Com.TheFallenGames.OSA.Core
{
	[Serializable]
	public class OSAException : SystemException
	{
		public OSAException() : base()
		{

		}

		public OSAException(string message) : base(message)
		{

		}

		public OSAException(string message, Exception innerException) : base(message, innerException)
		{

		}
	}
}
