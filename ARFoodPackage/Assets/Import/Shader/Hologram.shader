Shader "Custom/Hologram"
{
    Properties
    {
        _BaseColor   ("Base Color", Color)   = (0.0, 0.5, 1.0, 0.3)
        _RimColor    ("Rim Color", Color)    = (0.5, 1.0, 1.0, 1.0)
        _RimPower    ("Rim Power", Range(1,8)) = 3.0
        _ScanSpeed   ("Scan Speed", Float)   = 1.0
        _ScanScale   ("Scan Scale", Float)   = 40.0
        _MainTex     ("Main Texture", 2D)    = "white" {}
    }
    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
        }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard alpha:fade fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        float4   _BaseColor;
        float4   _RimColor;
        half     _RimPower;
        float    _ScanSpeed;
        float    _ScanScale;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 viewDir;        // _WorldSpaceCameraPos - worldPos, auto-filled
            float4 screenPos;      // for scanline
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // 1. 基础颜色贴图+半透明
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = tex.rgb * _BaseColor.rgb;
            o.Alpha  = tex.a * _BaseColor.a;

            // 2. Fresnel 边缘发光 (Rim)
            float ndotv = saturate(dot(normalize(o.Normal), normalize(IN.viewDir)));
            float rim = pow(1 - ndotv, _RimPower);
            o.Emission = _RimColor.rgb * rim;

            // 3. 扫描线效果 (沿屏幕 Y 方向)
            //    计算屏幕空间 Y（0~1），加上时间偏移，产生条纹
            float2 sc = IN.screenPos.xy / IN.screenPos.w;
            float scan = sin( (sc.y * _ScanScale + _Time.y * _ScanSpeed) * UNITY_PI );
            scan = smoothstep(0.0, 0.1, scan);   // 调整对比度
            o.Emission += o.Albedo * scan * 0.2; // 把扫描线也加到 Emission

            // 4. 金属度/光滑度
            o.Metallic  = 0.0;
            o.Smoothness = 0.6;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
