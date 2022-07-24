Shader "Custom/BlurUI_HDRP"
{
	Properties
	{
		[PerRendererData] _MainTex ("_MainTex", 2D) = "white" {}
		_AltTexture("_AltTexture", 2D) = "white" {}
		_Iterations("_Iterations", Range(0,100)) = 25
		_LoopIteration("_LoopIteration", Range(0,10)) = 10
		_Lightness ("_Lightness", Range(0,2)) = 1
        _Saturation ("_Saturation", Range(-10,10)) = 1
		_TintColor ("_TintColor",Color) = (1.0,1.0,1.0,0.0)

		[Toggle]
		_AlphaIsBlurAmount ("_AlphaIsBlurAmount",Float) = 0

		 // required for UI.Mask
         [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
         [HideInInspector] _Stencil ("Stencil ID", Float) = 0
         [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
         [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
         [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
         [HideInInspector] _ColorMask ("Color Mask", Float) = 15
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"PreviewType" = "Plane"
			"DisableBatching" = "True"
		}

		// required for UI.Mask
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        ColorMask [_ColorMask]
		//

		Pass
		{
			ZWrite Off
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				half4 color : COLOR;
				half4 screenpos : TEXCOORD2;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 screenuv : TEXCOORD1;
				half4 color : COLOR;
				float2 screenpos : TEXCOORD2;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.screenuv = ((o.vertex.xy / o.vertex.w) + 1) * 0.5;
				o.color = v.color;
				o.screenpos = ComputeScreenPos(o.vertex);
				return o;
			}

			float2 safemul(float4x4 M, float4 v)
			{
				float2 r;

				r.x = dot(M._m00_m01_m02, v);
				r.y = dot(M._m10_m11_m12, v);

				return r;
			}

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			uniform int _Iterations;
			uniform int _LoopIteration;
			uniform float _Lightness;
            uniform float _Saturation;
			uniform fixed4 _TintColor;
			uniform float _AlphaIsBlurAmount;

			uniform sampler2D _AltTexture;

			float4 frag(v2f i) : SV_Target
			{

				float4 color = float4(0,0,0,0);

				float4 m = tex2D(_MainTex, i.uv);
				float oma = m.a;

				if (_AlphaIsBlurAmount == 1)
				{
					_Iterations *= _Iterations * m.a;
					m.a = 1;
				}
				

				float w = (m.r + m.b + m.g) / 3;
				float2 uvWH = float2((_MainTex_TexelSize.z / _ScreenParams.x) * _MainTex_TexelSize.x, (_MainTex_TexelSize.w / _ScreenParams.y) * _MainTex_TexelSize.x);
				float2 uvBlur = float2(i.screenpos.x - (uvWH.x / 2), i.screenpos.y - (uvWH.y / 2));
				float4 BlurColor = float4(0, 0, 0, 0);
				float px = 1 / _ScreenParams.x;
				float py = 1 / _ScreenParams.y;

				float d = 0.0;
				float cw = 0.0;
				float totalWeight = 0.0;
				float2 offset = float2(0, 0);
				
				[loop]
				for (int x = -_LoopIteration; x <= _LoopIteration; x++)
				{
					[loop]
					for (int y = -_LoopIteration; y <= _LoopIteration; y++)
					{
						d = sqrt(pow(x, 2) + pow(y, 2));

						if (d == 0)
						{
							cw = 0;
						}
						else
						{
							cw = 1 / d;
						}

						totalWeight += cw;
						offset = float2(x * px, y * py) * (_Iterations / 5.00) * w;
						BlurColor += tex2D(_AltTexture, uvBlur + offset) * cw;

						totalWeight += cw;
						offset = float2(x * px, y * py) * (_Iterations / 2.50) * w;
						BlurColor += tex2D(_AltTexture, uvBlur + offset) * cw;

						totalWeight += cw;
						offset = float2(x * px, y * py) * (_Iterations / 1.25) * w;
						BlurColor += tex2D(_AltTexture, uvBlur + offset) * cw;
					}
				}

				BlurColor = BlurColor / totalWeight;

				if (_AlphaIsBlurAmount == 1)
				{
					w *= oma;
				}

				BlurColor = lerp(BlurColor, BlurColor * _Lightness, w);
				BlurColor = lerp(BlurColor, BlurColor * _TintColor, w);

				float4 intensity = dot(BlurColor, float3(0.299, 0.587, 0.114));
				float4 sat = lerp(intensity, BlurColor, _Saturation);
				BlurColor = lerp(BlurColor, sat, w);

				BlurColor.a = 1 * m.a;
				if (_AlphaIsBlurAmount == 1)
				{
					BlurColor.a = 1;
				}

				return BlurColor;
				
			}
			ENDCG
		}
	}

	Fallback "Sprites/Default"
}