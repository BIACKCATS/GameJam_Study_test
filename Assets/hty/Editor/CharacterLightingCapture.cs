using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Hty.CharacterLighting;

namespace Hty.EditorTools
{
    // Captures actual SRP camera output in Play Mode; never synthesizes lighting on CPU.
    public static class CharacterLightingCapture
    {
        static CharacterNormalLighting rig;
        static RenderTexture target;
        static int index, settling;
        static readonly string[] Directions={"Left","Right","Up","Down"};
        static StringBuilder events;
        public static bool Running { get; private set; }
        public static string Progress => Running ? index+"/60" : "idle";
        [MenuItem("HTY/Character Lighting/Capture 48 comparisons and 12 orbit views (Play Mode)")]
        public static void Begin()
        {
            if(File.Exists(CharacterNormalTestBuilder.Reports+"/FROZEN.txt"))throw new Exception("Frozen artifacts: coordinate before recapturing");
            if (!EditorApplication.isPlaying) throw new Exception("Enter Play Mode in the isolated test scene first");
            if (Running) throw new Exception("Capture already running");
            rig=UnityEngine.Object.FindAnyObjectByType<CharacterNormalLighting>();
            if (!rig || rig.frames.Length!=6) throw new Exception("Missing six-frame test rig");
            rig.enabled=false;
            target=new RenderTexture(640,480,24,RenderTextureFormat.ARGB32) {filterMode=FilterMode.Point,antiAliasing=1}; target.Create();
            rig.testCamera.targetTexture=target;
            events=new StringBuilder("index,file,frame,direction,normals,unityFrame,updates,lightX,lightY\n");
            index=0; settling=0; Running=true; Configure();
            RenderPipelineManager.endCameraRendering+=OnRendered;
        }
        static void Configure()
        {
            if(index<48) rig.SetState(index/8,(index/2)%4,index%2==0);
            else rig.SetState(0,0,true,(index-48)*Mathf.PI/6);
            settling=0;
        }
        static void OnRendered(ScriptableRenderContext context,Camera camera)
        {
            if(!Running || camera!=rig.testCamera || ++settling<3) return;
            try
            {
                string name=index<48 ? "F"+(index/8)+"_"+Directions[(index/2)%4]+"_"+(index%2==0?"ON":"OFF") : "Orbit_"+(index-48).ToString("D2");
                var previous=RenderTexture.active; RenderTexture.active=target;
                var image=new Texture2D(640,480,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,640,480),0,0); image.Apply();
                File.WriteAllBytes(CharacterNormalTestBuilder.Reports+"/Captures/"+name+".png",image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image); RenderTexture.active=previous;
                events.AppendLine(index+","+name+","+rig.frame+","+rig.direction+","+rig.normals+","+Time.frameCount+","+rig.renderedUpdates+","+rig.movingLight.transform.position.x.ToString(System.Globalization.CultureInfo.InvariantCulture)+","+rig.movingLight.transform.position.y.ToString(System.Globalization.CultureInfo.InvariantCulture));
                if(++index<60) Configure(); else Finish();
            }
            catch(Exception ex) { Debug.LogException(ex); Finish(); }
        }
        static void Finish()
        {
            Running=false; RenderPipelineManager.endCameraRendering-=OnRendered;
            rig.testCamera.targetTexture=null;
            var finishedTarget=target; target=null;
            EditorApplication.delayCall+=()=> { if(RenderTexture.active==finishedTarget)RenderTexture.active=null; finishedTarget.Release(); UnityEngine.Object.DestroyImmediate(finishedTarget); };
            rig.enabled=true; rig.animate=true; rig.slowOrbit=true;
            File.WriteAllText(CharacterNormalTestBuilder.Reports+"/CaptureEvents.csv",events.ToString());
            Debug.Log("HTY_CAPTURE_FINISHED count="+index);
        }
        [MenuItem("HTY/Character Lighting/Validate source, imports and captured pairs")]
        public static void Validate()
        {
            if(File.Exists(CharacterNormalTestBuilder.Reports+"/FROZEN.txt"))throw new Exception("Frozen artifacts: read existing validation or coordinate new report");
            var di=(TextureImporter)AssetImporter.GetAtPath(CharacterNormalTestBuilder.Diffuse);
            var ni=(TextureImporter)AssetImporter.GetAtPath(CharacterNormalTestBuilder.Normal);
            var data=new Texture2D(2,2,TextureFormat.RGBA32,false); data.LoadImage(File.ReadAllBytes(CharacterNormalTestBuilder.Normal));
            var src=new Texture2D(2,2,TextureFormat.RGBA32,false); src.LoadImage(File.ReadAllBytes(CharacterNormalTestBuilder.Source));
            int alphaMismatch=0,neutral=0,nonneutral=0;
            for(int y=0;y<16;y++) for(int x=0;x<96;x++)
            { var a=(Color32)src.GetPixel(x,y);var b=(Color32)data.GetPixel(x,y);if(a.a!=b.a)alphaMismatch++;if(a.a>0){if(Mathf.Abs(b.r-128)<=1 && Mathf.Abs(b.g-128)<=1)neutral++;else nonneutral++;} }
            var sb=new StringBuilder();sb.AppendLine("Unity "+Application.unityVersion+" GPU "+SystemInfo.graphicsDeviceName+" API "+SystemInfo.graphicsDeviceType);
            sb.AppendLine("Source alpha mismatch pixels="+alphaMismatch+" flat opaque="+neutral+" directional opaque="+nonneutral);
            sb.AppendLine("Diffuse sprite count="+AssetDatabase.LoadAllAssetsAtPath(CharacterNormalTestBuilder.Diffuse).OfType<Sprite>().Count()+" PPU="+di.spritePixelsPerUnit+" filter="+di.filterMode+" mip="+di.mipmapEnabled+" compression="+di.textureCompression);
            sb.AppendLine("Normal type="+ni.textureType+" filter="+ni.filterMode+" mip="+ni.mipmapEnabled+" compression="+ni.textureCompression+" secondary="+string.Join(",",di.secondarySpriteTextures.Select(t=>t.name)));
            int failed=0;
            for(int f=0;f<6;f++) foreach(var d in Directions)
            {
                var on=Load("F"+f+"_"+d+"_ON");var off=Load("F"+f+"_"+d+"_OFF");
                var a=on.GetPixels32();var b=off.GetPixels32();int changed=0;double delta=0;int clipped=0;
                for(int i=0;i<a.Length;i++){int diff=Math.Abs(a[i].r-b[i].r)+Math.Abs(a[i].g-b[i].g)+Math.Abs(a[i].b-b[i].b);if(diff>3)changed++;delta+=diff;if(a[i].r==255&&a[i].g==255&&a[i].b==255)clipped++;}
                if(changed==0)failed++;
                sb.AppendLine("F"+f+" "+d+" changedPixels="+changed+" totalRGBDelta="+delta+" allWhitePixels="+clipped);
                UnityEngine.Object.DestroyImmediate(on);UnityEngine.Object.DestroyImmediate(off);
            }
            sb.AppendLine("Zero-difference pair failures="+failed);
            File.WriteAllText(CharacterNormalTestBuilder.Reports+"/Validation.txt",sb.ToString());
            UnityEngine.Object.DestroyImmediate(data);UnityEngine.Object.DestroyImmediate(src);
            if(alphaMismatch!=0||failed!=0)throw new Exception("Validation failed; read Reports/Validation.txt");
            Debug.Log("HTY_VALIDATION_OK 24 ON/OFF pairs");
        }
        static Texture2D Load(string name){var t=new Texture2D(2,2);t.LoadImage(File.ReadAllBytes(CharacterNormalTestBuilder.Reports+"/Captures/"+name+".png"));return t;}
        [MenuItem("HTY/Character Lighting/Create actual-render comparison sheet")]
        public static void ContactSheet()
        {
            if(File.Exists(CharacterNormalTestBuilder.Reports+"/FROZEN.txt"))throw new Exception("Frozen artifacts");
            var sheet=new Texture2D(896,480,TextureFormat.RGB24,false);
            var background=Enumerable.Repeat(new Color(.07f,.08f,.11f),896*480).ToArray();sheet.SetPixels(background);
            for(int d=0;d<4;d++)for(int state=0;state<2;state++)
            {
                string key="F0_"+Directions[d]+"_"+(state==0?"ON":"OFF");var capture=Load(key);
                int ox=d*224+16,oy=state==0?256:16;
                for(int y=0;y<192;y++)for(int x=0;x<192;x++)sheet.SetPixel(ox+x,oy+y,capture.GetPixel(272+x/2,192+y/2));
                Label(sheet,"F0 "+"LRUD"[d]+" "+(state==0?"ON":"OFF"),ox,oy+198);
                UnityEngine.Object.DestroyImmediate(capture);
            }
            sheet.Apply();File.WriteAllBytes(CharacterNormalTestBuilder.Reports+"/Comparison_Frame0.png",sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);
        }
        static void Label(Texture2D texture,string text,int x,int y)
        {
            // Minimal fixed bitmap labels; underlying picture pixels are exact nearest-neighbor render crops.
            string letters="F0LRUDON";
            string[] glyphs={"11111/10000/10000/11110/10000/10000/10000","01110/10001/10011/10101/11001/10001/01110","10000/10000/10000/10000/10000/10000/11111","11110/10001/10001/11110/10100/10010/10001","10001/10001/10001/10001/10001/10001/01110","11110/10001/10001/10001/10001/10001/11110","01110/10001/10001/10001/10001/10001/01110","10001/11001/11001/10101/10011/10011/10001"};
            foreach(char c in text){int at=letters.IndexOf(c);if(at>=0){var rows=glyphs[at].Split('/');for(int gy=0;gy<7;gy++)for(int gx=0;gx<5;gx++)if(rows[gy][gx]=='1')for(int dy=0;dy<2;dy++)for(int dx=0;dx<2;dx++)texture.SetPixel(x+gx*2+dx,y+(6-gy)*2+dy,Color.white);}x+=12;}
        }
        public static void ValidateKeyboardControls()
        {
            if(!EditorApplication.isPlaying || Running)throw new Exception("Run in Play Mode after captures finish");
            if(File.Exists(CharacterNormalTestBuilder.Reports+"/FROZEN.txt"))throw new Exception("Frozen artifacts");
            var c=UnityEngine.Object.FindAnyObjectByType<CharacterNormalLighting>();
            var previous=UnityEngine.InputSystem.Keyboard.current;
            var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
            try
            {
                c.animate=false;c.slowOrbit=false;c.SetState(0,0,true);
                Action<UnityEngine.InputSystem.Key> press=key=> {
                    UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(key));
                    UnityEngine.InputSystem.InputSystem.Update();c.SendMessage("Update");
                    UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());
                    UnityEngine.InputSystem.InputSystem.Update();
                };
                var keys=new[]{UnityEngine.InputSystem.Key.LeftArrow,UnityEngine.InputSystem.Key.RightArrow,UnityEngine.InputSystem.Key.UpArrow,UnityEngine.InputSystem.Key.DownArrow};
                for(int i=0;i<4;i++){press(keys[i]);if(c.direction!=i||c.slowOrbit)throw new Exception("Arrow control failed "+i);}
                press(UnityEngine.InputSystem.Key.N);if(c.normals||c.movingLight.normalMapQuality!=UnityEngine.Rendering.Universal.Light2D.NormalMapQuality.Disabled)throw new Exception("Normal OFF failed");
                press(UnityEngine.InputSystem.Key.N);if(!c.normals||c.movingLight.normalMapQuality!=UnityEngine.Rendering.Universal.Light2D.NormalMapQuality.Accurate)throw new Exception("Normal ON failed");
                press(UnityEngine.InputSystem.Key.Period);if(c.frame!=1||c.animate)throw new Exception("Frame forward failed");
                press(UnityEngine.InputSystem.Key.Comma);if(c.frame!=0)throw new Exception("Frame backward failed");
                press(UnityEngine.InputSystem.Key.Space);if(!c.animate)throw new Exception("Animation toggle failed");
                press(UnityEngine.InputSystem.Key.O);if(!c.slowOrbit)throw new Exception("Orbit toggle failed");
                File.WriteAllText(CharacterNormalTestBuilder.Reports+"/Controls.txt","PASS: actual runtime Update consumed temporary Input System keyboard events for four arrows, N OFF/ON, period/comma frame step, Space animation and O orbit. Temporary keyboard removed, original device restored. Render capture proves 60 advancing camera frames; orbit capture uses fixed F0.\n");
            }
            finally { UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);if(previous!=null)previous.MakeCurrent(); }
        }
    }
}
