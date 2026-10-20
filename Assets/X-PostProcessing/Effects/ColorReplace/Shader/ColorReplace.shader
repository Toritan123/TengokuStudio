
//----------------------------------------------------------------------------------------------------------
// X-PostProcessing Library
// https://github.com/QianMo/X-PostProcessing-Library
// Copyright (C) 2020 QianMo. All rights reserved.
// Licensed under the MIT License 
// You may not use this file except in compliance with the License.You may obtain a copy of the License at
// http://opensource.org/licenses/MIT
//----------------------------------------------------------------------------------------------------------

Shader "Hidden/X-PostProcessing/ColorReplace"
{
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment Frag

			#include "../../../Shaders/StdLib.hlsl"
			#include "../../../Shaders/XPostProcessing.hlsl"

            uniform half4 _FromColor1;
            uniform half4 _ToColor1;
            uniform half  _Range1;
            uniform half  _Fuzziness1;

            uniform half4 _FromColor2;
            uniform half4 _ToColor2;
            uniform half  _Range2;
            uniform half  _Fuzziness2;

            uniform half4 _FromColor3;
            uniform half4 _ToColor3;
            uniform half  _Range3;
            uniform half  _Fuzziness3;

            uniform half4 _FromColor4;
            uniform half4 _ToColor4;
            uniform half  _Range4;
            uniform half  _Fuzziness4;

            uniform half4 _FromColor5;
            uniform half4 _ToColor5;
            uniform half  _Range5;
            uniform half  _Fuzziness5;

            half3 ColorReplace(half3 In, half3 From, half3 To, half Range, half Fuzziness)
            {
                half Distance = distance(From, In);
                return lerp(To, In, saturate((Distance - Range) / max(Fuzziness, 0.1)));
            }

            half4 Frag(VaryingsDefault i) : SV_Target
            {
                half4 sceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
                half3 col = sceneColor.rgb;

                col = ColorReplace(col, _FromColor1.rgb, _ToColor1.rgb, _Range1, _Fuzziness1);
                col = ColorReplace(col, _FromColor2.rgb, _ToColor2.rgb, _Range2, _Fuzziness2);
                col = ColorReplace(col, _FromColor3.rgb, _ToColor3.rgb, _Range3, _Fuzziness3);
                col = ColorReplace(col, _FromColor4.rgb, _ToColor4.rgb, _Range4, _Fuzziness4);
                col = ColorReplace(col, _FromColor5.rgb, _ToColor5.rgb, _Range5, _Fuzziness5);

                return half4(col, 1.0);
            }

			ENDHLSL
		}
	}
}
        
