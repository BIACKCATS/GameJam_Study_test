using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;
using Hty.CharacterLighting;

namespace Hty.EditorTools
{
    public static class CharacterNormalTestBuilder
    {
        public const string Root = "Assets/hty";
        public const string Source = "Packages/com.febucci.text-animator-unity/Samples~/Use case examples/DamageNumbers/Assets/Textures/PlayerSpriteSheett.png";
        public const string Diffuse = Root + "/Textures/Player_Diffuse.png";
        public const string Normal = Root + "/Textures/Player_Normal.png";
        public const string Scene = Root + "/Scenes/CharacterNormalLighting_Test.unity";
        public const string Reports = Root + "/Reports";
        static string Hash(string p) { using (var h = SHA256.Create()) return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p))).Replace("-", "").ToLowerInvariant(); }
        static void Ensure(string dir) { if (!AssetDatabase.IsValidFolder(dir)) { var parent = Path.GetDirectoryName(dir).Replace('\\','/'); Ensure(parent); AssetDatabase.CreateFolder(parent, Path.GetFileName(dir)); } }

        [MenuItem("HTY/Character Lighting/Build isolated test")]
        public static void Build()
        {
            if (File.Exists(Reports+"/FROZEN.txt")) throw new Exception("Frozen QA artifact: coordinate before regeneration");
            if (Application.dataPath.Replace('\\','/').Equals("D:/Project/GameJam_Study_test/Assets",StringComparison.OrdinalIgnoreCase)) throw new Exception("Never drive original checkout");
            foreach (var d in new[] { Root, Root+"/Textures", Root+"/Materials", Root+"/Scenes", Reports, Reports+"/Captures" }) Ensure(d);
            if (!File.Exists(Diffuse)) File.Copy(Source, Diffuse);
            if (Hash(Source) != Hash(Diffuse)) throw new Exception("Diffuse must be byte-identical");
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            source.LoadImage(File.ReadAllBytes(Source));
            var normal = new Texture2D(96, 16, TextureFormat.RGBA32, false, true);
            // Hand interpreted shape regions in each 16px frame. Never derive depth from RGB/luminance.
            // Head/hair cap rows 8..12, torso 3..7, legs/arms otherwise. Flat centers keep the style restrained.
            for (int f=0; f<6; f++) for (int y=0; y<16; y++) for (int x=0; x<16; x++)
            {
                var a = source.GetPixel(f*16+x,y).a;
                Vector3 n = Vector3.forward;
                if (a > 0)
                {
                    if (y>=8 && y<=12 && x>=4 && x<=10)
                    {
                        float nx = x<=5 ? -.26f : x>=9 ? .26f : 0;
                        float ny = y>=11 ? .25f : y==8 ? -.12f : 0;
                        n = new Vector3(nx,ny,1).normalized;
                    }
                    else if (y>=3 && y<=7 && x>=4 && x<=10)
                    {
                        float nx = x<=5 ? -.17f : x>=9 ? .17f : 0;
                        float ny = y==7 ? .12f : y==3 ? -.10f : 0;
                        n = new Vector3(nx,ny,1).normalized;
                    }
                    else if (y<=2) n = new Vector3(x<7 ? -.10f : .10f,0,1).normalized;
                }
                normal.SetPixel(f*16+x,y,new Color(n.x*.5f+.5f,n.y*.5f+.5f,n.z*.5f+.5f,a));
            }
            normal.Apply(); File.WriteAllBytes(Normal,normal.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(source); UnityEngine.Object.DestroyImmediate(normal);
            AssetDatabase.ImportAsset(Normal);
            var ni=(TextureImporter)AssetImporter.GetAtPath(Normal);
            ni.textureType=TextureImporterType.NormalMap; ni.convertToNormalmap=false; ni.filterMode=FilterMode.Point;
            ni.mipmapEnabled=false; ni.textureCompression=TextureImporterCompression.Uncompressed; ni.alphaSource=TextureImporterAlphaSource.FromInput;
            ni.SaveAndReimport();
            AssetDatabase.ImportAsset(Diffuse);
            var di=(TextureImporter)AssetImporter.GetAtPath(Diffuse);
            di.textureType=TextureImporterType.Sprite; di.spriteImportMode=SpriteImportMode.Multiple; di.spritePixelsPerUnit=16;
            di.filterMode=FilterMode.Point; di.mipmapEnabled=false; di.textureCompression=TextureImporterCompression.Uncompressed; di.alphaIsTransparency=true;
            di.SaveAndReimport();
            var factory=new SpriteDataProviderFactories(); factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(di); provider.InitSpriteEditorDataProvider();
            var old=provider.GetSpriteRects();
            var rects=Enumerable.Range(0,6).Select(i=>new SpriteRect { name="PlayerSpriteSheett_"+i,rect=new Rect(i*16,0,16,16),pivot=new Vector2(.5f,.5f),alignment=SpriteAlignment.Center,
                spriteID=old.Length==6 ? old[i].spriteID : GUID.Generate() }).ToArray();
            provider.SetSpriteRects(rects);
            var names=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            names.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            provider.Apply();
            di.secondarySpriteTextures=new[] { new SecondarySpriteTexture { name="_NormalMap", texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Normal) } };
            di.SaveAndReimport();
            var shader=Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
            if (!shader) throw new Exception("URP Sprite Lit shader unavailable");
            var matPath=Root+"/Materials/Character_SpriteLit.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (!material) { material=new Material(shader); AssetDatabase.CreateAsset(material,matPath); }
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("HTY Test Camera 160x120").AddComponent<Camera>();
            camera.transform.position=new Vector3(0,0,-10); camera.orthographic=true; camera.orthographicSize=3.75f;
            camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.055f,.065f,.09f);
            camera.allowHDR=false; camera.allowMSAA=false;
            camera.gameObject.AddComponent<UniversalAdditionalCameraData>().SetRenderer(0);
            var pp=camera.gameObject.AddComponent<PixelPerfectCamera>(); pp.assetsPPU=16; pp.refResolutionX=160; pp.refResolutionY=120;
            pp.gridSnapping=PixelPerfectCamera.GridSnapping.UpscaleRenderTexture; pp.cropFrame=PixelPerfectCamera.CropFrame.Windowbox;
            var ppSettings=new SerializedObject(pp); ppSettings.FindProperty("m_FilterMode").enumValueIndex=(int)PixelPerfectCamera.PixelPerfectFilterMode.Point; ppSettings.ApplyModifiedPropertiesWithoutUndo();
            var controller=new GameObject("HTY Character Normal Test").AddComponent<CharacterNormalLighting>();
            controller.character=new GameObject("Actual Febucci Sample Character").AddComponent<SpriteRenderer>();
            controller.character.sharedMaterial=material;
            controller.frames=AssetDatabase.LoadAllAssetsAtPath(Diffuse).OfType<Sprite>().OrderBy(s=>s.name).ToArray();
            controller.testCamera=camera;
            var ambient=new GameObject("Mild Ambient").AddComponent<Light2D>(); ambient.lightType=Light2D.LightType.Global; ambient.intensity=.25f; ambient.color=Color.white;
            SetLight(ambient,"m_ApplyToSortingLayers",new[]{0});
            controller.movingLight=new GameObject("Moving Accurate Normal Light").AddComponent<Light2D>();
            var light=controller.movingLight; light.lightType=Light2D.LightType.Point; light.intensity=.65f; light.color=new Color(1,.96f,.9f);
            light.pointLightInnerRadius=2.4f; light.pointLightOuterRadius=4.5f;
            SetLight(light,"m_ApplyToSortingLayers",new[]{0}); SetLight(light,"m_NormalMapDistance",1.4f);
            controller.SetState(0,0,true); controller.animate=true;
            EditorSceneManager.SaveScene(scene,Scene); AssetDatabase.SaveAssets();
            File.WriteAllText(Reports+"/BuildFacts.txt", "Source: "+Source+"\nSHA256 source="+Hash(Source)+"\nSHA256 copy="+Hash(Diffuse)+"\nActual sample provenance: Febucci Text Animator 3.20.1 DamageNumbers sample, not original game art.\n96x16; six 16x16 rectangles x=0,16,32,48,64,80 y=0, center pivots, PPU16.\nTest resolution 160x120 logical, 640x480 capture (4x), not a global game standard.\nNormal PNG stores original alpha; normal importer may repack runtime alpha. Pixel normals are interpreted cap/torso/clothing regions, not luminance heights.\nON=Accurate/OFF=Disabled on same Light2D. Lit material, intensity/color/radius and ambient unchanged.\nURP Sprite-Lit baseline performs diffuse normal relighting, not PBR/specular gloss. No custom shader.\n");
            AssetDatabase.Refresh(); Debug.Log("HTY_BUILD_OK six sprites="+controller.frames.Length);
        }
        static void SetLight(Light2D light,string name,object value) { typeof(Light2D).GetField(name,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).SetValue(light,value); }
    }
}
