Shader "Custom/SurfaceShader" {
	SubShader{
		//ZWrite Off
		//Blend One One//加算合成

		Pass {
			CGPROGRAM
			// シェーダーモデルは5.0を指定
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

	StructuredBuffer<float2> d_ht;
	
	float4 ui_calcPos(uint ui_idx)
	{
		uint x = ui_idx % 256;
		uint z = ui_idx / 256;
		float2 h = d_ht[ui_idx];
		if ((x + z) % 2) h.x = -h.x;
		return float4(0.25 * x - 32.0, 8.0 * h.x, 0.25 * z - 32.0, 1);
	}

	struct VSOut {
		float4 pos : SV_POSITION;
	};

	// 頂点シェーダ
	VSOut vert(uint id : SV_VertexID)
	{
		VSOut output;
		output.pos = ui_calcPos(id);
		output.pos = mul(UNITY_MATRIX_VP, output.pos);
		return output;
	}

	// ピクセルシェーダー
	fixed4 frag(VSOut i) : COLOR
	{
		return float4(1,1,1,1);
	}
	ENDCG
 }
	}
}