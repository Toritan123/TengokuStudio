using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using HeavenStudio.Util

namespace HeavenStudio.Common
{
	public class HintGiver : MonoBehaviour
	{
		public List<HintStruct> generalTips = new List<HintStruct>();
		public List<HintStruct> playTips = new List<HintStruct>();
		public List<HintStruct> editorTips = new List<HintStruct>();
		public List<HintStruct> sillyTips = new List<HintStruct>();
		public List<HintStruct> factTips = new List<HintStruct>();
		

		public string GenerateHint()
		{
			if (PersistentDataManager.gameSettings.showLoadingTips)
			{
				List<HintStruct> allTips = new();

				PersistentDataManager.gameSettings.enabledTips ??= new(){HintGiver.HintTypes.General};

				allTips.AddRange(generalTips);
				if (PersistentDataManager.gameSettings.enabledTips.Contains(HintTypes.Play)) allTips.AddRange(playTips);
				if (PersistentDataManager.gameSettings.enabledTips.Contains(HintTypes.Editor)) allTips.AddRange(editorTips);
				if (PersistentDataManager.gameSettings.enabledTips.Contains(HintTypes.Silly)) allTips.AddRange(sillyTips);
				if (PersistentDataManager.gameSettings.enabledTips.Contains(HintTypes.Fact)) allTips.AddRange(factTips);

				HintStruct selectedHint = null;

				while (selectedHint == null)
				{
					HintStruct potentialHint = allTips[UnityEngine.Random.Range(0, allTips.Count)];

					if (UnityEngine.Random.Range(0f, 1f) >= potentialHint.rerollChance)
					{
						selectedHint = potentialHint;
					}
				}

				return $"<size=75%><alpha=#55>{Localization.EnumName(selectedHint.type)}</size><alpha=#C8>\n{Localization.T(selectedHint.hint)}";

			}
			else
			{
				return "";
			}
		}

		[Serializable]
		public class HintStruct
		{
			public string hint;
			public float rerollChance = 1;
			public HintTypes type;
		}

		public enum HintTypes
		{
			General,
			Play,
			Editor,
			Silly,
			Fact
		}
	}
}