using UnityEngine;
using UnityEngine.UI;

// Frame-local masks follow the door image at every canvas size without editing the sheet.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class GongikWindowBlackout : MaskableGraphic
{
    protected override void Awake(){base.Awake();raycastTarget=false;color=Color.black;}
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        AddWindow(mesh,new[]{new Vector2(0,91),new Vector2(46,91),new Vector2(53,98),new Vector2(53,174),new Vector2(46,181),new Vector2(0,181)});
        // The small upper-right cutout preserves the hanging strap over the glass.
        AddWindow(mesh,new[]{new Vector2(371,110),new Vector2(426,110),new Vector2(426,181),new Vector2(378,181),new Vector2(371,174)});
        AddWindow(mesh,new[]{new Vector2(371,110),new Vector2(371,98),new Vector2(378,91),new Vector2(413,91),new Vector2(413,110)});
    }
    void AddWindow(VertexHelper mesh,Vector2[] points)
    {
        int start=mesh.currentVertCount;var r=rectTransform.rect;
        foreach(var p in points)mesh.AddVert(new Vector3(r.xMin+p.x/426f*r.width,r.yMax-p.y/307f*r.height),color,Vector2.zero);
        for(int i=1;i<points.Length-1;i++)mesh.AddTriangle(start,start+i,start+i+1);
    }
}
