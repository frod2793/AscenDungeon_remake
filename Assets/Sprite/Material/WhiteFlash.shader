// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/WhiteFlash"
{
    	Properties {		
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
		_FlashColor ("Flash Color", Color) = (1, 1, 1, 1)
		_FlashAmount ("Flash Amount", Range(0.0, 1.0)) = 0.0
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" "IgnoreProjector"="True" }
		LOD 200

		Cull Off
        
        //any other setting if it needs...

		Blend One OneMinusSrcAlpha

		Pass
		{
	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata_t
			{
		    	float4 vertex   : POSITION;
		    	float4 color    : COLOR;
		    	float2 texcoord : TEXCOORD0; 
			};

			struct v2f
			{
	    		float4 vertex  : SV_POSITION;
	    		fixed4 color   : COLOR;
	    		half2 texcoord : TEXCOORD0;
			};
	
			fixed4 _Color;
			fixed4 _FlashColor;
			float _FlashAmount;

			v2f vert (appdata_t IN)
			{
			    v2f OUT;
			    OUT.vertex = UnityObjectToClipPos(IN.vertex);
			    OUT.texcoord = IN.texcoord;
			    OUT.color = IN.color * _Color;

			    return OUT;
			}

			sampler2D _MainTex;
	
			fixed4 frag (v2f IN) : COLOR
			{
				fixed4 c = tex2D (_MainTex, IN.texcoord) * IN.color;
				c.rgb = lerp (c.rgb, _FlashColor.rgb, _FlashAmount);
				c.rgb *= c.a;
				return c;
			}
			ENDCG
		}
	}
	
	Fallback "Diffuse"
}
