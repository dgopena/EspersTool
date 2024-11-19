Shader "Particles/Z Write Pre-Pass Alpha Blend"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Cutoff("Cutoff", Range(0,1)) = 0.5
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 100

            Pass {
                ColorMask 0

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;

                fixed _Cutoff;

                v2f vert(appdata_full v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                    return o;
                }

                void frag(v2f i)
                {
                    fixed4 col = tex2D(_MainTex, i.uv);
                    clip(col.a - _Cutoff);
                }
                ENDCG
            }

            Pass
            {
                Blend SrcAlpha OneMinusSrcAlpha
                ZWrite Off

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    fixed4 color : TEXCOORD1;
                    UNITY_FOG_COORDS(2)
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;

                fixed _Cutoff;

                v2f vert(appdata_full v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    o.color = v.color;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv);
                    clip(col.a - _Cutoff);
                    UNITY_APPLY_FOG(i.fogCoord, col);
                    col.rgb *= i.color.rgb;
                    col.a = i.color.a;
                    return col;
                }
                ENDCG
            }
        }
}