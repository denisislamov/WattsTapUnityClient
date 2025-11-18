Shader "WattsTap/UI/Border"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BorderColor ("Border Color", Color) = (1,1,1,1)
        _BorderThickness ("Border Thickness", Range(0, 0.49)) = 0.02
        _CornerRadius ("Corner Radius", Range(0, 0.49)) = 0
        _SideEnable ("Sides (Left,Top,Right,Bottom)", Vector) = (1,1,1,1)
        _EdgeSoftness ("Edge Softness", Range(0.0005, 0.05)) = 0.005

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
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _BorderColor;
            float4 _SideEnable;
            float _BorderThickness;
            float _CornerRadius;
            float _EdgeSoftness;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            inline float sdRoundRect(float2 p, float2 halfSize, float radius)
            {
                float2 hs = max(halfSize, float2(0.0001, 0.0001));
                float maxRadius = min(hs.x, hs.y) - 0.0001;
                float r = clamp(radius, 0.0, maxRadius);
                float2 q = abs(p) - (hs - r);
                float2 maxQ = max(q, 0.0);
                return length(maxQ) + min(max(q.x, q.y), 0.0) - r;
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
                float2 uv = IN.texcoord;
                float2 centered = uv - 0.5;
                float2 halfSize = float2(0.5, 0.5);

                float thickness = saturate(_BorderThickness);
                thickness = min(thickness, 0.49);
                float radius = clamp(_CornerRadius, 0.0, 0.49);

                float outerSdf = sdRoundRect(centered, halfSize, radius);

                float2 innerHalfSize = max(halfSize - thickness, float2(0.001, 0.001));
                float innerRadius = max(radius - thickness, 0.0);
                float innerSdf = sdRoundRect(centered, innerHalfSize, innerRadius);

                float softness = max(_EdgeSoftness, 0.0005);
                float outerMask = saturate(1.0 - smoothstep(0.0, softness, outerSdf));
                float innerMask = saturate(1.0 - smoothstep(0.0, softness, innerSdf));
                float ring = saturate(outerMask - innerMask);
                ring *= step(0.0001, thickness);

                float dirSoft = max(softness * 0.5, 0.00025);
                float leftMask = saturate(1.0 - smoothstep(thickness, thickness + dirSoft, uv.x));
                float rightMask = saturate(1.0 - smoothstep(thickness, thickness + dirSoft, 1.0 - uv.x));
                float bottomMask = saturate(1.0 - smoothstep(thickness, thickness + dirSoft, uv.y));
                float topMask = saturate(1.0 - smoothstep(thickness, thickness + dirSoft, 1.0 - uv.y));

                float directional = max(
                    max(leftMask * _SideEnable.x, rightMask * _SideEnable.z),
                    max(topMask * _SideEnable.y, bottomMask * _SideEnable.w)
                );

                float borderMask = saturate(ring * directional);

                fixed4 vertexColor = IN.color;
                fixed4 baseColor = (tex2D(_MainTex, uv) + _TextureSampleAdd) * _Color * vertexColor;
                fixed4 borderColor = _BorderColor * vertexColor;

                fixed4 color = baseColor;
                color.rgb = lerp(color.rgb, borderColor.rgb, borderMask);
                color.a = saturate(baseColor.a + borderMask * borderColor.a);

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

