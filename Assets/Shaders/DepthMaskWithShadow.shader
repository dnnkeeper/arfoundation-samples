// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//===============================================================================
//Copyright (c) 2015 PTC Inc. All Rights Reserved.
//
//Confidential and Proprietary - Protected under copyright and other laws.
//Vuforia is a trademark of PTC Inc., registered in the United States and other
//countries.
//===============================================================================
//===============================================================================
//Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
//All Rights Reserved.
//Confidential and Proprietary - Qualcomm Connected Experiences, Inc.
//===============================================================================

Shader "Custom/DepthMaskWithShadow" {

	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}//Needed by the Diffuse Fallback
		_Color("Main Color", Color) = (1,1,1,.5)
		_ShadowIntensity("Shadow Intensity", Range(0, 1)) = 0.6
			_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
	}
	//SubShader
	//{
	//	Tags { "RenderType" = "Opaque" }
	//	//LOD 200

	//	Pass
	//	{
	//		Tags { "LightMode" = "ForwardBase" }
	//		CGPROGRAM
	//		#pragma vertex vert
	//		#pragma fragment frag
	//		#pragma multi_compile_fwdbase 
	//		#include "UnityCG.cginc"
	//		#include "AutoLight.cginc"

	//		struct vertOut {
	//			float4 pos : SV_POSITION;
	//			LIGHTING_COORDS(0,1) // NEEDED FOR SHADOWS.
	//		};

	//		vertOut vert(appdata_base v)
	//		{
	//			vertOut o;
	//			o.pos = UnityObjectToClipPos(v.vertex);
	//			TRANSFER_VERTEX_TO_FRAGMENT(o); // NEEDED FOR SHADOWS.
	//			return o;
	//		}

	//		float4 frag(vertOut i) :COLOR
	//		{
	//			fixed atten = LIGHT_ATTENUATION(i);// NEEDED FOR SHADOWS.
	//			float4 c;
	//			c = float4(0,1,0,1) * atten;
	//			return c;
	//		}
	//		ENDCG
	//	}//Pass
	//	
	//	
	//	// Pass to render object as a shadow caster
	//	Pass 
	//	{
	//		Name "ShadowCaster"
	//		Tags { "LightMode" = "ShadowCaster" }

	//		Fog {Mode Off}
	//		ZWrite On ZTest LEqual Cull Off
	//		Offset 1, 1

	//		CGPROGRAM
	//		#pragma vertex vert
	//		#pragma fragment frag
	//		#pragma multi_compile_shadowcaster
	//		#pragma fragmentoption ARB_precision_hint_fastest
	//		#include "UnityCG.cginc"
	//		#include "AutoLight.cginc"

	//		struct v2f {
	//			V2F_SHADOW_CASTER;
	//		};

	//		v2f vert(appdata_base v)
	//		{
	//			v2f o;
	//			TRANSFER_SHADOW_CASTER(o)
	//			return o;
	//		}

	//		float4 frag(v2f i) : COLOR
	//		{
	//			SHADOW_CASTER_FRAGMENT(i)
	//		}
	//		ENDCG
	//	} //Pass
	//	
	//	// Pass to render object as a shadow collector
	//	Pass 
	//	{
	//		Name "ShadowCollector"
	//		Tags { "LightMode" = "ShadowCollector" }

	//		Fog {Mode Off}
	//		ZWrite On ZTest LEqual

	//		CGPROGRAM
	//		#pragma vertex vert
	//		#pragma fragment frag
	//		#pragma fragmentoption ARB_precision_hint_fastest
	//		#pragma multi_compile_shadowcollector

	//		#define SHADOW_COLLECTOR_PASS
	//		#include "UnityCG.cginc"

	//		struct appdata {
	//			float4 vertex : POSITION;
	//		};

	//		struct v2f {
	//			V2F_SHADOW_COLLECTOR;
	//		};

	//		v2f vert(appdata v)
	//		{
	//			v2f o;
	//			TRANSFER_SHADOW_COLLECTOR(o)
	//			return o;
	//		}

	//		fixed4 frag(v2f i) : COLOR
	//		{
	//			SHADOW_COLLECTOR_FRAGMENT(i)
	//		}
	//	ENDCG

	//	}//Pass

	//} 
	SubShader
	{

		Tags {"Queue" = "Geometry-10" } // "RenderType" = "TransparentCutout" }
		
		ZTest LEqual
		ZWrite On
		Cull Back

		Stencil
		{
			Ref 2
			Comp notequal
			Pass keep
		}

		Pass
		{
			Tags {"LightMode" = "ForwardBase" } // "RenderType" = "TransparentCutout"}
			//Alphatest Greater [_Cutoff]
			//AlphaToMask On
			Blend OneMinusSrcColor SrcColor
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			uniform fixed4  _Color;
			uniform float _ShadowIntensity;
			uniform sampler2D _MainTex;

			uniform float4 _MainTex_ST;

			uniform float4 _MainTex_TexelSize;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				LIGHTING_COORDS(0,1)
			};
			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
				TRANSFER_VERTEX_TO_FRAGMENT(o);

				return o;
			}
			fixed4 frag(v2f i) : COLOR
			{
				fixed4 c = tex2D(_MainTex, i.uv);
				float attenuation = LIGHT_ATTENUATION(i);
				fixed3 col = attenuation;// *_ShadowIntensity * _Color;
				return fixed4(col.r, col.g, col.b, c.a);
			}
			ENDCG
		}

	}

	FallBack "Legacy Shaders/Transparent/Cutout/Diffuse" //note: required for passes: ForwardBase, ShadowCaster, ShadowCollector
}
