using UnityEngine;

public class SetGlobalShaderPosition : MonoBehaviour
{
    public string variableName = "_HitPos";
    public void LateUpdate()
    {
        Shader.SetGlobalVector(variableName, transform.position);
    }
}
