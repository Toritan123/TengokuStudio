//{"Values":["0","Custom/Drunk","_MainTex","0",""]}|{"position":{"serializedVersion":"2","x":0.0,"y":0.0,"width":212.0,"height":89.0},"name":"Float","selected":false,"Values":["Horizontal","Horizontal wobble","1"],"serial":0,"unique":-1,"type":"FloatField"}|{"position":{"serializedVersion":"2","x":0.0,"y":106.0,"width":212.0,"height":89.0},"name":"Float","selected":false,"Values":["Vertical","Vertical wobble","1"],"serial":1,"unique":843,"type":"FloatField"}|{"position":{"serializedVersion":"2","x":0.0,"y":212.0,"width":212.0,"height":89.0},"name":"Float","selected":false,"Values":["Speed","Animation speed","1"],"serial":2,"unique":1106,"type":"FloatField"}|{"tempTextures":0,"passes":[{"position":{"serializedVersion":"2","x":0.0,"y":36.0,"width":212.0,"height":16.0},"InputLabels":["Game"],"OutputLabels":["Screen"],"PassLabels":["0"],"VariableLabels":["None"],"Input":0,"Output":0,"Pass":0,"Iterations":1,"Variable":0}],"passOptions":["0"],"inputOptions":["Game"],"outputOptions":["Screen"],"variableOptions":["None"]}
//\	CameraOutput\	3809.227\	52.09741\	192\	215\		False\			null\			null\		False\			null\			null\		False\			null\			null\		True\			1\			3\	CameraInput\	3419.223\	34.09717\	192\	335\		False\			null\			null\		False\			null\			null\		False\			null\			null\		True\			0\			3\		True\			5\			0\		True\			6\			0\		False\			null\			null\	StereoUV\	502.5867\	391.5018\	192\	175\		True\			22\			1\		True\			21\			1\		False\			null\			null\	Sin\	2208.179\	349.9066\	192\	135\		True\			8\			1\		True\			13\			0\	Sin\	1993.229\	725.3358\	192\	135\		True\			7\			1\		True\			15\			0\	Add\	3057.379\	24.55737\	192\	215\		/4\		True\			1\			4\		True\			2\			0\		True\			8\			0\		False\			null\			null\	Add\	3132.378\	707.0579\	192\	215\		/4\		True\			1\			5\		True\			2\			1\		True\			7\			0\		False\			null\			null\	Mul\	2640.637\	802.2978\	192\	215\		/4\		True\			6\			2\		True\			4\			0\		True\			16\			0\		False\			null\			null\	Mul\	2637.463\	340.708\	192\	215\		/4\		True\			5\			2\		True\			3\			0\		True\			16\			0\		False\			null\			null\	Mul\	1908.799\	1427.008\	192\	215\		/4\		True\			21\			2\		True\			20\			0\		True\			17\			0\		False\			null\			null\	_Float\	672.2556\	1370.355\	192\	95\		/Speed\		/3\		/1106\		True\			19\			2\	_Float\	2205.944\	537.4974\	192\	95\		/Horizontal\		/1\		/-1\		True\			15\			3\	_Float\	2240.229\	911.7833\	192\	95\		/Vertical\		/2\		/843\		True\			13\			3\	Mul\	1900.15\	273.3306\	192\	215\		/4\		True\			3\			1\		True\			21\			0\		True\			14\			0\		True\			12\			0\	Value1\	1341.835\	672.8543\	192\	95\		/10.0\		True\			15\			2\	Mul\	1792.429\	898.9183\	192\	255\		/5\		True\			4\			1\		True\			22\			0\		True\			14\			0\		True\			11\			0\		False\			null\			null\	Value1\	2336.34\	1363.093\	192\	95\		/0.01\		True\			8\			2\	Value1\	1023.842\	1463.094\	192\	95\		/0.1\		True\			9\			2\	Time\	594.573\	974.7625\	192\	255\		False\			null\			null\		True\			19\			1\		False\			null\			null\		False\			null\			null\		False\			null\			null\	Mul\	1009.612\	1177.183\	192\	215\		/4\		True\			20\			1\		True\			18\			1\		True\			10\			0\		False\			null\			null\	Add\	1515.762\	1260.715\	192\	175\		/3\		True\			9\			1\		True\			19\			0\		False\			null\			null\	Add\	1178.779\	423.6111\	192\	215\		/4\		True\			13\			1\		True\			2\			1\		True\			9\			0\		False\			null\			null\	Add\	981.6356\	695.0396\	192\	215\		/4\		True\			15\			1\		True\			2\			0\		True\			9\			0\		False\			null\			null

Shader "Custom/Drunk" {

	SubShader {
		Cull Off ZWrite Off ZTest Always

		Pass {
			HLSLPROGRAM
			#pragma vertex VertDefault
			#pragma fragment Frag

			#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

			TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
			uniform half _Horizontal;
			uniform half _Vertical;
			uniform half _Speed;

			half4 Frag (VaryingsDefault i) : SV_Target {
				half4 CameraOutput = 0.0;
				CameraOutput.rgb = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, half2((i.texcoordStereo.x + (sin(((i.texcoordStereo.y + (((_Time.y * _Speed)) * 0.1)) * 10.0 * _Vertical)) * 0.01)),(i.texcoordStereo.y + (sin(((i.texcoordStereo.x + (((_Time.y * _Speed)) * 0.1)) * 10.0 * _Horizontal)) * 0.01)))).rgb;
				return CameraOutput;
			}
			ENDHLSL
		}
	}
}