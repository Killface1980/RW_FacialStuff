// Source code for face parts sahder
// This needs to be compiled in Unity Editor into an asset bundle because Unity doesn't support compiling shaders outside of editor.
// Therefore changing the code in this file won't do anything.
Shader "Custom/Mod/FacialStuff/FacePart"
{
    Properties
    {
		_MainTex ("Main texture", 2DArray) = "white" { }
		_Color ("Color", Color) = (1,1,1,1)
		_TexIndex ("Texture Array Index", Float) = 0
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

            UNITY_DECLARE_TEX2DARRAY(_MainTex);
			float4 _Color;
			float _TexIndex;
			
            VertexOut VertexProgram(VertexIn v)
            {
                VertexOut o;
                o.outPos = UnityObjectToClipPos(v.vertexPos);
                o.texCoord = v.texCoord;
                return o;
            }

            float4 FragmentProgram(VertexOut i) : SV_Target
            {
				float4 partTexColor = UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(i.texCoord, _TexIndex));
				partTexColor *= _Color;
				if(partTexColor.a < 0.1f)
				{
					discard;
				}
				return partTexColor;
            }
            ENDCG
        }
    }
}
