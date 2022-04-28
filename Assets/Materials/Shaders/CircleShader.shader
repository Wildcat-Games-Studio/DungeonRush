Shader "Unlit/CircleShader"
{
	Properties
	{
		_EdgeSoftness("Edge Softness", Range(0.0, 1.0)) = 0.1
		_Color("Color", Color) = (0.0, 0.0, 0.0, 1.0)
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		//Blend SrcAlpha OneMinusSrcAlpha
		Blend SrcAlpha One
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			//float Radius;
			float _EdgeSoftness;
			fixed4 _Color;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float center_dst = distance(i.uv, float2(0.5, 0.5));

				float t1 = 1.0 - smoothstep(0.5 - _EdgeSoftness, 0.5, center_dst);
				fixed4 col = fixed4(_Color.rgb, _Color.a * t1);
				return col;
			}
			ENDCG
		}
	}
}
