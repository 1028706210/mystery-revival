using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MysteryRevival
{
    public static class CorridorWindowChecks
    {
        const string Path="Assets/Resources/corridor_window_wall.png";

        public static void Validate()
        {
            string fullPath=System.IO.Path.Combine(Application.dataPath,Path.Substring("Assets/".Length));
            if(!File.Exists(fullPath))throw new Exception("Missing corridor window resource: "+Path);

            var importer=AssetImporter.GetAtPath(Path) as TextureImporter;
            if(importer==null)throw new Exception("Missing TextureImporter: "+Path);
            Check(importer.textureType==TextureImporterType.Sprite,"texture type Sprite");
            Check(importer.spriteImportMode==SpriteImportMode.Single,"single sprite mode");
            Check(Near(importer.spritePixelsPerUnit,100),"PPU 100");
            Check(Vector2.Distance(importer.spritePivot,new Vector2(.5f,8f/176f))<.0001f,"pivot (0.5, 8/176)");
            Check(importer.filterMode==FilterMode.Point,"point filtering");
            Check(importer.wrapMode==TextureWrapMode.Clamp,"clamp wrapping");
            Check(!importer.mipmapEnabled,"mipmaps disabled");
            Check(importer.textureCompression==TextureImporterCompression.Uncompressed,"uncompressed import");
            Check(importer.alphaIsTransparency,"alpha transparency");

            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Path);
            if(texture==null)throw new Exception("Texture import failed: "+Path);
            Check(texture.width==192&&texture.height==176,"imported Texture2D 192x176");
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Path);
            if(sprite==null)throw new Exception("Sprite import failed: "+Path);
            Check(sprite.name=="corridor_window_wall","sprite name");
            Check(Near(sprite.rect.width,192)&&Near(sprite.rect.height,176),"Sprite 192x176");
            Check(Near(sprite.pixelsPerUnit,100),"imported PPU 100");
            Check(Near(sprite.pivot.x,96)&&Near(sprite.pivot.y,8),"imported pivot 96,8");

            var decoded=new Texture2D(2,2);
            try {
                Check(ImageConversion.LoadImage(decoded,File.ReadAllBytes(fullPath)),"PNG decode");
                Check(decoded.width==192&&decoded.height==176,"PNG 192x176");
            } finally {
                UnityEngine.Object.DestroyImmediate(decoded);
            }

            Debug.Log("WINDOW_ASSET_CHECK=PASS");
        }

        static bool Near(float actual,float expected){return Mathf.Abs(actual-expected)<.001f;}
        static void Check(bool value,string name){if(!value)throw new Exception("WINDOW_ASSET_CHECK=FAIL "+name);}
    }
}
