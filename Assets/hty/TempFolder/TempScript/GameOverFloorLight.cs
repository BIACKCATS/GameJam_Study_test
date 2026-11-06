using UnityEngine;
using UnityEngine.UI;

// Soft pool of light on the floor, drawn behind the character.
public sealed class GameOverFloorLight : MaskableGraphic
{
    public RawImage character;
    public RawImage sourceLight;
    [Min(0.1f)] public float widthMultiplier = 1.2f;
    [Range(0.1f,1f)] public float heightRatio = 0.3f;
    public Vector2 floorOffset = new Vector2(0,-65);
    [Range(0,1)] public float opacity = 0.28f;

    void LateUpdate()
    {
        if(!character || !sourceLight)return;
        var target=character.rectTransform;
        rectTransform.anchorMin=target.anchorMin;
        rectTransform.anchorMax=target.anchorMax;
        rectTransform.pivot=target.pivot;
        rectTransform.anchoredPosition=target.anchoredPosition+floorOffset;
        float width=target.rect.width*Mathf.Abs(target.localScale.x)*widthMultiplier;
        rectTransform.sizeDelta=new Vector2(width,width*heightRatio);
        color=new Color(1,0.98f,0.9f,opacity*sourceLight.color.a);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var rect=rectTransform.rect;
        const int segments=64;
        var vertex=UIVertex.simpleVert;
        vertex.position=rect.center;vertex.color=color;vh.AddVert(vertex);
        for(int ring=0;ring<2;ring++)
        {
            float radius=ring==0?0.65f:1f;
            var tint=color;tint.a*=ring==0?0.65f:0f;
            for(int i=0;i<segments;i++)
            {
                float angle=i*Mathf.PI*2/segments;
                vertex.position=rect.center+new Vector2(Mathf.Cos(angle)*rect.width,Mathf.Sin(angle)*rect.height)*0.5f*radius;
                vertex.color=tint;vh.AddVert(vertex);
            }
        }
        for(int i=0;i<segments;i++)
        {
            int a=1+i,b=1+(i+1)%segments;
            vh.AddTriangle(0,a,b);
            vh.AddTriangle(a,a+segments,b+segments);
            vh.AddTriangle(a,b+segments,b);
        }
    }
}
