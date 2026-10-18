#ifndef W_LIGHT_COLOR
#define W_LIGHT_COLOR

#ifdef LIGHT_COLOR_ON
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Includes/Lighting.hlsl"

    half _Specular;
    half _Shiness;
    half4 _SpecularMin;
    half4 _SpecularMax;
#endif

    void GetLightColor(inout half4 color, v2f input)
    {
#ifdef LIGHT_COLOR_ON

        half4 lightColor = half4(LightColorHalf(input.worldPos.xyz), 1);
        color *= lightColor;
        
        half3 lightDir = LightDirectionHalf(input.worldPos.xyz);

        half3 lightReflectDirection = reflect(-lightDir, input.normWorld);
        half lightSeeDirection = saturate(dot(lightReflectDirection, normalize(input.viewDir)));

        if(lightSeeDirection < _Specular) 
        {
            lightSeeDirection = 0; 
        } 
        else 
        {
            // Remap from [_Specular, 1] to [0, 1], safe when _Specular is 1 (no divide by zero)
            lightSeeDirection = saturate((lightSeeDirection - _Specular) / max((half)1 - _Specular, (half)0.0001));
        }

        // pow of a negative or zero base is undefined: some GPUs (Mali) return NaN, which shows
        // up as black or flickering pixels, and half precision can round the base just below 0
        half shininessPower = pow(max(lightSeeDirection, (half)0.0001), max(_Shiness, (half)0.001));

        half4 specColor = lerp(_SpecularMin, _SpecularMax, lightSeeDirection);

        half3 specularReflection = specColor.rgb * shininessPower * specColor.a;

        color += half4(specularReflection, 0);
#endif
    }

#endif