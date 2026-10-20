// Modified Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Unlit/Mem" {
Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
    // _OverlayTex ("Speck Overlay (RGB)", 2D) = "white" {}
}

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 100

    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _OverlayTex;
            float4 _OverlayTex_ST;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_TexelSize;
            CBUFFER_END   

            float getOutlineMix(v2f i, int numRounds)
            {
                float center = tex2D(_MainTex, i.texcoord).a;
                fixed2 nextCoord;
                int step = 5.0;
                int directions = 16;
                int numToDivide = 1;
                for (int r = 1; r < numRounds; r++) {
                    for (float t = 0; t < 360.0; t += 360.0 / directions) {
                        nextCoord = i.texcoord + fixed2(step * r * cos(t) * _MainTex_TexelSize.x, step * r * sin(t) * _MainTex_TexelSize.y);
                        if (nextCoord.x == saturate(nextCoord.x) && nextCoord.y == saturate(nextCoord.y)) {
                            center += tex2D(_MainTex, nextCoord).a;
                            numToDivide++;
                        }
                    }
                }
                center = center / numToDivide;
                return 1.0 - center;
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
                fixed4 colOverlay = tex2D(_OverlayTex, i.texcoord);
                col.rgb -= 0.05; // darken overall
                col.rgb += (colOverlay.rgb * (1 - 2.5 * getOutlineMix(i, 2))); // noise
                col.rgb += 0.5 * getOutlineMix(i, 8); // inner glow
                col.rgb -= 0.8 * getOutlineMix(i, 2); // darken sides

                UNITY_APPLY_FOG(i.fogCoord, col);
                UNITY_OPAQUE_ALPHA(col.a);
                return col;
            }
        ENDCG
    }
}

}
