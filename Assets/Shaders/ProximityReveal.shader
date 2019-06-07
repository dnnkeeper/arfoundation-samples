Shader "Custom/ProximityReveal"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		_Emission ("Emission", Range(0,1)) = 0.0
        //_MainTex ("Albedo (RGB)", 2D) = "white" {}
        //_Glossiness ("Smoothness", Range(0,1)) = 0.5
        //_Metallic ("Metallic", Range(0,1)) = 0.0
		//_RaycastHitPos("_RaycastHitPos", Vector) = (0,0,0,0)
		_Radius ("Radius", Range(0.01,1)) = 1.0
		_DotsRadius ("Dots Radius", Range(0.01,10)) = 5.0
		//_Pow ("Pow", Range(0.01,10)) = 2.0
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
		
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue" = "AlphaTest"  } //Geometry-10
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Lambert fullforwardshadows //alpha:auto //AlphaTest:_Cutoff

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
			float3 worldPos;
			float4 color : COLOR;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		uniform fixed4 _RaycastHitPos;
		half _Emission, _Radius, _DotsRadius, _Pow, _Cutoff;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
			fixed2 uv = IN.worldPos.xz/_DotsRadius ;
            fixed4 c = _Color * (1.0 - length( round(uv) - uv )) * IN.color; //tex2D (_MainTex, IN.worldPos.xz) * _Color;
			//c.a = pow(c.a, 2.0);
			float distance = 
			//length(IN.uv_MainTex-0.5)/_Radius;
			saturate( length(IN.worldPos - _RaycastHitPos) / _Radius );
            o.Albedo = c.rgb; //(1.0-pow(distance, _Pow))*c.a; //
            // Metallic and smoothness come from slider variables
            //o.Metallic = _Metallic;
            //o.Smoothness = _Glossiness;
            o.Alpha = (1.0-pow(distance, 8.0))*c.a - _Cutoff;
			o.Emission = c * _Emission;
			clip(o.Alpha);
        }
        ENDCG
    }
    //FallBack "Diffuse"
}
