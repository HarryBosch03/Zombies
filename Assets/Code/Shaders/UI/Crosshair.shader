Shader "Unlit/Crosshair"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_ReticleSize("Reticle Size", float) = 0.1
		_Color("Color", Color) = (1, 0, 0, 1)
		_Brightness("Brightenss", float) = 1.0
		_Offset("Offset", float) = 100.0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		Blend One One
		Cull Back
		
		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

			struct Attributes
			{
				float4 vertex : POSITION;
			};

			struct Varyings
			{
				float4 localPos : VAR_LOCALPOS;
				float4 vertex : SV_POSITION;
			};

			float _ReticleSize;

			Varyings vert (Attributes input)
			{
				Varyings output;
				output.vertex = TransformObjectToHClip(input.vertex.xyz);
				output.localPos = input.vertex;
				return output;
			}

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			float4 _Color;
			float _Brightness;
			float _Offset;
			
			half4 frag (Varyings input) : SV_Target
			{
				float3 position = TransformObjectToWorld(input.localPos);
				float3 viewDir = normalize(position - _WorldSpaceCameraPos);
				position += viewDir * _Offset;
				position = TransformWorldToObject(position);
				
				float2 uv = (position.xy / (_ReticleSize * _Offset * 0.01)) * 0.5 + 0.5;
				
				half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv.xy);
				
				return float4(_Color.rgb * tex.rgb * tex.a * max(0, _Brightness), _Color.a * tex.a * tex.a);
			}
			ENDHLSL
		}
	}
}