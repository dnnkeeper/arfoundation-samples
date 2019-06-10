
Shader "Custom/DepthMaskWithShadow" {

	Properties{
	}
	SubShader
	{
		Tags {"Queue" = "Geometry-10" }
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
			Tags {"LightMode" = "ForwardBase" }
			
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			struct v2f
			{
				float4 pos : SV_POSITION;
				LIGHTING_COORDS(0,1)
			};
			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(o);

				return o;
			}
			fixed4 frag(v2f i) : COLOR
			{
				float attenuation = LIGHT_ATTENUATION(i);
				float col = saturate(0.5-attenuation);
				return fixed4(0,0,0,col);
			}
			ENDCG
		}
		Pass
		{
			Tags { "LightMode"="ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"
			struct v2f {
				V2F_SHADOW_CASTER;
			};
			v2f vert(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}
			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}
