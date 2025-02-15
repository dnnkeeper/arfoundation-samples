﻿Shader "Unlit/ARCoreBackground(LINEAR)"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

    // For GLES3
    SubShader
    {
        Pass
        {
            ZWrite Off
            Cull Off

            GLSLPROGRAM

#pragma only_renderers gles3

#ifdef SHADER_API_GLES3
#extension GL_OES_EGL_image_external_essl3 : require
#endif

            uniform mat4 _UnityDisplayTransform;

#ifdef VERTEX
            varying vec2 textureCoord;

            void main()
            {
#ifdef SHADER_API_GLES3
                float flippedV = 1.0 - gl_MultiTexCoord0.y;
                textureCoord.x = _UnityDisplayTransform[0].x * gl_MultiTexCoord0.x + _UnityDisplayTransform[1].x * flippedV + _UnityDisplayTransform[2].x;
                textureCoord.y = _UnityDisplayTransform[0].y * gl_MultiTexCoord0.x + _UnityDisplayTransform[1].y * flippedV + _UnityDisplayTransform[2].y;
                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
#endif
            }
#endif

#ifdef FRAGMENT
            varying vec2 textureCoord;
            uniform samplerExternalOES _MainTex;

            void main()
            {
#ifdef SHADER_API_GLES3

                gl_FragColor = vec4(texture(_MainTex, textureCoord).xyz, 1);

				gl_FragColor.rgb = pow(gl_FragColor.rgb, vec3(2.2));
				gl_FragColor.r = clamp(gl_FragColor.r, 0.0, 1.0);
				gl_FragColor.g = clamp(gl_FragColor.g, 0.0, 1.0);
				gl_FragColor.b = clamp(gl_FragColor.b, 0.0, 1.0);
#endif
            }

#endif
            ENDGLSL
        }
    }

    FallBack Off
}
