Shader "Custom/DotsAroundHitPosition"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		_Emission ("Emission", Range(0,1)) = 0.0
		//_BumpMap ("DotsNormalmap", 2D) = "bump" {}
        //_MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
		//_RaycastHitPos("_RaycastHitPos", Vector) = (0,0,0,0)
		_Radius ("Fade Radius", Range(0.01,1.0)) = 1.0
		_DotsRadius ("Dots Scale", Range(0.01,1.0)) = 1.0
		//_Pow ("Pow", Range(0.01,10)) = 2.0
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue" = "AlphaTest"  } 
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:auto //AlphaTest:_Cutoff

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
		//sampler2D _BumpMap;

        struct Input
        {
            float2 uv_MainTex;
			float2 uv2_MainTex;
			float3 worldPos;
			float4 color : COLOR;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		uniform fixed4 _RaycastHitPos;
		half _Emission, _Radius, _DotsRadius, _Pow, _Cutoff;
		float _ShortestUVMapping;
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
			
			fixed2 uv = IN.worldPos.xz/_DotsRadius ;
			uv = round(uv) - uv;
            fixed dotRadius =  (1.0 - length(uv)) * _Color.a;
			float distanceToHitPosition = 
			//length(IN.uv_MainTex-0.5)/_Radius;
			saturate( length(IN.worldPos - _RaycastHitPos) / _Radius );

			//dotRadius = dotRadius * max(1.0, pow(1.0-distanceToHitPosition, 4.0)*1.5);

            o.Albedo = _Color * dotRadius * IN.color; //(1.0-pow(distanceToHitPosition, _Pow))*c.a; //
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
			
			half fadeRadius = (1.0 - pow(distanceToHitPosition, 2.0) );
            half alpha = (dotRadius) * fadeRadius;
			
			
			// Fade out from as we pass the edge.
			// uv2.x stores a mapped UV that will be "1" at the beginning of the feathering.
			// We fade until we reach at the edge of the shortest UV mapping.
			// This is the remmaped UV value at the vertex.
			// We choose the shorted one so that ll edges will fade out completely.
			// See ARFeatheredPlaneMeshVisualizer.cs for more details.
			alpha *= 1-smoothstep(1, _ShortestUVMapping, IN.uv2_MainTex.x * 0.5);

			alpha -= _Cutoff ;

			clip(alpha);
			
			o.Normal = (half3(uv.x, uv.y,  1.0-(uv.x*uv.x)-(uv.y*uv.y) )); //UnpackNormal(tex2D(_BumpMap, uv));
			o.Emission = o.Albedo * alpha * _Emission * max(1.0, pow(1.0-distanceToHitPosition, 5.0)*200);
			o.Alpha = 1.0;
			
        }
        ENDCG
    }
    //FallBack "Diffuse"
}
