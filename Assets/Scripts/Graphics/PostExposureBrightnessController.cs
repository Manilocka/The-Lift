using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

// Реальное изменение яркости через URP Volume -> ColorAdjustments.postExposure (без прямой зависимости от URP)
public class PostExposureBrightnessController : MonoBehaviour
{
    object volume;           // UnityEngine.Rendering.Volume
    object profile;          // VolumeProfile
    object colorAdjustments; // UnityEngine.Rendering.Universal.ColorAdjustments
    Type volumeType;
    Type profileType;
    Type colorAdjType;

    void Awake()
    {
        TryResolveTypes();
        FindVolumeWithColorAdjustments();
    }

    void TryResolveTypes()
    {
        volumeType = Type.GetType("UnityEngine.Rendering.Volume, Unity.RenderPipelines.Core.Runtime")
                     ?? Type.GetType("UnityEngine.Rendering.Volume, Unity.RenderPipelines.Universal.Runtime");
        profileType = Type.GetType("UnityEngine.Rendering.VolumeProfile, Unity.RenderPipelines.Core.Runtime")
                      ?? Type.GetType("UnityEngine.Rendering.VolumeProfile, Unity.RenderPipelines.Universal.Runtime");
        colorAdjType = Type.GetType("UnityEngine.Rendering.Universal.ColorAdjustments, Unity.RenderPipelines.Universal.Runtime");
    }

    void FindVolumeWithColorAdjustments()
    {
        if (volumeType == null || colorAdjType == null) return;
        var volumes = FindObjectsOfType(typeof(MonoBehaviour))
            .Where(mb => mb != null && volumeType.IsAssignableFrom(mb.GetType()))
            .ToArray();
        foreach (var v in volumes)
        {
            var profProp = volumeType.GetProperty("profile");
            profile = profProp?.GetValue(v);
            if (profile == null) continue;
            var componentsField = profileType.GetField("components");
            var list = componentsField?.GetValue(profile) as System.Collections.IEnumerable;
            if (list == null) continue;
            foreach (var comp in list)
            {
                if (comp != null && colorAdjType.IsAssignableFrom(comp.GetType()))
                {
                    volume = v;
                    colorAdjustments = comp;
                    return;
                }
            }
        }
    }

    // brightness [0..100] -> постэкспозиция [-2..+1] (примерный маппинг)
    public void Apply(float brightness)
    {
        if (colorAdjustments == null || colorAdjType == null) return;
        float t = Mathf.Clamp01(brightness / 100f);
        float postExp = Mathf.Lerp(-2f, 1f, t);
        var postExposureField = colorAdjType.GetField("postExposure");
        var minMaxField = colorAdjType.GetField("active");
        if (postExposureField == null) return;
        var clampedFloatType = postExposureField.FieldType; // FloatParameter
        var valueProp = clampedFloatType.GetProperty("value");
        var paramInstance = postExposureField.GetValue(colorAdjustments);
        if (paramInstance != null && valueProp != null)
        {
            valueProp.SetValue(paramInstance, postExp);
        }
    }
}


