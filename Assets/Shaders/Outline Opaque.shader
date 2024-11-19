Shader "Custom/Outline Opaque"
{
    Properties
    {
        _Color("Main Color", Color) = (.5,.5,.5,1)
        _MainTex("Texture", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        [Space(10)]
        _OutColor("Outline Color", Color) = (1,1,1,1)
        _OutValue("Outline Value", Range(0.0,0.2)) = 0.1
    }
    SubShader
    {
        // outlien pass
        Pass
        {
            Tags
            {
                "Queue" = "Transparent"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _OutColor;
            float _OutValue;
            float4 _Color;

            float4 outline(float4 vertexPos, float outValue)
            {
                float4x4 scale = float4x4
                    (
                        1 + outValue,0,0,0,
                        0,1 + outValue,0,0,
                        0,0,1 + outValue,0,
                        0,0,0,1 + outValue
                        );
                return mul(scale, vertexPos);
            }

            v2f vert(appdata v)
            {
                v2f o;

                float4 vertexPos = outline(v.vertex, _OutValue);

                o.vertex = UnityObjectToClipPos(vertexPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return float4(_OutColor.r, _OutColor.g, _OutColor.b, _OutColor.a);
            }
            ENDCG
        }

        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
                // Physically based Standard lighting model, and enable shadows on all light types
                #pragma surface surf Standard fullforwardshadows

                // Use shader model 3.0 target, to get nicer looking lighting
                #pragma target 3.0

                sampler2D _MainTex;

                struct Input
                {
                    float2 uv_MainTex;
                };

                half _Glossiness;
                half _Metallic;
                fixed4 _Color;

                // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
                // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
                // #pragma instancing_options assumeuniformscaling
                UNITY_INSTANCING_BUFFER_START(Props)
                    // put more per-instance properties here
                UNITY_INSTANCING_BUFFER_END(Props)

                void surf(Input IN, inout SurfaceOutputStandard o)
                {
                    // Albedo comes from a texture tinted by color
                    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                    o.Albedo = c.rgb;
                    // Metallic and smoothness come from slider variables
                    o.Metallic = _Metallic;
                    o.Smoothness = _Glossiness;
                    o.Alpha = c.a;
                }
                ENDCG
    }
}
