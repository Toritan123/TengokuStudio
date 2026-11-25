Shader "Custom/BuiltToScale/Divider"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)

        [Toggle] _UseDirectColor ("Use Direct Color", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "GetAutoOutlineColor.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            half _UseDirectColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return lerp(GetAutoOutlineColor(_Color), _Color, _UseDirectColor);
            }
            ENDCG
        }
    }
}
