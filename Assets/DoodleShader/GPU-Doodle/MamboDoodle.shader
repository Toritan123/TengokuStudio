Shader "MamboDoodle"
{
	Properties
	{
		_AddColor("Add", Color) = (0, 0, 0, 1)
		_MainTex("Texture", 2D) = "white" {}

		_DoodleMaxOffset("Doodle Max Offset", vector) = (0.005, 0.005, 0, 0)
		_DoodleFrameTime("Doodle Frame Time", Float) = 0.2
		_DoodleFrameCount("Doodle Frame Count", Int) = 24
		_DoodleNoiseScale("Doodle Noise Scale", vector) = (35, 35, 1, 1)

		_HueShift("Hue Shift", Range(0, 1)) = 0
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Off

		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "UtilsCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4 _Color;
			fixed4 _AddColor;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}

			float2 _DoodleMaxOffset;
			float _DoodleFrameTime;
			int _DoodleFrameCount;
			float2 _DoodleNoiseScale;

			float _HueShift;

			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
				float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
				float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
				float d = q.x - min(q.w, q.y);
				float e = 1e-10;
				return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			float3 HSVToRGB(float3 c)
			{
				float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
				float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 offset = DoodleTextureOffset(
					i.uv,
					_DoodleMaxOffset,
					_Time.y,
					_DoodleFrameTime,
					_DoodleFrameCount,
					_DoodleNoiseScale
				);

				fixed4 col = tex2D(_MainTex, i.uv + offset);

				// hue shift
				float3 hsv = RGBToHSV(col.rgb);
				hsv.x = frac(hsv.x + _HueShift);
				col.rgb = HSVToRGB(hsv);

				// sprite color
				col *= i.color;

				// add color
				col.rgb += _AddColor.rgb;

				return col;
			}
			ENDCG
		}
	}
}