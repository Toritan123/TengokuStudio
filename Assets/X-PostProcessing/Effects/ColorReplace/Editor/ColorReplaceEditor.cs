
//----------------------------------------------------------------------------------------------------------
// X-PostProcessing Library
// https://github.com/QianMo/X-PostProcessing-Library
// Copyright (C) 2020 QianMo. All rights reserved.
// Licensed under the MIT License 
// You may not use this file except in compliance with the License.You may obtain a copy of the License at
// http://opensource.org/licenses/MIT
//----------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using UnityEditor.Rendering.PostProcessing;
using UnityEngine.Rendering.PostProcessing;

namespace XPostProcessing
{
    [PostProcessEditor(typeof(ColorReplace))]
    public sealed class ColorReplaceEditor : PostProcessEffectEditor<ColorReplace>
    {

        SerializedParameterOverride FromColor1;
        SerializedParameterOverride ToColor1;
        SerializedParameterOverride Range1;
        SerializedParameterOverride Fuzziness1;

        SerializedParameterOverride FromColor2;
        SerializedParameterOverride ToColor2;
        SerializedParameterOverride Range2;
        SerializedParameterOverride Fuzziness2;

        SerializedParameterOverride FromColor3;
        SerializedParameterOverride ToColor3;
        SerializedParameterOverride Range3;
        SerializedParameterOverride Fuzziness3;

        SerializedParameterOverride FromColor4;
        SerializedParameterOverride ToColor4;
        SerializedParameterOverride Range4;
        SerializedParameterOverride Fuzziness4;

        SerializedParameterOverride FromColor5;
        SerializedParameterOverride ToColor5;
        SerializedParameterOverride Range5;
        SerializedParameterOverride Fuzziness5;


        public override void OnEnable()
        {
            FromColor1 = FindParameterOverride(x => x.FromColor1);
            ToColor1   = FindParameterOverride(x => x.ToColor1);
            Range1     = FindParameterOverride(x => x.Range1);
            Fuzziness1 = FindParameterOverride(x => x.Fuzziness1);

            FromColor2 = FindParameterOverride(x => x.FromColor2);
            ToColor2   = FindParameterOverride(x => x.ToColor2);
            Range2     = FindParameterOverride(x => x.Range2);
            Fuzziness2 = FindParameterOverride(x => x.Fuzziness2);

            FromColor3 = FindParameterOverride(x => x.FromColor3);
            ToColor3   = FindParameterOverride(x => x.ToColor3);
            Range3     = FindParameterOverride(x => x.Range3);
            Fuzziness3 = FindParameterOverride(x => x.Fuzziness3);

            FromColor4 = FindParameterOverride(x => x.FromColor4);
            ToColor4   = FindParameterOverride(x => x.ToColor4);
            Range4     = FindParameterOverride(x => x.Range4);
            Fuzziness4 = FindParameterOverride(x => x.Fuzziness4);

            FromColor5 = FindParameterOverride(x => x.FromColor5);
            ToColor5   = FindParameterOverride(x => x.ToColor5);
            Range5     = FindParameterOverride(x => x.Range5);
            Fuzziness5 = FindParameterOverride(x => x.Fuzziness5);
        }
        public override string GetDisplayTitle()
        {
            return XPostProcessingEditorUtility.DISPLAY_TITLE_PREFIX + base.GetDisplayTitle();
        }


        public override void OnInspectorGUI()
        {
            AddSlot("Slot 1", FromColor1, ToColor1, Range1, Fuzziness1);
            AddSlot("Slot 2", FromColor2, ToColor2, Range2, Fuzziness2);
            AddSlot("Slot 3", FromColor3, ToColor3, Range3, Fuzziness3);
            AddSlot("Slot 4", FromColor4, ToColor4, Range4, Fuzziness4);
            AddSlot("Slot 5", FromColor5, ToColor5, Range5, Fuzziness5);
        }

        private void AddSlot(
            string title,
            SerializedParameterOverride from,
            SerializedParameterOverride to,
            SerializedParameterOverride range,
            SerializedParameterOverride fuzziness)
        {
            EditorUtilities.DrawHeaderLabel(title);

            EditorUtilities.DrawHeaderLabel("From-To Color");
            PropertyField(from);
            PropertyField(to);

            EditorUtilities.DrawHeaderLabel("Color Precision");
            PropertyField(range);
            PropertyField(fuzziness);
        }

    }
}
        
