Shader "Custom/LowPolyWater" {
	Properties {
		_Color ("Color", Color) = (0, 0.2706, 1, 0.6706)
		_WireColor("Wire color", Color) = (0.149, 0, 1, 1)
		_FoamColor("Foam color", Color) = (1, 1, 1, 1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5

		_NoiseScale("Noise scale", float) = 0.01
		_Height("Height", float) = 0.1
		_Period("Wave period", float) = 25
		_Thickness("Thickness", float) = 20
		_FoamDistance("Foam distance", float) = 5
		_DepthDistance("Depth distance", Range(0, 1)) = 0
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 200
		
		Cull Off

		CGPROGRAM
		
		#pragma surface surf Standard fullforwardshadows vertex:vert alpha
		#pragma target 3.0

		#include "SimplexNoise2D.cginc"

		struct Input {
			float3 oPos;
			float4 screenPos;
			float4 color : COLOR;
		};

		float _Glossiness;
		float _Period;
		float _Height;
		float _NoiseScale;
		float _Thickness;
		float _FoamDistance;
		float _DepthDistance;

		float4 _Color,
			_WireColor,
			_FoamColor;

		sampler2D _CameraDepthTexture;

		UNITY_INSTANCING_CBUFFER_START(Props)
		UNITY_INSTANCING_CBUFFER_END

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			v.vertex.y += snoise((v.vertex.xz + _Time.x / _Period) / _NoiseScale) * _Height;
			o.oPos = v.vertex.xyz;
		}
		
		float edgeFactor(float3 color, float depth)
		{
			float3 d = abs(ddx(color)) + abs(ddy(color));
			float3 a3 = smoothstep(0,
				d * _Thickness / depth,
				color);

			return min(min(a3.x, a3.y), a3.z);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float screenDepth = LinearEyeDepth(
				SAMPLE_DEPTH_TEXTURE_PROJ(
					_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos)));
			float foam = saturate(abs(screenDepth - IN.screenPos.w) / _FoamDistance);

			float depthFactor = saturate(IN.screenPos.w / 30) * (1 - _DepthDistance);
			float wire = edgeFactor(IN.color.xyz, IN.screenPos.w);
			float4 c = lerp(_FoamColor, lerp(_WireColor, _Color, saturate(wire + depthFactor)), foam);

			o.Albedo = c.rgb;
			o.Normal = 
				cross(normalize(ddx(IN.oPos)), normalize(ddy(IN.oPos)));
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
}
