using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    [System.Serializable]
    public class MaterialShaderReplacementInfo
    {
        public Material material;
        public Shader shader;
        Shader originalShader;

        public void SetReplacementShader(Shader shader)
        {
            originalShader = material.shader;
            material.shader = shader;
        }

        public void ResetShader()
        {
            material.shader = originalShader;
        }
    }

    public class MaterialShaderReplacement : MonoBehaviour
    {
        public List<MaterialShaderReplacementInfo> materials;

        bool replaced;

        public void OnEnable()
        {
            replaced = true;
            foreach (var m in materials)
            {
                m.SetReplacementShader(m.shader);
            }
        }

        public void OnDisable()
        {
            replaced = false;
            foreach (var m in materials)
            {
                m.ResetShader();
            }
        }

        public void Toggle()
        {
            if (replaced)
            {
                OnDisable();
            }
            else
            {
                OnEnable();
            }
        }

        public void ReplaceMaterialShader(MaterialShaderReplacementInfo m, Shader s)
        {
            m.shader = s;
        }
    }
}