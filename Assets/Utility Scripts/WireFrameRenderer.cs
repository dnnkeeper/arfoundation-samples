using UnityEngine;
using System.Collections;

public class WireFrameRenderer : MonoBehaviour {

	public Color lineColor; 
	//public Color backgroundColor; 
	//public bool ZWrite = true; 
	//public bool AWrite = true; 
	//public bool blend = true; 
	
	private Vector3[] lines; 
	private ArrayList linesArray; 
	public Material lineMaterial;
    new Collider collider;
	//private MeshRenderer meshRenderer; 

	// Use this for initialization
	void Start () {
        collider = GetComponent<Collider>();
        Init();
    }
    bool inited;
    public void Init()
    {
        inited = false;
        var meshRenderer = GetComponent<MeshRenderer>(); 
        //if(meshRenderer != null){
            //meshRenderer.enabled = false;
        //}
        //meshRenderer.material = new Material("Shader \"Lines/Background\" { Properties { _Color (\"Main Color\", Color) = (1,1,1,1) } SubShader { Pass {" + (ZWrite ? " ZWrite on " : " ZWrite off ") + (blend ? " Blend SrcAlpha OneMinusSrcAlpha" : " ") + (AWrite ? " Colormask RGBA " : " ") + "Lighting Off Offset 1, 1 Color[_Color] }}}"); 

        // Old Syntax without Bind :    
        //   lineMaterial = new Material("Shader \"Lines/Colored Blended\" { SubShader { Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite On Cull Front Fog { Mode Off } } } }"); 

        // New Syntax with Bind : 
        //lineMaterial = new Material("Shader \"Lines/Colored Blended\" { SubShader { Pass { Blend SrcAlpha OneMinusSrcAlpha BindChannels { Bind \"Color\",color } ZWrite On Cull Front Fog { Mode Off } } } }"); 

        //lineMaterial.hideFlags = HideFlags.HideAndDontSave; 
        //lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave; 
        
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null)
        {
            Mesh mesh = filter.sharedMesh;
            if (mesh == null || !mesh.isReadable)
                return;

            inited = true;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            linesArray = new ArrayList();

            for (int i = 0; i < triangles.Length / 3; i++)
            {
                linesArray.Add(vertices[triangles[i * 3]]);
                linesArray.Add(vertices[triangles[i * 3 + 1]]);
                linesArray.Add(vertices[triangles[i * 3 + 2]]);
            }

            lines = new Vector3[triangles.Length];
            for (int i = 0; i < triangles.Length; i++)
            {
                lines[i] = (Vector3)linesArray[i];
            }
        }
    }

	void OnRenderObject() 
	{    
        if (!inited)
        {
            return;
        }
		//meshRenderer.sharedMaterial.color = backgroundColor; 
		lineMaterial.SetPass(0); 
		
		GL.PushMatrix(); 
		GL.MultMatrix(transform.localToWorldMatrix); 
		GL.Begin(GL.LINES); 
		GL.Color(lineColor); 
		
		for (int i = 0; i < lines.Length / 3; i++) 
		{ 
			GL.Vertex(lines[i * 3]); 
			GL.Vertex(lines[i * 3 + 1]); 
			
			GL.Vertex(lines[i * 3 + 1]); 
			GL.Vertex(lines[i * 3 + 2]); 
			
			GL.Vertex(lines[i * 3 + 2]); 
			GL.Vertex(lines[i * 3]); 
		} 
		
		GL.End(); 
		GL.PopMatrix(); 
	}
	
	// Update is called once per frame
	void Update () {
	    if (collider != null)
        {
            if (!collider.enabled)
                inited = false;
        }
	}
}
