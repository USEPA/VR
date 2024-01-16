Shader "Custom/AlwaysVisibleShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
             Cull Off
         Pass{
             ZTest Greater
             }
         Pass{
             ZTest Less
         }
         Pass{
             ZTest Always
         }

         Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf NoLighting

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

        void surf (Input IN, inout SurfaceOutput o) {
         //o.Albedo = _Color.rgb * _Color.a;
         o.Emission = _Color.rgb;
           o.Alpha = _Color.a;
     }
      fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten) {
         return fixed4(0,0,0,0);//half4(s.Albedo, s.Alpha);
     }

        ENDCG
    }
    FallBack "Diffuse"
}
