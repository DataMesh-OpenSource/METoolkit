// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/sd_AlphaTex_Nolighting" 
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
			_Emission("Emission",Range(0,1)) = 0.4
				_Alpha("Alpha",Range(0,1)) = 1
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" "RenderQueue" = "3000" }
		//cull off
		ZWrite Off
		LOD 200

		CGPROGRAM

		#pragma vertex vert		
		#pragma surface surf NoLighting alpha

		#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		half _TotalAlpha;
		fixed _Emission;
		fixed _Alpha;

		struct Input 
		{
			float2 uv_MainTex;
		};

		void vert(inout appdata_full v,out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
		}
		//SurfaceOutputStandard
		void surf(Input IN, inout SurfaceOutput o) {

			fixed4 color_tex = tex2D(_MainTex, IN.uv_MainTex)*_Color;
			o.Albedo = color_tex.rgb;
			o.Alpha = color_tex.a;
			o.Emission = _Emission;
		}

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha*_Alpha;		
			return c;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
