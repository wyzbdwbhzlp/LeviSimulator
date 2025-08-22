Shader "Custom/SmokeSphere"
{
    Properties
    {
        _SmokeColor ("Tint Color", Color) = (1,1,1,1)      // 烟雾整体颜色
        _Alpha("Global Alpha", Range(0,1)) = 1             // 全局透明度
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 2 // 边缘 Fresnel 强度

        _NoiseTex("Noise Texture", 2D) = "white" {}       // 烟雾贴图
        _NoiseScale("Noise Scale", Float) = 1             // 烟雾贴图缩放
        _NoiseSpeed("Noise Speed", Float) = 0.2           // 烟雾流动速度
    }

    SubShader
    {
        // 透明材质，在内层材质后渲染
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // 透明混合
            ZWrite Off                      // 不写入深度缓冲，避免遮挡内部物体

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _SmokeColor;
            float _Alpha;
            float _FresnelPower;

            sampler2D _NoiseTex;
            float4 _NoiseTex_ST; 
            float _NoiseScale;
            float _NoiseSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // ---------- Fresnel ---------- 
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = 1 - saturate(dot(viewDir, i.worldNormal));
                fresnel = pow(fresnel, _FresnelPower);

                // ---------- 噪声贴图 ----------
                float2 noiseUV = i.uv * _NoiseScale + float2(_Time.y * _NoiseSpeed, 0);
                float noise = tex2D(_NoiseTex, noiseUV).r;

                // ---------- 最终颜色 ----------
                fixed4 col = _SmokeColor;
                col.a = noise * fresnel * _Alpha; // 透明度由噪声和 Fresnel 控制
                return col;
            }
            ENDCG
        }
    }
}
