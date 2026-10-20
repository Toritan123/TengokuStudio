Shader "SpecularLUT"
{
    Properties
    {
        _Albedo("Albedo", 2D) = "white" {}
        _AlbedoColor("Albedo Color", Color) = (1,1,1,1)
        
        _SpecularColor("Specular Color", Color) = (1,1,1,1)
        _SpecularPower("Specular Power/Tightness", Range(1, 256)) = 32

        _RimLUT("Rim Light LUT", 2D) = "white" {}
        _RimColor("Rim Color (Tint)", Color) = (1,1,1,1)
        _RimPower("Rim Power/Thickness", Range(0.01, 5)) = 5.0
        
        _FresnelColor("Fresnel Color", Color) = (0,0,0,1) 
        _FresnelPower("Fresnel Power/Thickness", Range(0.1, 50)) = 5.0

        _EmissionTexture("Emission Texture", 2D) = "white" {}
        _Emission("Emission", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM
        #pragma surface surf Lambert finalcolor:FinalColorPass fullforwardshadows
        #pragma target 3.0

        sampler2D _Albedo;
        fixed4 _AlbedoColor;

        fixed4 _SpecularColor;
        half _SpecularPower;

        sampler2D _RimLUT;
        fixed4 _RimColor;
        half _RimPower;
        
        fixed4 _FresnelColor;
        half _FresnelPower;

        sampler2D _EmissionTexture;
        fixed4 _Emission;

        struct Input
        {
            float2 uv_Albedo;
            float2 uv_EmissionTexture;
            float3 worldNormal;
            float3 viewDir;
        };
        
        void FinalColorPass (Input IN, SurfaceOutput o, inout fixed4 color)
        {
            half3 viewDir = normalize(IN.viewDir);
            half3 worldNormal = normalize(IN.worldNormal);
            half3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

            // lut 
            half NdotV = dot(worldNormal, viewDir);
            half rimAmount = 1.0 - saturate(NdotV);
            half rimCoord = pow(rimAmount, _RimPower);
            half3 rimFromLUT = tex2D(_RimLUT, float2(rimCoord, 0.5)).rgb;
            
            // fresnel
            half fresnelAmount = pow(rimAmount, _FresnelPower);
            half3 finalFresnel = _FresnelColor.rgb * fresnelAmount;
            
            // specular
            half3 halfVector = normalize(lightDir + viewDir);
            half NdotH = dot(worldNormal, halfVector);
            half specularAmount = pow(saturate(NdotH), _SpecularPower);
            half NdotL = saturate(dot(worldNormal, lightDir));
            half3 finalSpecular = _SpecularColor.rgb * specularAmount * NdotL * _LightColor0.rgb;

            color.rgb += finalSpecular;         
            color.rgb += rimFromLUT * _RimColor.rgb; 
            color.rgb += finalFresnel;          
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_Albedo, IN.uv_Albedo) * _AlbedoColor;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            o.Emission = tex2D(_EmissionTexture, IN.uv_EmissionTexture).rgb * _Emission.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}