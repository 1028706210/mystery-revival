using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MysteryRevival
{
    public static class DoorIntegrationChecks
    {
        public static void Validate()
        {
            ValidateSprite("Assets/Resources/door_game_frame.png");
            ValidateSprite("Assets/Resources/door_game_leaf.png");
            Debug.Log("DOOR_ASSET_CHECK=PASS");
        }

        static void ValidateSprite(string path)
        {
            string fullPath=Path.Combine(Application.dataPath,path.Substring("Assets/".Length));
            if(!File.Exists(fullPath))throw new Exception("Missing door resource: "+path);

            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null)throw new Exception("Missing TextureImporter: "+path);
            Check(importer.textureType==TextureImporterType.Sprite,path+" texture type");
            Check(importer.spriteImportMode==SpriteImportMode.Single,path+" single sprite mode");
            Check(Near(importer.spritePixelsPerUnit,100),path+" PPU 100");
            Check(Vector2.Distance(importer.spritePivot,new Vector2(.5f,.05f))<.0001f,path+" pivot (0.5, 0.05)");
            Check(importer.filterMode==FilterMode.Point,path+" point filtering");
            Check(!importer.mipmapEnabled,path+" mipmaps disabled");
            Check(importer.textureCompression==TextureImporterCompression.Uncompressed,path+" uncompressed import");
            Check(importer.alphaIsTransparency,path+" alpha transparency");

            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(texture==null)throw new Exception("Texture import failed: "+path);
            Check(texture.width==128&&texture.height==160,path+" imported Texture2D 128x160");
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(sprite==null)throw new Exception("Sprite import failed: "+path);
            Check(sprite.name==Path.GetFileNameWithoutExtension(path),path+" sprite name");
            Check(Near(sprite.rect.width,128)&&Near(sprite.rect.height,160),path+" Sprite 128x160");
            Check(Near(sprite.pixelsPerUnit,100),path+" imported PPU 100");
            Check(Near(sprite.pivot.x,64)&&Near(sprite.pivot.y,8),path+" imported pivot");

            var decoded=new Texture2D(2,2);
            try {
                Check(ImageConversion.LoadImage(decoded,File.ReadAllBytes(fullPath)),path+" PNG decode");
                Check(decoded.width==128&&decoded.height==160,path+" PNG 128x160");
            } finally {
                UnityEngine.Object.DestroyImmediate(decoded);
            }
        }

        static bool Near(float actual,float expected){return Mathf.Abs(actual-expected)<.001f;}
        static void Check(bool value,string name){if(!value)throw new Exception("DOOR_ASSET_CHECK=FAIL "+name);}
    }
}
