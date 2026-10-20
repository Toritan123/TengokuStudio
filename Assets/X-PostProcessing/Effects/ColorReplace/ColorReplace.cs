
//----------------------------------------------------------------------------------------------------------
// X-PostProcessing Library
// https://github.com/QianMo/X-PostProcessing-Library
// Copyright (C) 2020 QianMo. All rights reserved.
// Licensed under the MIT License 
// You may not use this file except in compliance with the License.You may obtain a copy of the License at
// http://opensource.org/licenses/MIT
//----------------------------------------------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;


namespace XPostProcessing
{

    [Serializable]
    [PostProcess(typeof(ColorReplaceRenderer), PostProcessEvent.AfterStack, "X-PostProcessing/ColorAdjustment/ColorReplace")]
    public class ColorReplace : PostProcessEffectSettings
    {
        [Header("Slot 1")]
        [ColorUsage(true, true)]
        public ColorParameter FromColor1 = new ColorParameter { value = new Color(0.8f, 0.0f, 0.0f, 1) };

        [ColorUsage(true, true)]
        public ColorParameter ToColor1 = new ColorParameter { value = new Color(0.0f, 0.8f, 0.0f, 1) };
        [Range(0.0f, 1.0f)]
        public FloatParameter Range1 = new FloatParameter { value = 0.2f };

        [Range(0.0f, 1.0f)]
        public FloatParameter Fuzziness1 = new FloatParameter { value = 0.5f };

        [Header("Slot 2")]
        [ColorUsage(true, true)]
        public ColorParameter FromColor2 = new ColorParameter { value = new Color(0.8f, 0.0f, 0.0f, 1) };

        [ColorUsage(true, true)]
        public ColorParameter ToColor2 = new ColorParameter { value = new Color(0.0f, 0.8f, 0.0f, 1) };
        [Range(0.0f, 1.0f)]
        public FloatParameter Range2 = new FloatParameter { value = 0.2f };

        [Range(0.0f, 1.0f)]
        public FloatParameter Fuzziness2 = new FloatParameter { value = 0.5f };

        [Header("Slot 3")]
        [ColorUsage(true, true)]
        public ColorParameter FromColor3 = new ColorParameter { value = new Color(0.8f, 0.0f, 0.0f, 1) };

        [ColorUsage(true, true)]
        public ColorParameter ToColor3 = new ColorParameter { value = new Color(0.0f, 0.8f, 0.0f, 1) };
        [Range(0.0f, 1.0f)]
        public FloatParameter Range3 = new FloatParameter { value = 0.2f };

        [Range(0.0f, 1.0f)]
        public FloatParameter Fuzziness3 = new FloatParameter { value = 0.5f };

        [Header("Slot 4")]
        [ColorUsage(true, true)]
        public ColorParameter FromColor4 = new ColorParameter { value = new Color(0.8f, 0.0f, 0.0f, 1) };

        [ColorUsage(true, true)]
        public ColorParameter ToColor4 = new ColorParameter { value = new Color(0.0f, 0.8f, 0.0f, 1) };
        [Range(0.0f, 1.0f)]
        public FloatParameter Range4 = new FloatParameter { value = 0.2f };

        [Range(0.0f, 1.0f)]
        public FloatParameter Fuzziness4 = new FloatParameter { value = 0.5f };

        [Header("Slot 5")]
        [ColorUsage(true, true)]
        public ColorParameter FromColor5 = new ColorParameter { value = new Color(0.8f, 0.0f, 0.0f, 1) };

        [ColorUsage(true, true)]
        public ColorParameter ToColor5 = new ColorParameter { value = new Color(0.0f, 0.8f, 0.0f, 1) };
        [Range(0.0f, 1.0f)]
        public FloatParameter Range5 = new FloatParameter { value = 0.2f };

        [Range(0.0f, 1.0f)]
        public FloatParameter Fuzziness5 = new FloatParameter { value = 0.5f };
    }

    public sealed class ColorReplaceRenderer : PostProcessEffectRenderer<ColorReplace>
    {
        private Shader shader;
        private const string PROFILER_TAG = "X-ColorReplace";

        public override void Init()
        {
            shader = Shader.Find("Hidden/X-PostProcessing/ColorReplace");
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            CommandBuffer cmd = context.command;
            PropertySheet sheet = context.propertySheets.Get(shader);
            cmd.BeginSample(PROFILER_TAG);

            sheet.properties.SetColor("_FromColor1", settings.FromColor1);
            sheet.properties.SetColor("_ToColor1", settings.ToColor1);
            sheet.properties.SetFloat("_Range1", settings.Range1);
            sheet.properties.SetFloat("_Fuzziness1", settings.Fuzziness1);

            // this is funny
            sheet.properties.SetColor("_FromColor2", settings.FromColor2);
            sheet.properties.SetColor("_ToColor2", settings.ToColor2);
            sheet.properties.SetFloat("_Range2", settings.Range2);
            sheet.properties.SetFloat("_Fuzziness2", settings.Fuzziness2);

            sheet.properties.SetColor("_FromColor3", settings.FromColor3);
            sheet.properties.SetColor("_ToColor3", settings.ToColor3);
            sheet.properties.SetFloat("_Range3", settings.Range3);
            sheet.properties.SetFloat("_Fuzziness3", settings.Fuzziness3);

            sheet.properties.SetColor("_FromColor4", settings.FromColor4);
            sheet.properties.SetColor("_ToColor4", settings.ToColor4);
            sheet.properties.SetFloat("_Range4", settings.Range4);
            sheet.properties.SetFloat("_Fuzziness4", settings.Fuzziness4);

            sheet.properties.SetColor("_FromColor5", settings.FromColor5);
            sheet.properties.SetColor("_ToColor5", settings.ToColor5);
            sheet.properties.SetFloat("_Range5", settings.Range5);
            sheet.properties.SetFloat("_Fuzziness5", settings.Fuzziness5);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
            cmd.EndSample(PROFILER_TAG);
        }
    }
}
        
