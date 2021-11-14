// Source code for Hair shader
// This needs to be compiled in Unity Editor into an asset bundle because Unity doesn't support compiling shaders outside of editor.
// Therefore changing the code in this file won't do anything.
Shader "Custom/Mod/FacialStuff/Hair"
{
    Properties
    {
		_MainTex ("Main texture", 2D) = "white" { }
		_MaskTex ("Mask texture", 2D) = "white" { }
		_Color ("Color", Color) = (1,1,1,1)
		_ColorTwo ("Color Two", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent-100" "RenderType" = "Transparent" }

        Pass
        {
			Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent-100" "RenderType" = "Transparent" }
			
			Blend SrcAlpha OneMinusSrcAlpha
			
            CGPROGRAM
            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram

            #include "UnityCG.cginc"

            struct VertexIn
            {
                float4 vertexPos : POSITION;
                float2 texCoord : TEXCOORD0;
            };

            struct VertexOut
            {
                float2 texCoord : TEXCOORD0;
                float4 outPos : SV_POSITION;
            };

            sampler2D _MainTex;
			sampler2D _MaskTex;
			float4 _Color;
			float4 _ColorTwo;
			
            VertexOut VertexProgram(VertexIn v)
            {
                VertexOut o;
                o.outPos = UnityObjectToClipPos(v.vertexPos);
                o.texCoord = v.texCoord;
                return o;
            }

            float4 FragmentProgram(VertexOut i) : SV_Target
            {
                float4 fragColor = tex2D(_MainTex, i.texCoord) * _Color;
				float4 maskColor = tex2D(_MaskTex, i.texCoord);
				fragColor *= maskColor;
				// Map view doesn't seem to support alpha channel even if RenderType is set to Transparent
				if(fragColor.a < 0.1)
				{
					discard;
				}
				return fragColor;
            }
            ENDCG
        }
    }
}
