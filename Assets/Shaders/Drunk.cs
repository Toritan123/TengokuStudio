using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace PPU
{
	[Serializable]
	[PostProcess(typeof(DrunkRenderer), PostProcessEvent.AfterStack, "Custom/Drunk")]
	public sealed class Drunk : PostProcessEffectSettings
	{
		[Range(0.0f, 10.0f)]
        public FloatParameter Horizontal = new FloatParameter { value = 0f };
		[Range(0.0f, 10.0f)]
        public FloatParameter Vertical = new FloatParameter { value = 0f };
		[Range(0.0f, 10.0f)]
        public FloatParameter Speed = new FloatParameter { value = 0f };
	}

	public sealed class DrunkRenderer : PostProcessEffectRenderer<Drunk>
	{
		public override void Render(PostProcessRenderContext context)
		{
			var sheet = context.propertySheets.Get(Shader.Find("Custom/Drunk"));
			sheet.properties.SetFloat("_Horizontal", settings.Horizontal);
			sheet.properties.SetFloat("_Vertical", settings.Vertical);
			sheet.properties.SetFloat("_Speed", settings.Speed);
			context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
		}
	}
}