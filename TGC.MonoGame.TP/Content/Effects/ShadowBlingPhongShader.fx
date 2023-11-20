#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 WorldViewProjection;
float4x4 InverseTransposeWorld;
float4x4 World;
float4x4 LightViewProjection;

float3 lightPosition;
float3 ambientColor; // Light's Ambient Color
float3 diffuseColor; // Light's Diffuse Color
float3 specularColor; // Light's Specular Color
float KAmbient; 
float KDiffuse; 
float KSpecular;
float shininess; 
float3 eyePosition; // Camera position

float2 shadowMapSize;

#define MAX_COUNT 5
float3 ImpactPositions[MAX_COUNT];
float3 ImpactDirections[MAX_COUNT];
int Impacts = 0;

float BulletRadius = 0.5;

static const float modulatedEpsilon = 0.000041200182749889791011810302734375;
static const float maxEpsilon = 0.000023200045689009130001068115234375;

texture baseTexture;
sampler2D textureSampler = sampler_state
{
	Texture = (baseTexture);
	MagFilter = Linear;
	MinFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;
};
    

texture shadowMap;
sampler2D shadowMapSampler =
sampler_state
{
	Texture = <shadowMap>;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct DepthPassVertexShaderInput
{
	float4 Position : POSITION0;
};

struct DepthPassVertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 ScreenSpacePosition : TEXCOORD1;
};

struct ShadowedVertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float2 TextureCoordinates : TEXCOORD0;
};

struct ShadowedVertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 TextureCoordinates : TEXCOORD0;
	float4 WorldSpacePosition : TEXCOORD1;
	float4 LightSpacePosition : TEXCOORD2;
    float4 Normal : TEXCOORD3;
};




DepthPassVertexShaderOutput DepthVS(in DepthPassVertexShaderInput input)
{
	DepthPassVertexShaderOutput output;
	output.Position = mul(input.Position, WorldViewProjection);
	output.ScreenSpacePosition = mul(input.Position, WorldViewProjection);
	return output;
}

float4 DepthPS(in DepthPassVertexShaderOutput input) : COLOR
{
    float depth = input.ScreenSpacePosition.z / input.ScreenSpacePosition.w;
    return float4(depth, depth, depth, 1.0);
}



ShadowedVertexShaderOutput MainVS(in ShadowedVertexShaderInput input)
{
	ShadowedVertexShaderOutput output;
	output.WorldSpacePosition = mul(input.Position, World);
    // input.Position = mul(input.Position, World);

    // float Distance = 0;
    // float mask = 0;
    // for(int i=0; i < MAX_COUNT; i++) {
    //     if(i >= Impacts)
    //         break;
    //     Distance = distance(input.Position.xyz, ImpactPositions[i]);
    //     mask = step(Distance, 1.25);
    //     input.Position.xyz = lerp(input.Position.xyz, input.Position.xyz + ImpactDirections[i], mask);
    // }

	output.Position = mul(input.Position, WorldViewProjection);
	output.TextureCoordinates = input.TextureCoordinates;
    output.Normal = mul(float4(input.Normal, 1), InverseTransposeWorld);
    
	output.LightSpacePosition = mul(output.WorldSpacePosition, LightViewProjection);
	return output;
}

float4 ShadowedPS(in ShadowedVertexShaderOutput input) : COLOR
{
    float3 lightSpacePosition = input.LightSpacePosition.xyz / input.LightSpacePosition.w;
    float2 shadowMapTextureCoordinates = 0.5 * lightSpacePosition.xy + float2(0.5, 0.5);
    shadowMapTextureCoordinates.y = 1.0f - shadowMapTextureCoordinates.y;
	
	
    float3 normal = normalize(input.Normal.rgb);
    float3 lightDirection = normalize(lightPosition - input.WorldSpacePosition.xyz);
    float inclinationBias = max(modulatedEpsilon * (1.0 - dot(normal, lightDirection)), maxEpsilon);
	
    float shadowMapDepth = tex2D(shadowMapSampler, shadowMapTextureCoordinates).r + inclinationBias;
	
	// Compare the shadowmap with the REAL depth of this fragment
	// in light space
    float notInShadow = step(lightSpacePosition.z, shadowMapDepth);
	
	float4 baseColor = tex2D(textureSampler, input.TextureCoordinates);
    baseColor.rgb *= 0.5 + 0.5 * notInShadow;
	return baseColor;
}

float4 ShadowedPCFPS(in ShadowedVertexShaderOutput input) : COLOR
{
    float3 lightSpacePosition = input.LightSpacePosition.xyz / input.LightSpacePosition.w;
    float2 shadowMapTextureCoordinates = 0.5 * lightSpacePosition.xy + float2(0.5, 0.5);
    shadowMapTextureCoordinates.y = 1.0f - shadowMapTextureCoordinates.y;
	
    float3 normal = normalize(input.Normal.rgb);
    float3 lightDirection = normalize(lightPosition - input.WorldSpacePosition.xyz);
    float inclinationBias = max(modulatedEpsilon * (1.0 - dot(normal, lightDirection)), maxEpsilon);
	
	// Sample and smooth the shadowmap
	// Also perform the comparison inside the loop and average the result
    float notInShadow = 0.0;
    float2 texelSize = 1.0 / shadowMapSize;
    for (int x = -1; x <= 1; x++)
        for (int y = -1; y <= 1; y++)
        {
            float pcfDepth = tex2D(shadowMapSampler, shadowMapTextureCoordinates + float2(x, y) * texelSize).r + inclinationBias;
            notInShadow += step(lightSpacePosition.z, pcfDepth) / 9.0;
        }
	
    float4 texelColor = tex2D(textureSampler, input.TextureCoordinates);
    texelColor.rgb *= 0.5 + 0.5 * notInShadow;

	// float4 texelColor = tex2D(textureSampler, input.TextureCoordinates);
    float3 viewDirection = normalize(eyePosition - input.WorldSpacePosition.xyz);
    float3 halfVector = normalize(lightDirection + viewDirection);
	float NdotL = saturate(dot(input.Normal.xyz, lightDirection));
    float3 diffuseLight = KDiffuse * diffuseColor * NdotL;
	float NdotH = dot(input.Normal.xyz, halfVector);
    float3 specularLight = sign(NdotL) * KSpecular * specularColor * pow(saturate(NdotH), shininess);
    float4 finalColor = float4(saturate(ambientColor * KAmbient + diffuseLight) * texelColor.rgb + specularLight, texelColor.a);
    return finalColor;
}




technique DepthPass
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL DepthVS();
		PixelShader = compile PS_SHADERMODEL DepthPS();
	}
};

technique DrawShadowed
{
	pass Pass0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL ShadowedPS();
	}
};

technique DrawShadowedPCF
{
    pass Pass0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL ShadowedPCFPS();
    }
};