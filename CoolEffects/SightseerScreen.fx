sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    float4 Color = 0.0f;
    float blurSizeX = (0.005) * uOpacity * distance(coords, float2(0.5, 0.5));
    Color += tex2D(uImage0, float2(coords.x - 3.0 * blurSizeX, coords.y)) * 0.09f;
    Color += tex2D(uImage0, float2(coords.x - 2.0 * blurSizeX, coords.y)) * 0.11f;
    Color += tex2D(uImage0, float2(coords.x - blurSizeX, coords.y)) * 0.18f;
    Color += tex2D(uImage0, coords) * 0.24;
    Color += tex2D(uImage0, float2(coords.x + blurSizeX, coords.y)) * 0.18f;
    Color += tex2D(uImage0, float2(coords.x + 2.0 * blurSizeX, coords.y)) * 0.11f;
    Color += tex2D(uImage0, float2(coords.x + 3.0 * blurSizeX, coords.y)) * 0.09f;
    //creates the screen blur.
    float darkness = 0.1 * uOpacity;
    //controls how dark stuff gets.
    Color = float4(Color.r - darkness, Color.g - darkness, Color.b - darkness, Color.a);
    Color = lerp(Color, float4(0, 0, 0, 1), distance(coords, float2(0.5, 0.5)) * 0.2 * uOpacity);
    //Sets the end result of the screenshader, including the colors.
    return Color;
}

technique Technique1
{
    pass Sightseer
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}