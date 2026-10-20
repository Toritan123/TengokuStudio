Shader "Custom/SpecularRGBAmbient"
{
    Properties
    {
        _ColorMask ("Color Mask (RGB)", 2D) = "white" {}
        _RedColor ("Red Channel Color", Color) = (1,0,0,1)
        _GreenColor ("Green Channel Color", Color) = (0,1,0,1)
        _BlueColor ("Blue Channel Color", Color) = (0,0,1,1)
        _AmbientColor ("Ambient/Shadow Color", Color) = (0.2, 0.2, 0.2, 1)

        _SpecularTexture ("Specular Texture", 2D) = "white" {}
        _SpecularColor ("Specular Tint", Color) = (1,1,1,1)
        _Glossiness ("Glossiness", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf SimpleBlinnPhong addshadow

        sampler2D _ColorMask;
        fixed4 _RedColor;
        fixed4 _GreenColor;
        fixed4 _BlueColor;
        fixed4 _AmbientColor;

        sampler2D _SpecularTexture;
        fixed4 _SpecularColor;
        half _Glossiness;

        half4 LightingSimpleBlinnPhong (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            half NdotL = dot(s.Normal, lightDir);
            half3 diffuse = s.Albedo * _LightColor0.rgb * saturate(NdotL);

            half3 halfDir = normalize(lightDir + viewDir);
            half NdotH = dot(s.Normal, halfDir);
            float specPower = exp2(s.Gloss * 10.0 + 1.0);
            half3 specular = s.Specular * pow(NdotH, specPower) * _LightColor0.rgb;
            
            half4 c;
            c.rgb = (s.Albedo * _AmbientColor.rgb) + (diffuse + specular) * atten;
            c.a = s.Alpha;
            return c;
        }

        struct Input
        {
            float2 uv_ColorMask;
            float2 uv_SpecularTexture;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 mask = tex2D(_ColorMask, IN.uv_ColorMask);

            fixed3 albedo = mask.r * _RedColor.rgb + mask.g * _GreenColor.rgb + mask.b * _BlueColor.rgb;
            
            o.Albedo = albedo;
            o.Specular = tex2D(_SpecularTexture, IN.uv_SpecularTexture).rgb * _SpecularColor.rgb;
            o.Gloss = _Glossiness;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}