#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldViewProj; //Matriz World * View * Projection
float3x3 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

//Textura para Albedo
texture albedoTexture;
sampler2D albedoSampler = sampler_state
{
	Texture = (albedoTexture);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Textura para Normals
texture normalTexture;
sampler2D normalSampler = sampler_state
{
	Texture = (normalTexture);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Textura para Metallic
texture metallicTexture;
sampler2D metallicSampler = sampler_state
{
	Texture = (metallicTexture);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Textura para Roughness
texture roughnessTexture;
sampler2D roughnessSampler = sampler_state
{
	Texture = (roughnessTexture);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Textura para Ambient Occlusion
texture aoTexture;
sampler2D aoSampler = sampler_state
{
	Texture = (aoTexture);
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
};

//Input del Vertex Shader
struct Light
{
	float3 Position;
	float3 Color;
} ;

#define LIGHT_COUNT 4

float3 lightPositions[4];
float3 lightColors[4];

float3 eyePosition; //Posicion de la camara

static const float PI = 3.14159265359;

//Input del Vertex Shader
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TextureCoordinates : TEXCOORD0;
};

//Output del Vertex Shader
struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TextureCoordinates : TEXCOORD0;
	float3 WorldNormal : TEXCOORD1;
	float4 WorldPosition : TEXCOORD2;
};

//Vertex Shader
VertexShaderOutput MainVS(VertexShaderInput input)
{
	VertexShaderOutput output;

	// Proyectamos la position
	output.Position = mul(input.Position, matWorldViewProj);

	// Propagamos las coordenadas de textura
	output.TextureCoordinates = input.TextureCoordinates;

	// Usamos la matriz normal para proyectar el vector normal
	output.WorldNormal = mul(input.Normal, matInverseTransposeWorld);

	// Usamos la matriz de world para proyectar la posicion
	output.WorldPosition = mul(input.Position, matWorld);

	return output;
}

float3 getNormalFromMap(float2 textureCoordinates, float3 worldPosition, float3 worldNormal)
{
	float3 tangentNormal = tex2D(normalSampler, textureCoordinates).xyz * 2.0 - 1.0;

	float3 Q1 = ddx(worldPosition);
	float3 Q2 = ddy(worldPosition);
	float2 st1 = ddx(textureCoordinates);
	float2 st2 = ddy(textureCoordinates);

	worldNormal = normalize(worldNormal.xyz);
	float3 T = normalize(Q1 * st2.y - Q2 * st1.y);
	float3 B = -normalize(cross(worldNormal, T));
	float3x3 TBN = float3x3(T, B, worldNormal);

	return normalize(mul(tangentNormal, TBN));
}

float DistributionGGX(float3 normal, float3 halfVector, float roughness)
{
	float a = roughness * roughness;
	float a2 = a * a;
	float NdotH = max(dot(normal, halfVector), 0.0);
	float NdotH2 = NdotH * NdotH;

	float nom = a2;
	float denom = (NdotH2 * (a2 - 1.0) + 1.0);
	denom = PI * denom * denom;

	return nom / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
	float r = (roughness + 1.0);
	float k = (r * r) / 8.0;

	float nom = NdotV;
	float denom = NdotV * (1.0 - k) + k;

	return nom / denom;
}

float GeometrySmith(float3 normal, float3 view, float3 light, float roughness)
{
	float NdotV = max(dot(normal, view), 0.0);
	float NdotL = max(dot(normal, light), 0.0);
	float ggx2 = GeometrySchlickGGX(NdotV, roughness);
	float ggx1 = GeometrySchlickGGX(NdotL, roughness);

	return ggx1 * ggx2;
}

float3 fresnelSchlick(float cosTheta, float3 F0)
{
	return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);
}

//Pixel Shader
float4 MainPS(VertexShaderOutput input) : COLOR
{
	float3 albedo = pow(tex2D(albedoSampler, input.TextureCoordinates).rgb, float3(2.2, 2.2, 2.2));
	float metallic = tex2D(metallicSampler, input.TextureCoordinates).r;
	float roughness = tex2D(roughnessSampler, input.TextureCoordinates).r;
	float ao = tex2D(aoSampler, input.TextureCoordinates).r;



	float3 worldNormal = input.WorldNormal;
	float3 normal = getNormalFromMap(input.TextureCoordinates, input.WorldPosition.xyz, worldNormal);
    float3 view = normalize(eyePosition - input.WorldPosition.xyz);

	float3 F0 = float3(0.04, 0.04, 0.04);
	F0 = lerp(F0, albedo, metallic);
	
	// Reflectance equation
	float3 Lo = float3(0.0, 0.0, 0.0);
	
	for (int index = 0; index < 4; index++)
	{
		float3 light = lightPositions[index] - input.WorldPosition.xyz;
		float distance = length(light);
		// Normalize our light vector after using its length
		light = normalize(light);
		float3 halfVector = normalize(view + light);		
		float attenuation = 1.0 / (distance);
		float3 radiance = lightColors[index] * attenuation;


		// Cook-Torrance BRDF
		float NDF = DistributionGGX(normal, halfVector, roughness);
		float G = GeometrySmith(normal, view, light, roughness);
		float3 F = fresnelSchlick(max(dot(halfVector, view), 0.0), F0);

		float3 nominator = NDF * G * F;
		float denominator = 4.0 * max(dot(normal, view), 0.0) + 0.001;
		float3 specular = nominator / denominator;

		float3 kS = F;
        
		float3 kD = float3(1.0, 1.0, 1.0) - kS;
        
		kD *= 1.0 - metallic;

		// Scale light by NdotL
		float NdotL = max(dot(normal, light), 0.0);

        //TODO
		Lo += (kD * NdotL * albedo / PI + specular) * radiance;
	}

	float3 ambient = float3(0.03, 0.03, 0.03) * albedo * ao;

    float3 color = ambient + Lo;

	// HDR tonemapping
	color = color / (color + float3(1, 1, 1));
    
	float exponent = 1.0 / 2.2;
	// Gamma correct
	color = pow(color, float3(exponent, exponent, exponent));

    return float4(color, 1.0);
}

technique PBR
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};