using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    public class UpgradeStandardShaderMaterials
    {
        static List<MaterialUpgrader> GetHDUpgraders()
        {
            var upgraders = new List<MaterialUpgrader>();
            upgraders.Add(new StandardsToHDLitMaterialUpgrader("Standard", "HDRP/Lit"));
            upgraders.Add(new StandardsToHDLitMaterialUpgrader("Standard (Specular setup)", "HDRP/Lit"));
            upgraders.Add(new StandardsToHDLitMaterialUpgrader("Standard (Roughness setup)", "HDRP/Lit"));

            upgraders.Add(new UnlitsToHDUnlitUpgrader("Unlit/Color", "HDRP/Unlit"));
            upgraders.Add(new UnlitsToHDUnlitUpgrader("Unlit/Texture", "HDRP/Unlit"));
            upgraders.Add(new UnlitsToHDUnlitUpgrader("Unlit/Transparent", "HDRP/Unlit"));
            upgraders.Add(new UnlitsToHDUnlitUpgrader("Unlit/Transparent Cutout", "HDRP/Unlit"));
            return upgraders;
        }

        [MenuItem("Edit/Render Pipeline/Upgrade Project Materials to High Definition Materials", priority = CoreUtils.editMenuPriority2)]
        static void UpgradeMaterialsProject()
        {
            MaterialUpgrader.UpgradeProjectFolder(GetHDUpgraders(), "Upgrade to HD Material");
        }

        [MenuItem("Edit/Render Pipeline/Upgrade Selected Materials to High Definition Materials", priority = CoreUtils.editMenuPriority2)]
        static void UpgradeMaterialsSelection()
        {
            MaterialUpgrader.UpgradeSelection(GetHDUpgraders(), "Upgrade to HD Material");
        }

        [MenuItem("Edit/Render Pipeline/Upgrade Scene Light Intensity for High Definition", priority = CoreUtils.editMenuPriority2)]
        static void UpgradeLights()
        {
            Light[] lights = Light.GetLights(LightType.Directional, 0);
            foreach (var l in lights)
            {
                l.intensity *= Mathf.PI;
            }
        }
    }
}
