Shader "Custom/BuiltToScale/Grid"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}

        _Color ("Color", Color) = (1,1,1,1)
        _EndColor ("End Color", Color) = (0,0,0,1)

        [Toggle]
        _UseCustomEndColor ("Use Custom End Color", Float) = 0

        _Lighting ("Lighting", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

        sampler2D _MainTex;

        fixed4 _Color;
        fixed4 _EndColor;

        float _UseCustomEndColor;
        float _Lighting;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex;
            fixed4 tex = tex2D(_MainTex, uv);
            
            // Get the auto darkened base color
            fixed4 autoShade = _Color * .65;

            // Get the world y position
            float y = IN.worldPos.y;

            // The grid blocks go from 0 to .96 in world space on the y-axis, and there's 4 grid blocks in height
            // I'm using slightly less than .96 here so that grid spaces on top get the brightest color
            float stepSize = 0.955 / 4;

            // Get the stepped gradient
            float gradient = 1.0 - (floor(y / stepSize) * stepSize);

            fixed4 autoGradient = lerp(_Color, autoShade, gradient);
            fixed4 customGradient = lerp(_Color, _EndColor, gradient);

            // Blend between the auto gradient and the custom gradient based on the boolean
            fixed4 finalColor = lerp(autoGradient, customGradient, _UseCustomEndColor);
            // Override the tint color with white based on the desired amount of lighting
            finalColor = lerp(finalColor, 1, _Lighting);

            // Multiply the base texture by the final color
            fixed4 texturedColor = tex * finalColor;

            o.Albedo = texturedColor.rgb;
            o.Alpha  = texturedColor.a;

            o.Metallic   = 0.0;
            o.Smoothness = 0.5;
        }

        ENDCG
    }

    FallBack "Diffuse"
}