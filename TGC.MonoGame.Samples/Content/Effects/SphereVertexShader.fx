#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Custom Effects - https://docs.monogame.net/articles/content/custom_effects.html
// High-level shader language (HLSL) - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl
// Programming guide for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-pguide
// Reference for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-reference
// HLSL Semantics - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics

// Matrix = float4x4
// Vector2 = float2
// Vector3 = float3
// Vector4 = float4

uniform float4x4 World;
uniform float4x4 View;
uniform float4x4 Projection;
uniform float Time = 0;

struct VertexShaderInput
{
	// Posicion en espacio local
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	// Posicion en espacio de proyeccion
	float4 Position : SV_POSITION;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	// W = 1

	// y >= 0

	float uno = length(input.Normal);

	// |A| * |B| * cos(C)
	// Normalizados = cos(C)

	// dot => Que tan alineados estan dos vectores (si estan normalizados)
	// dot => Proyeccion de un vector sobre el otro si no estan normalizados

	// dot(a, b) => 1 si estan alineados
	// dot(a, b) = cos(45°) = 0.707...
	// dot(a, b) => 0 si son perpendiculares
	// dot(a, b) => -1 si estan alineados pero en sentido contrario

	// clamp(v, a, b) => v entre a y b
	// saturate(v) => clamp(v, 0.0, 1.0)

	// cross(a, b) => vector perpendicular a ambos

	// abs(a) => Valor absoluto de a

	// floor(a) => Valor redondeado hacia el entero mas cercano hacia abajo
	// ceil(a) => Valor redondeado hacia el entero mas cercano hacia arriba
	// round(a) => Valor redondeado hacia el entero mas cercano

	// length(v) => longitud del vector / norma

	// normalize(v) => devuelve el vector normalizado

	// step(a, b) => Escalon => 1 si a <= b
	
	// Traduce a...

	input.Position.x += 0.05 * sin(input.Position.y * 10.0 + Time * 10.0);

	// Local a Mundo
    float4 worldPosition = mul(input.Position, World);
		
	// Mundo a Vista
    float4 viewPosition = mul(worldPosition, View);

	// Vista a Proyeccion
    output.Position = mul(viewPosition, Projection);

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return float4(0.0, 0.0, 0.0, 1.0);
}

technique BaseTechnique
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

