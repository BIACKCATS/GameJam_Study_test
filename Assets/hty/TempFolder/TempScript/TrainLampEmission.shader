Shader "TempFolder/TrainLampEmission"
{
 Properties { [PerRendererData] _MainTex("Sprite",2D)="white"{} _LampEmission("Lamp Emission",Range(0,10))=3 }
 SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent"}
 Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass {
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex; float _LampEmission;
 struct Input {float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR;};
 struct Output {float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR;};
 Output vert(Input v){Output o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o;}
 float4 frag(Output i):SV_Target {
 float4 c=tex2D(_MainTex,i.uv)*i.color;
 // The fluorescent strips occupy the upper 2–8 percent of the full interior texture.
 float band=step(0.92,i.uv.y)*step(i.uv.y,0.985);
 float bright=smoothstep(0.5,0.85,min(c.r,min(c.g,c.b)));
 c.rgb*=1+band*bright*_LampEmission; return c;
 }
 ENDHLSL
 }
 }
}
