Shader "Custom/SurfaceShader" {
	SubShader{

		Pass {
			CGPROGRAM
			// シェーダーモデルは5.0を指定
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

	StructuredBuffer<float2> d_ht;
	uint N;
	float halfN;
	float dx;
	float dz;

	float4 ui_calcPos(uint ui_idx)
	{
		uint x = ui_idx % N;
		uint z = ui_idx / N;
		float2 h = d_ht[ui_idx];
		return float4((1.0 * x - halfN) * dx, h.x, (1.0 * z - halfN) * dz, 1);
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
		return float4(0,1,1,1);
	}
	ENDCG
 }
	}
}