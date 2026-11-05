Shader "TempFolder/GameOverBloomAdditive"
{
 Properties { [PerRendererData] _MainTex("Texture",2D)="white"{} _Emission("Emission",Float)=1 }
 SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
 Cull Off ZWrite Off ZTest Always Blend One One
 Pass {
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex; float _Emission;
 struct Input {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 struct Output {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
 Output vert(Input v){Output o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
 float4 frag(Output i):SV_Target {float4 c=tex2D(_MainTex,i.uv);return float4(c.rgb*c.a*i.color.rgb*i.color.a*_Emission,1);}
 ENDHLSL
 }
 }
}
