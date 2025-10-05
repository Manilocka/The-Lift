Shader "Hidden/RetroEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FishEyeStrength ("Fish Eye Strength", Range(0, 2)) = 0.3
        _ChromaticAberration ("Chromatic Aberration", Range(0, 1)) = 0.1
        _Vignette ("Vignette", Range(0, 1)) = 0.3
        _Noise ("Noise", Range(0, 1)) = 0.05
        _NoiseOffset ("Noise Offset", Float) = 0
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.3
        _ScanlineCount ("Scanline Count", Range(1, 10)) = 3
        _ScanlineOffset ("Scanline Offset", Float) = 0
        _TintColor ("Tint Color", Color) = (0.2, 1, 0.2, 1)
        _TintStrength ("Tint Strength", Range(0, 1)) = 0.2
        _EnableScanlines ("Enable Scanlines", Int) = 1
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
            float _FishEyeStrength;
            float _ChromaticAberration;
            float _Vignette;
            float _Noise;
            float _NoiseOffset;
            float _ScanlineIntensity;
            float _ScanlineCount;
            float _ScanlineOffset;
            float4 _TintColor;
            float _TintStrength;
            int _EnableScanlines;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            float2 fisheye(float2 uv, float strength)
            {
                float2 center = float2(0.5, 0.5);
                float2 delta = uv - center;
                float distance = length(delta);
                float distortion = 1.0 + strength * distance * distance;
                return center + delta * distortion;
            }
            
            float vignette(float2 uv, float strength)
            {
                float2 center = float2(0.5, 0.5);
                float distance = length(uv - center);
                return 1.0 - smoothstep(0.3, 0.8, distance) * strength;
            }
            
            float scanlines(float2 uv, float count, float intensity, float offset)
            {
                float scanline = sin((uv.y + offset) * count * 3.14159 * 2.0) * 0.5 + 0.5;
                return lerp(1.0, scanline, intensity);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // Fish eye эффект
                uv = fisheye(uv, _FishEyeStrength);
                
                // Проверка на выход за границы
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                {
                    return fixed4(0, 0, 0, 1);
                }
                
                // Хроматическая аберрация
                float2 offset = float2(_ChromaticAberration * 0.01, 0);
                fixed4 colR = tex2D(_MainTex, uv + offset);
                fixed4 colG = tex2D(_MainTex, uv);
                fixed4 colB = tex2D(_MainTex, uv - offset);
                
                fixed4 col = fixed4(colR.r, colG.g, colB.b, colG.a);
                
                // Виньетка
                float vignetteFactor = vignette(uv, _Vignette);
                col.rgb *= vignetteFactor;
                
                // Шум
                if (_Noise > 0)
                {
                    float noiseValue = random(uv + _NoiseOffset) * _Noise;
                    col.rgb += noiseValue;
                }
                
                // Сканирующие линии
                if (_EnableScanlines)
                {
                    float scanlineFactor = scanlines(uv, _ScanlineCount, _ScanlineIntensity, _ScanlineOffset);
                    col.rgb *= scanlineFactor;
                }
                
                // Цветовой оттенок
                col.rgb = lerp(col.rgb, col.rgb * _TintColor.rgb, _TintStrength);
                
                return col;
            }
            ENDCG
        }
    }
}
