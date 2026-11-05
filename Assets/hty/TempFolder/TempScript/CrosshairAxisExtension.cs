using UnityEngine;

// Extends only the existing axes; the original animated center is unchanged.
public sealed class CrosshairAxisExtension : MonoBehaviour
{
    public CrosshairAimController controller;
    public bool extendToScreenEdges = true;
    readonly UnityEngine.UI.Image[] lines = new UnityEngine.UI.Image[8];
    static readonly int[] DismissFrames = {13,8,7,6,5,4,3,2,1,0,-2,-1};
    static readonly float[] ClickScale = {1,.96f,.91f,1.04f,1.015f,.99f,1,1};
    void Awake()
    {
        if (!controller || !controller.crosshair) { Debug.LogError("CrosshairAxisExtension: Missing crosshair reference.",this); enabled=false; return; }
        var source=controller.crosshair.rectTransform;
        for(int i=0;i<lines.Length;i++) {
            var go=new GameObject("CrosshairAxisExtension"+i,typeof(RectTransform),typeof(UnityEngine.UI.Image));
            var rect=go.GetComponent<RectTransform>(); rect.SetParent(source.parent,false);
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0.5f,0.5f);
            rect.SetSiblingIndex(source.GetSiblingIndex());
            lines[i]=go.GetComponent<UnityEngine.UI.Image>(); lines[i].raycastTarget=false; lines[i].enabled=false;
        }
    }
    void LateUpdate()
    {
        if(!controller || !controller.crosshair) { Hide(); return; }
        var image=controller.crosshair;
        if(!extendToScreenEdges || !image.enabled || !image.gameObject.activeInHierarchy) { Hide(); return; }
        bool click=image.texture==controller.clickSheet;
        bool dismiss=image.texture==controller.dismissSheet;
        int rows=click?3:dismiss?4:5;
        int column=Mathf.RoundToInt(image.uvRect.x*3);
        int row=Mathf.RoundToInt((1-image.uvRect.y)*rows)-1;
        int frame=Mathf.Clamp(row*3+column,0,click?7:dismiss?11:13);
        int sourceFrame=dismiss?DismissFrames[frame]:click?13:frame;
        if(sourceFrame<0) { Hide(); return; }
        float recoil=click?ClickScale[frame]:1;
        float horizontal=Mathf.Round(12+136*Mathf.Min(1,sourceFrame/5f));
        float vertical=Mathf.Min(80,horizontal);
        float pixel=6*controller.sizeMultiplier;
        var center=image.rectTransform.anchoredPosition;
        var bounds=((RectTransform)image.rectTransform.parent).rect;
        float opacity=Mathf.Clamp01(sourceFrame/5f);
        var dark=new Color32(65,15,27,255);
        var red=sourceFrame<2?new Color32(155,32,53,255):new Color32(255,51,77,255);
        for(int direction=0;direction<4;direction++) {
            float start,end;
            bool isHorizontal=direction<2;
            float extent=(isHorizontal?horizontal:vertical)*recoil*pixel;
            float axis=isHorizontal?center.x:center.y;
            if(direction==0) {start=bounds.xMin;end=axis-extent;}
            else if(direction==1) {start=axis+extent;end=bounds.xMax;}
            else if(direction==2) {start=bounds.yMin;end=axis-extent;}
            else {start=axis+extent;end=bounds.yMax;}
            for(int layer=0;layer<2;layer++) {
                var line=lines[direction*2+layer]; line.enabled=end>start && opacity>0;
                if(!line.enabled) continue;
                Color color=layer==0?dark:red; color.a=opacity; line.color=color;
                float thickness=(layer==0?3:1)*pixel*recoil;
                line.rectTransform.anchoredPosition=isHorizontal?new Vector2((start+end)*0.5f,center.y):new Vector2(center.x,(start+end)*0.5f);
                line.rectTransform.sizeDelta=isHorizontal?new Vector2(end-start+pixel,thickness):new Vector2(thickness,end-start+pixel);
            }
        }
    }
    void Hide() { foreach(var line in lines) if(line) line.enabled=false; }
    void OnDisable() { Hide(); }
    void OnDestroy() { foreach(var line in lines) if(line) Destroy(line.gameObject); }
}
