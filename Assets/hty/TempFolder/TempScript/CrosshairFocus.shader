Shader "TempFolder/CrosshairFocus"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Darkness ("Darkness", Range(0,1)) = 0.4
        _Radius ("Radius", Float) = 336
        _Softness ("Edge Softness", Float) = 16
        _Size ("Panel Size", Vector) = (1920,1080,0,0)
        _Center ("Center", Vector) = (960,540,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 _Size, _Center;
            float _Radius, _Softness, _Darkness;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            Output vert(Input v) { Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color; return o; }
            fixed4 frag(Output i) : SV_Target
            {
                float distanceFromAim = length(i.uv * _Size.xy - _Center.xy);
                float mask = smoothstep(_Radius, _Radius + max(0.01, _Softness), distanceFromAim);
                return fixed4(0,0,0,mask * _Darkness * i.color.a);
            }
            ENDHLSL
        }
    }
}
