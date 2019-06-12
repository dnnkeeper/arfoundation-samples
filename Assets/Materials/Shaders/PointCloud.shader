Shader "Custom/PointCloud" {
    Properties{
	[HDR]
        _Color("PointCloud Color", Color) = (0.121, 0.737, 0.823, 1.0)
        
    }
    SubShader{
	Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100
		Blend One One
		
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float size : PSIZE;
                float4 center : TEXCOORD0;
                float2 radius : TEXCOORD1;
            };

            fixed4 _Color;
            
            v2f vert(appdata v, out float4 vertex : SV_POSITION)
            {
                v2f o;
                vertex = UnityObjectToClipPos(v.vertex);

                // Converts center.xy into [0,1] range then mutiplies them with screen size.
                o.center = ComputeScreenPos(vertex);
                o.center.xy /= o.center.w;
                o.center.x *= _ScreenParams.x;
                o.center.y *= _ScreenParams.y;

                o.size = 10.0;
                o.radius = 10.0;
                return o;
            }

            // vpos contains the integer coordinates of the current pixel, which is used
            // to caculate the distance between current pixel and center of the point.
            fixed4 frag(v2f i, UNITY_VPOS_TYPE vpos : VPOS) : SV_Target
            {

                float dis = distance(vpos.xy, i.center.xy);
                if (dis > i.radius.x / 2)
                {
                    discard;
                }
                return _Color;
            }
            ENDCG
        }
    }
}
