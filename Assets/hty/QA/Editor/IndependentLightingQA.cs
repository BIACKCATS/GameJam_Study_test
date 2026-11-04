using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Hty.CharacterLighting;

namespace Hty.QA
{
    public static class IndependentLightingQA
    {
        const string Root="Assets/hty/QA";
        const string Diffuse="Assets/hty/Textures/Player_Diffuse.png";
        const string Normal="Assets/hty/Textures/Player_Normal.png";
        const string Source="Packages/com.febucci.text-animator-unity/Samples~/Use case examples/DamageNumbers/Assets/Textures/PlayerSpriteSheett.png";
        static CharacterNormalLighting rig;
        static RenderTexture rt;
        static int index,settle;
        static StringBuilder events;
        static readonly string[] D={"Left","Right","Up","Down"};
        public static bool Running;
        public static string Progress=>Running ? index+"/61" : "idle";
        static string Hash(string path) { using(var h=System.Security.Cryptography.SHA256.Create()) return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant(); }
        static Texture2D Raw(string path) { var t=new Texture2D(2,2,TextureFormat.RGBA32,false);t.LoadImage(File.ReadAllBytes(path));return t; }
        public static string Audit()
        {
            var sb=new StringBuilder();
            var source=Raw(Source);var diffuse=Raw(Diffuse);var normal=Raw(Normal);
            int alpha=0,color=0,flat=0,directional=0;
            var s=source.GetPixels32();var d=diffuse.GetPixels32();var n=normal.GetPixels32();
            for(int i=0;i<s.Length;i++){if(s[i].a!=n[i].a)alpha++;if(!s[i].Equals(d[i]))color++;if(s[i].a>0){if(Math.Abs(n[i].r-128)<=1&&Math.Abs(n[i].g-128)<=1)flat++;else directional++;}}
            sb.AppendLine("UTC="+DateTime.UtcNow.ToString("O"));
            sb.AppendLine("project="+Application.dataPath+" Unity="+Application.unityVersion+" GPU="+SystemInfo.graphicsDeviceName+" API="+SystemInfo.graphicsDeviceType);
            sb.AppendLine("sourceSHA256="+Hash(Source)+" diffuseSHA256="+Hash(Diffuse)+" normalSHA256="+Hash(Normal));
            sb.AppendLine("size source="+source.width+"x"+source.height+" normal="+normal.width+"x"+normal.height+" RGBA mismatch="+color+" alpha mismatch="+alpha+" flat="+flat+" directional="+directional);
            var di=(TextureImporter)AssetImporter.GetAtPath(Diffuse);var ni=(TextureImporter)AssetImporter.GetAtPath(Normal);
            sb.AppendLine("diffuse type="+di.textureType+" mode="+di.spriteImportMode+" PPU="+di.spritePixelsPerUnit+" filter="+di.filterMode+" mip="+di.mipmapEnabled+" compression="+di.textureCompression);
            sb.AppendLine("normal type="+ni.textureType+" sRGB="+ni.sRGBTexture+" filter="+ni.filterMode+" mip="+ni.mipmapEnabled+" compression="+ni.textureCompression+" convert="+ni.convertToNormalmap);
            foreach(var t in di.secondarySpriteTextures)sb.AppendLine("secondary="+t.name+" path="+AssetDatabase.GetAssetPath(t.texture));
            foreach(var p in new[]{"Standalone","Android","iPhone","WebGL"}){var a=di.GetPlatformTextureSettings(p);var b=ni.GetPlatformTextureSettings(p);sb.AppendLine("override "+p+" diffuse="+a.overridden+"/"+a.textureCompression+" normal="+b.overridden+"/"+b.textureCompression);}
            var sprites=AssetDatabase.LoadAllAssetsAtPath(Diffuse).OfType<Sprite>().OrderBy(x=>x.name).ToArray();
            sb.AppendLine("spriteCount="+sprites.Length);
            foreach(var sprite in sprites)sb.AppendLine("sprite="+sprite.name+" rect="+sprite.rect+" pivotPixels="+sprite.pivot+" PPU="+sprite.pixelsPerUnit);
            rig=UnityEngine.Object.FindAnyObjectByType<CharacterNormalLighting>();
            if(rig){var l=rig.movingLight;var pp=rig.testCamera.GetComponent<PixelPerfectCamera>();var so=new SerializedObject(l);sb.AppendLine("material="+rig.character.sharedMaterial.shader.name+" sortingLayer="+rig.character.sortingLayerID+" lightType="+l.lightType+" normalQuality="+l.normalMapQuality+" distance="+so.FindProperty("m_NormalMapDistance").floatValue+" intensity="+l.intensity+" color="+l.color+" inner="+l.pointLightInnerRadius+" outer="+l.pointLightOuterRadius);sb.AppendLine("targetSortingLayers="+string.Join(",",Enumerable.Range(0,so.FindProperty("m_ApplyToSortingLayers").arraySize).Select(i=>so.FindProperty("m_ApplyToSortingLayers").GetArrayElementAtIndex(i).intValue)));sb.AppendLine("cameraPPU="+pp.assetsPPU+" reference="+pp.refResolutionX+"x"+pp.refResolutionY+" grid="+pp.gridSnapping+" crop="+pp.cropFrame+" filter="+new SerializedObject(pp).FindProperty("m_FilterMode").enumValueIndex);}
            var pipeline=(UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            sb.AppendLine("pipeline="+AssetDatabase.GetAssetPath(pipeline)+" renderer="+pipeline.scriptableRenderer.GetType().FullName+" quality="+QualitySettings.GetQualityLevel()+" compileFailed="+EditorUtility.scriptCompilationFailed);
            File.WriteAllText(Root+"/IndependentAudit.txt",sb.ToString());
            UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(diffuse);UnityEngine.Object.DestroyImmediate(normal);
            return sb.ToString();
        }
        public static string Begin()
        {
            if(!EditorApplication.isPlaying||Running)throw new Exception("Need own playing scene and idle QA");
            rig=UnityEngine.Object.FindAnyObjectByType<CharacterNormalLighting>();if(!rig)throw new Exception("No rig");
            rig.enabled=false;rig.animate=false;rig.slowOrbit=false;
            rt=new RenderTexture(640,480,24,RenderTextureFormat.ARGB32){filterMode=FilterMode.Point,antiAliasing=1};rt.Create();rig.testCamera.targetTexture=rt;
            index=0;events=new StringBuilder("index,file,frame,normal,unityFrame,lightX,lightY,material,intensity,quality\n");Running=true;Configure();RenderPipelineManager.endCameraRendering+=Rendered;
            return "Started independent SRP capture61";
        }
        static void Configure(){if(index<48)rig.SetState(index/8,(index/2)%4,index%2==0);else rig.SetState(0,0,true,(index-48)*Mathf.PI/6);settle=0;}
        static void Rendered(ScriptableRenderContext context,Camera camera)
        {
            if(!Running||camera!=rig.testCamera||++settle<3)return;
            try{
                string name=index<48?"F"+index/8+"_"+D[(index/2)%4]+"_"+(index%2==0?"ON":"OFF"):"Orbit_"+(index-48).ToString("D2");
                var prev=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(640,480,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,640,480),0,0);tex.Apply();File.WriteAllBytes(Root+"/Captures/"+name+".png",tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);RenderTexture.active=prev;
                var l=rig.movingLight;events.AppendLine(index+","+name+","+rig.frame+","+rig.normals+","+Time.frameCount+","+l.transform.position.x.ToString(System.Globalization.CultureInfo.InvariantCulture)+","+l.transform.position.y.ToString(System.Globalization.CultureInfo.InvariantCulture)+","+rig.character.sharedMaterial.shader.name+","+l.intensity.ToString(System.Globalization.CultureInfo.InvariantCulture)+","+l.normalMapQuality);
                if(++index<61)Configure();else Finish();
            }catch(Exception e){File.WriteAllText(Root+"/CaptureFailure.txt",e.ToString());Finish();}
        }
        static void Finish(){Running=false;RenderPipelineManager.endCameraRendering-=Rendered;rig.testCamera.targetTexture=null;var finished=rt;rt=null;EditorApplication.delayCall+=()=>{if(RenderTexture.active==finished)RenderTexture.active=null;finished.Release();UnityEngine.Object.DestroyImmediate(finished);};File.WriteAllText(Root+"/CaptureEvents.csv",events.ToString());rig.enabled=true;rig.animate=true;rig.slowOrbit=true;Debug.Log("INDEPENDENT_QA_CAPTURE_COMPLETE "+index);}
        public static string Controls()
        {
            if(!EditorApplication.isPlaying||Running)throw new Exception("Need idle playing scene");
            rig=UnityEngine.Object.FindAnyObjectByType<CharacterNormalLighting>();var prior=Keyboard.current;var keyboard=InputSystem.AddDevice<Keyboard>();var sb=new StringBuilder();
            try{rig.enabled=false;rig.animate=false;rig.slowOrbit=false;rig.SetState(0,0,true);
                Action<Key> press=k=>{InputSystem.QueueStateEvent(keyboard,new KeyboardState(k));InputSystem.Update();rig.SendMessage("Update");InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();};
                var keys=new[]{Key.LeftArrow,Key.RightArrow,Key.UpArrow,Key.DownArrow};for(int i=0;i<4;i++){press(keys[i]);if(rig.direction!=i||rig.slowOrbit)throw new Exception("direction "+i);sb.AppendLine("PASS direction "+i);}
                press(Key.N);if(rig.normals||rig.movingLight.normalMapQuality!=Light2D.NormalMapQuality.Disabled)throw new Exception("N OFF");press(Key.N);if(!rig.normals||rig.movingLight.normalMapQuality!=Light2D.NormalMapQuality.Accurate)throw new Exception("N ON");sb.AppendLine("PASS N OFF/ON with same Lit material");
                for(int i=1;i<=6;i++){press(Key.Period);if(rig.frame!=i%6||rig.animate)throw new Exception("frame forward");}press(Key.Comma);if(rig.frame!=5)throw new Exception("frame backward wrap");sb.AppendLine("PASS all6 forward steps and backward wrap");
                press(Key.Space);if(!rig.animate)throw new Exception("Space");press(Key.O);if(!rig.slowOrbit)throw new Exception("O");sb.AppendLine("PASS Space animation and O slow orbit toggles");
                sb.AppendLine("Temporary Input System keyboard events processed by actual runtime Update; not human physical input.");
            }catch(Exception e){sb.AppendLine("FAIL "+e);throw;}finally{InputSystem.RemoveDevice(keyboard);if(prior!=null)prior.MakeCurrent();rig.enabled=true;File.WriteAllText(Root+"/IndependentControls.txt",sb.ToString());}return sb.ToString();
        }
        public static string Measure()
        {
            var sb=new StringBuilder();int bad=0,totalBlockErrors=0;long min=long.MaxValue,max=0;
            for(int f=0;f<6;f++)foreach(var dir in D){var a=Raw(Root+"/Captures/F"+f+"_"+dir+"_ON.png");var b=Raw(Root+"/Captures/F"+f+"_"+dir+"_OFF.png");var ap=a.GetPixels32();var bp=b.GetPixels32();int changed=0,blocks=0;long delta=0;for(int i=0;i<ap.Length;i++){int v=Math.Abs(ap[i].r-bp[i].r)+Math.Abs(ap[i].g-bp[i].g)+Math.Abs(ap[i].b-bp[i].b);delta+=v;if(v>3)changed++;}for(int y=192;y<288;y+=4)for(int x=272;x<368;x+=4){var c=ap[y*640+x];for(int yy=0;yy<4;yy++)for(int xx=0;xx<4;xx++)if(!c.Equals(ap[(y+yy)*640+x+xx]))blocks++;}totalBlockErrors+=blocks;if(changed==0)bad++;min=Math.Min(min,delta);max=Math.Max(max,delta);sb.AppendLine("F"+f+" "+dir+" changed="+changed+" RGBdelta="+delta+" nonuniform4x4pixels="+blocks);UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);}
            var first=Raw(Root+"/Captures/Orbit_00.png");var last=Raw(Root+"/Captures/Orbit_12.png");var fa=first.GetPixels32();var la=last.GetPixels32();int closure=0;for(int i=0;i<fa.Length;i++)if(!fa[i].Equals(la[i]))closure++;UnityEngine.Object.DestroyImmediate(first);UnityEngine.Object.DestroyImmediate(last);
            sb.AppendLine("pairs24 zeroDifferenceFailures="+bad+" deltaRange="+min+".."+max+" total4x4nonuniformPixels="+totalBlockErrors+" orbit360ClosureDifferentPixels="+closure);
            // Crops only: never computes or replaces lighting.
            var sheet=new Texture2D(768,576,TextureFormat.RGB24,false);sheet.SetPixels(Enumerable.Repeat(new Color(.06f,.07f,.09f),768*576).ToArray());
            for(int f=0;f<6;f++)for(int dir=0;dir<4;dir++)for(int state=0;state<2;state++){var t=Raw(Root+"/Captures/F"+f+"_"+D[dir]+"_"+(state==0?"ON":"OFF")+".png");for(int y=0;y<96;y++)for(int x=0;x<96;x++)sheet.SetPixel((dir*2+state)*96+x,(5-f)*96+y,t.GetPixel(272+x,192+y));UnityEngine.Object.DestroyImmediate(t);}sheet.Apply();File.WriteAllBytes(Root+"/AllFramesActualCrops.png",sheet.EncodeToPNG());UnityEngine.Object.DestroyImmediate(sheet);
            File.WriteAllText(Root+"/IndependentPixels.txt",sb.ToString());return sb.ToString();
        }
    }
}
