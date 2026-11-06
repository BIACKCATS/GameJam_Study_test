using UnityEngine;

// Pooled UI rocket, animated PixelFX burst, and varied colored spark rays.
public sealed class TrueEndingFirework : MonoBehaviour
{
    UnityEngine.UI.RawImage primary, secondary;
    UnityEngine.UI.Image rocket, trail;
    readonly UnityEngine.UI.Image[] sparks=new UnityEngine.UI.Image[20];
    readonly Vector2[] directions=new Vector2[20];
    Vector2 origin,destination;
    float age,flight,size,fps,rotation,visualScale=1;
    Color hue;
    bool built;
    T Graphic<T>(string label) where T:UnityEngine.UI.Graphic
    {
        var go=new GameObject(label,typeof(RectTransform),typeof(CanvasRenderer));go.transform.SetParent(transform,false);
        var image=go.AddComponent<T>();image.raycastTarget=false;return image;
    }
    void Build()
    {
        if(built)return;built=true;
        trail=Graphic<UnityEngine.UI.Image>("RocketTrail");rocket=Graphic<UnityEngine.UI.Image>("Rocket");
        primary=Graphic<UnityEngine.UI.RawImage>("PixelFXBurst");secondary=Graphic<UnityEngine.UI.RawImage>("SecondaryBurst");
        for(int i=0;i<sparks.Length;i++)sparks[i]=Graphic<UnityEngine.UI.Image>("Spark"+i);
    }
    public void Launch(Texture2D[] sheets,Vector2 start,Vector2 end,float travel,float diameter,float frameRate,float scale=1)
    {
        Build();visualScale=Mathf.Max(0.01f,scale);origin=start;destination=end;flight=travel;size=diameter*visualScale;fps=frameRate;age=0;
        rotation=Random.Range(0,360);hue=Color.HSVToRGB(Random.value,0.5f,1);
        int index=Random.Range(0,sheets.Length);primary.texture=sheets[index];secondary.texture=sheets[(index+1)%sheets.Length];
        primary.rectTransform.sizeDelta=Vector2.one*size;secondary.rectTransform.sizeDelta=Vector2.one*size*Random.Range(0.55f,0.8f);
        primary.rectTransform.localRotation=Quaternion.Euler(0,0,rotation);secondary.rectTransform.localRotation=Quaternion.Euler(0,0,-rotation);
        primary.color=Color.white;secondary.color=new Color(hue.r,hue.g,hue.b,0.65f);
        for(int i=0;i<sparks.Length;i++)
        {
            float angle=(i*360f/sparks.Length+rotation)*Mathf.Deg2Rad;
            directions[i]=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*Random.Range(0.65f,1f);
            sparks[i].color=Color.HSVToRGB(Mathf.Repeat(rotation/360+i*0.035f,1),0.55f,1);
            sparks[i].rectTransform.sizeDelta=new Vector2(Random.Range(3,6),Random.Range(8,19))*visualScale;
            sparks[i].rectTransform.localRotation=Quaternion.Euler(0,0,angle*Mathf.Rad2Deg-90);
        }
        gameObject.SetActive(true);Render();
    }
    void Update(){age+=Time.unscaledDeltaTime;Render();}
    void Render()
    {
        bool flying=age<flight;
        rocket.enabled=trail.enabled=flying;primary.enabled=secondary.enabled=!flying;
        if(flying)
        {
            float t=Mathf.Clamp01(age/flight);float eased=1-(1-t)*(1-t);
            Vector2 point=Vector2.Lerp(origin,destination,eased),direction=(destination-origin).normalized;
            rocket.rectTransform.anchoredPosition=point;rocket.rectTransform.sizeDelta=new Vector2(5,14)*visualScale;
            rocket.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-90);
            rocket.color=Color.Lerp(hue,Color.white,0.6f);
            trail.rectTransform.anchoredPosition=point-direction*27*visualScale;trail.rectTransform.sizeDelta=new Vector2(3,48)*visualScale;
            trail.rectTransform.localRotation=rocket.rectTransform.localRotation;trail.color=new Color(hue.r,hue.g,hue.b,0.45f);
            foreach(var spark in sparks)spark.enabled=false;
            return;
        }
        float burstAge=age-flight;int frame=Mathf.FloorToInt(burstAge*fps);
        if(frame>=16){gameObject.SetActive(false);return;}
        primary.rectTransform.anchoredPosition=secondary.rectTransform.anchoredPosition=destination;
        primary.uvRect=secondary.uvRect=new Rect(frame%4/4f,1-(frame/4+1)/4f,0.25f,0.25f);
        float progress=Mathf.Clamp01(burstAge/(16/fps));
        for(int i=0;i<sparks.Length;i++)
        {
            var spark=sparks[i];spark.enabled=true;
            spark.rectTransform.anchoredPosition=destination+directions[i]*size*0.52f*(1-Mathf.Pow(1-progress,3))+Vector2.down*45*visualScale*progress*progress;
            var color=spark.color;color.a=(1-progress)*(1-progress);spark.color=color;
            spark.rectTransform.localScale=Vector3.one*Mathf.Lerp(1,0.3f,progress);
        }
    }
}
