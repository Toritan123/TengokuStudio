Shader "Custom/Opacity"
{
    Properties
    {
        _Opacity("Opacity", Range(0, 1)) = 1.0

        _Albedo("Albedo", 2D) = "white" {}
        _AlbedoColor("Albedo Color", Color) = (1,1,1,1)
        
        _Smoothness("Smoothness", Range(0, 1)) = 0.5

        _SpecularColor("Specular Color", Color) = (1,1,1,1)
        _SpecularPower("Specular Power/Tightness", Range(1, 256)) = 32

        _RimLUT("Rim Light LUT", 2D) = "white" {}
        _RimColor("Rim Color (Tint)", Color) = (1,1,1,1)
        _RimPower("Rim Power/Thickness", Range(0.01, 10)) = 1.0
        
        _FresnelColor("Fresnel Color", Color) = (0,0,0,1) 
        _FresnelPower("Fresnel Power/Thickness", Range(0.01, 10)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard alpha:fade

        sampler2D _Albedo;
        fixed4 _AlbedoColor;
        half _Opacity;
        half _Smoothness; 

        fixed4 _SpecularColor;
        half _SpecularPower;

        sampler2D _RimLUT;
        fixed4 _RimColor;
        half _RimPower;
        
        fixed4 _FresnelColor;
        half _FresnelPower;

        struct Input
        {
            float2 uv_Albedo;
            float3 worldNormal;
            float3 viewDir;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_Albedo, IN.uv_Albedo) * _AlbedoColor;
            o.Albedo = c.rgb;
            o.Alpha = c.a * _Opacity;

            half3 viewDir = normalize(IN.viewDir);
            half3 worldNormal = normalize(IN.worldNormal);
            half3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

            half3 halfVector = normalize(lightDir + viewDir);
            half NdotH = dot(worldNormal, halfVector);
            half specularAmount = pow(saturate(NdotH), _SpecularPower);
            half3 finalSpecular = _SpecularColor.rgb * specularAmount * _LightColor0.rgb;

            half rimAmount = 1.0 - saturate(dot(worldNormal, viewDir));
            
            half rimCoord = pow(rimAmount, _RimPower);
            half4 rimFromLUT = tex2D(_RimLUT, float2(rimCoord, 0.5));
            half3 finalLUTRim = lerp(half3(0,0,0), rimFromLUT.rgb * _RimColor.rgb, rimFromLUT.a);
            
            half fresnelAmount = pow(rimAmount, _FresnelPower);
            half3 finalFresnel = _FresnelColor.rgb * fresnelAmount;

            o.Emission = finalSpecular + finalLUTRim + finalFresnel;

            o.Metallic = 0.0;
            o.Smoothness = _Smoothness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}