Shader "WattsTap/UI/Gradient"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _ColorTop ("Top Color", Color) = (1,1,1,1)
        _ColorBottom ("Bottom Color", Color) = (0,0,0,1)
        _Angle ("Gradient Angle", Range(0, 360)) = 0
        _GradientStrength ("Gradient Strength", Range(0.1, 10)) = 1
        
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
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float2 gradientUV : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;
            fixed4 _ColorTop;
            fixed4 _ColorBottom;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _Angle;
            float _GradientStrength;
            
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                
                OUT.color = v.color;
                
                // Calculate gradient UV based on angle
                float angleRad = radians(_Angle);
                float cosAngle = cos(angleRad);
                float sinAngle = sin(angleRad);
                
                // Rotate UV coordinates
                float2 centeredUV = v.texcoord - 0.5;
                float rotatedV = centeredUV.x * sinAngle + centeredUV.y * cosAngle;
                OUT.gradientUV = float2(0, (rotatedV + 0.5));
                
                return OUT;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                // Sample texture
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
                
                // Calculate gradient factor with strength control
                float gradientFactor = saturate(pow(abs(IN.gradientUV.y), 1.0 / _GradientStrength));
                
                // Mix colors based on gradient
                fixed4 gradientColor = lerp(_ColorBottom, _ColorTop, gradientFactor);
                
                // Apply gradient to texture
                color *= gradientColor;
                
                // Apply vertex color
                color *= IN.color;
                
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

