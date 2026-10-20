Shader "Sprites/Cutout_LibBG_Waves"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _AddColor ("Screen", Color) = (0,0,0,0)

        _CanvasSize ("Wave Canvas Size", float) = (1632, 918, 0, 0)

        _AnimFrame ("Animation Frame #", float) = 0
        _TopWaveSpeed ("Top Wave Speed", float) = 1
        _BottomWaveSpeed ("Bottom Wave Speed", float) = 1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _AddColor;
            float2 _CanvasSize;
            float _AnimFrame;
            float _TopWaveSpeed;
            float _BottomWaveSpeed;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float2 shear(float2 coord, float shearPix, float centerY, float amp)
            {
                return float2(coord.x + (coord.y - centerY) * shearPix / amp, coord.y);
            }

            float irregWaveFunction(float n)
            {
                return sin(radians(n + 90.0 * sin(radians(0.3 * n))));
            }

            float calculateWave(float2 pathCoord, float centerY, float amp, float shearPix, float blurriness, float waveOffset, float time)
            {
                float2 shearedCoord = shear(pathCoord, shearPix, centerY, amp);
                float pulsingHalfWidth = amp * (0.35 + 0.15 * sin(radians(1.5 * shearedCoord.x + 30.0 * time + waveOffset)));
                float fragDistFromCenter = abs(shearedCoord.y - (centerY + (amp * 0.5) * irregWaveFunction(1.0 * shearedCoord.x - 32.0 * time - waveOffset)));
                return smoothstep(pulsingHalfWidth + blurriness, pulsingHalfWidth, fragDistFromCenter);
            }

            float2 topPath(float2 coord, float time)
            {
                return float2(coord.x, coord.y + 32.0 * sin(0.01 * coord.x + 0.2 * time) - 0.00015 * pow(coord.x-180.0, 2.0));
            }

            float2 bottomPath(float2 coord, float time)
            {
                return float2(coord.x, coord.y + 36.0 * sin(0.005 * coord.x + 0.25 * time) + 0.08 * coord.x);
            }   

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                half a = color.a;

                float2 waveScreenCoord = IN.texcoord * _CanvasSize;
                if (_ScreenParams.x / _ScreenParams.y > _CanvasSize.x / _CanvasSize.y)
                {
                    float2 coordsTransform = (_CanvasSize.y * _ScreenParams.x) / (_CanvasSize.x * _ScreenParams.y);
                    waveScreenCoord *= coordsTransform;
                    waveScreenCoord.y += (_CanvasSize.y - _CanvasSize.y * coordsTransform) / 2;
                } 

                color.a = 0;
                float topAnimFrame = _AnimFrame * _TopWaveSpeed;
                float bottomAnimFrame = _AnimFrame * _BottomWaveSpeed;
                color.a += a * calculateWave(topPath(waveScreenCoord, topAnimFrame), 540.0, 90.0, 170.0, 21.0, 0.0, topAnimFrame);
                color.a += a * calculateWave(bottomPath(waveScreenCoord,bottomAnimFrame), 260.0, 40.0, -20.0, 18.0, 800.0, bottomAnimFrame);

                a = color.a;
                fixed4 screen = 1.0 - (1.0 - _AddColor.rgba) * (1.0 - color.rgba);
                color = screen * _Color.rgba;
                color.a = a;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}