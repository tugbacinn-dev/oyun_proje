using UnityEditor;
using UnityEngine;

namespace PatininIzinde.EditorTools
{
    public static class MaterialFixTools
    {
        private const string CityPeopleMaterialsPath = "Assets/DenysAlmaral/CityPeople/Materials";
        private const string WorkingCityMaterialPath = "Assets/CubexCube - Free City Pack I/Materials/Texture_1.mat";

        [MenuItem("Pati'nin Izinde/Kurulum/CityPeople Materyallerini URP Duzelt")]
        public static void FixCityPeopleMaterials()
        {
            Shader targetShader = FindCompatibleLitShader();
            if (targetShader == null)
            {
                EditorUtility.DisplayDialog(
                    "Shader bulunamadi",
                    "Uygun Lit/Standard shader bulunamadi. Materyalleri elle kontrol etmek gerekir.",
                    "Tamam");
                return;
            }

            string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { CityPeopleMaterialsPath });
            int fixedCount = 0;

            foreach (string guid in materialGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    continue;
                }

                Undo.RecordObject(material, "Fix CityPeople Materials");

                Color baseColor = Color.white;
                Texture mainTexture = null;

                if (material.HasProperty("_Color"))
                {
                    baseColor = material.GetColor("_Color");
                }

                if (material.HasProperty("_MainTex"))
                {
                    mainTexture = material.GetTexture("_MainTex");
                }

                material.shader = targetShader;

                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", baseColor);
                }

                if (mainTexture != null && material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", mainTexture);
                }

                EditorUtility.SetDirty(material);
                fixedCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Materyaller duzeltildi",
                $"{fixedCount} CityPeople materyali {targetShader.name} shader'a alindi.",
                "Tamam");
        }

        private static Shader FindCompatibleLitShader()
        {
            string[] shaderNames =
            {
                "Universal Render Pipeline/Lit",
                "Universal Render Pipeline/Simple Lit",
                "Universal Render Pipeline/Unlit",
                "Standard",
                "Unlit/Texture",
                "Unlit/Color"
            };

            foreach (string shaderName in shaderNames)
            {
                Shader shader = Shader.Find(shaderName);
                if (shader != null)
                {
                    return shader;
                }
            }

            Material workingMaterial = AssetDatabase.LoadAssetAtPath<Material>(WorkingCityMaterialPath);
            return workingMaterial != null ? workingMaterial.shader : null;
        }
    }
}
