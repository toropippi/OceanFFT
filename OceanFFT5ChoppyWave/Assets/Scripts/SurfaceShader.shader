Shader "Custom/SurfaceShader" {
	Properties{
		_MainTex("-" , 2D) = "black" {}
	}

	SubShader{

		Pass {
			CGPROGRAM
			// シェーダーモデルは5.0を指定
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
	StructuredBuffer<float2> d_ht;
	StructuredBuffer<float2> d_displaceX;
	StructuredBuffer<float2> d_displaceZ;
	uint N;
	float halfN;
	float dx;
	float dz;
	float lambda;

	float Get_d_ht_real(uint x,uint z)
	{
		float2 h = d_ht[x % 256 + z % 256 * 256];
		return h.x;
	}

	float2 Get_d_displaceXZ(uint x, uint z)
	{
		float2 ret;
		ret.x = d_displaceX[x % 256 + z % 256 * 256].x;
		ret.y = d_displaceZ[x % 256 + z % 256 * 256].x;
		return lambda * ret;
	}

	struct VSOut {
		float4 pos : SV_POSITION;
		float3 pos2 : TEXCOORD2;
		float2 uv : TEXCOORD0;
	};

	// 頂点シェーダ、1つの4角形ポリゴンに頂点4つ
	VSOut vert(uint id : SV_VertexID,float3 normal : NORMAL)
	{
		VSOut output;
		uint sqx = ((id + 1) % 4 / 2);//連続するid4つで四角形を作る
		uint sqz = (1 - id % 4 / 2);
		sqx += (id / 4) % 256;
		sqz += (id / 4) / 256;
		output.pos.y = Get_d_ht_real(sqx, sqz);
		output.pos.xz = float2((sqx - halfN) * dx, (sqz - halfN) * dz);//4角形
		output.pos.xz += Get_d_displaceXZ(sqx, sqz);
		output.pos.w = 1;
		output.pos2 = output.pos.xyz;
		output.pos = mul(UNITY_MATRIX_VP, output.pos);
		float rN1 = 1.0 / N;
		output.uv = float2(sqx, sqz) * rN1 + float2(0.5, 0.5) * rN1;
		return output;
	}
	
	// ピクセルシェーダー
	// ワールド座標系がよくわからないのでレイトレーシング的に色を決定している(要修正)
	float4 frag(VSOut i) : COLOR
	{
		float4 col;
		float3 normal = normalize(tex2D(_MainTex, i.uv).xyz);//線形補完されて単位ベクトルじゃなくなっているので
		float3 viewDir = normalize(i.pos2 - float3(0, 41, -330));
		float3 lightDir = normalize(float3(0.15, 0.45, 0.6));
		float3 reflectDir = -2.0 * dot(normal, viewDir) * normal + viewDir;
		float v = dot(reflectDir, lightDir);
		float3 sky = (v + 1.0) * float3(105.0 / 256, 133.0 / 256, 184.0 / 256);
		float fresnel = (0.05 + (1 - 0.05) * pow(1 - max(dot(normal, -viewDir),0), 5));
		col.xyz = sky * fresnel + (1.0 - fresnel) * float3(0.01, 0.13, 0.15);
		col.xyz += pow(max(v, 0), 249) * float3(1.2, 1.0, 0.86);
		col.w = 1;
		return col;
	}

	ENDCG
 }
	}
}