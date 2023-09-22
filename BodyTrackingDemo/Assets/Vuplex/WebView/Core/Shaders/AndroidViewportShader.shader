// <copyright file="VideoUnlitShader.cs" company="Google Inc.">
// Copyright (C) 2017 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>

// This shader is a modified version of VideoUnlitShader.cs from the Unity Google VR SDK.
// For Android, it renders from OES_external_image textures, which require special
// OpenGLES extensions and a special texture sampler.
Shader "Vuplex/Android Viewport Shader" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        [Toggle(FLIP_X)] _FlipX ("Flip X", Float) = 0
        [Toggle(FLIP_Y)] _FlipY ("Flip Y", Float) = 0

        [Header(Properties set programmatically)]
        _VideoCutoutRect("Video Cutout Rect", Vector) = (0, 0, 0, 0)
        _CropRect("Crop Rect", Vector) = (0, 0, 0, 0)

        // Include these UI properties from UI-Default.shader
        // in order to support UI Scroll Views.
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader {
        Pass {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }

            // Include these UI properties from UI-Default.shader
            // in order to support UI Scroll Views.
            Stencil {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }

            Lighting Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask [_ColorMask]

            GLSLPROGRAM
                #pragma only_renderers gles gles3
                #extension GL_OES_EGL_image_external : require
                #extension GL_OES_EGL_image_external_essl3 : enable

                #pragma multi_compile ___ FLIP_X
                #pragma multi_compile ___ FLIP_Y
                #pragma multi_compile _ UNITY_COLORSPACE_GAMMA

                precision mediump int;
                precision mediump float;

                #ifdef VERTEX
                    uniform mat4 video_matrix;
                    uniform int unity_StereoEyeIndex;
                    varying vec2 uv;
                    // Pass the vertex color to the fragment shader
                    // so that it can be used for calculating alpha.
                    // This is needed, for example, to allow CanvasGroup.alpha
                    // to control the alpha.
                    varying vec4 vertexColor;

                    void main() {
                        gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
                        vec4 untransformedUV = gl_MultiTexCoord0;
                        vertexColor = gl_Color;
                        #ifdef FLIP_X
                            untransformedUV.x = 1.0 - untransformedUV.x;
                        #endif
                        #ifdef FLIP_Y
                            untransformedUV.y = 1.0 - untransformedUV.y;
                        #endif

                        uv = (video_matrix * untransformedUV).xy;
                    }
                #endif

                #ifdef FRAGMENT

                    // A port of GammaToLinearSpace from UnityCG.cginc
                    vec3 GammaToLinearSpace (vec3 sRGB) {

                        return sRGB * (sRGB * (sRGB * 0.305306011 + 0.682171111) + 0.012522878);
                    }

                    uniform samplerExternalOES _MainTex;
                    uniform vec4 _VideoCutoutRect;
                    uniform vec4 _CropRect;
                    varying vec2 uv;
                    varying vec4 vertexColor;

                    void main() {

                        vec4 col = texture2D(_MainTex, uv);
                        float cutoutWidth = _VideoCutoutRect.z;
                        float cutoutHeight = _VideoCutoutRect.w;

                        #ifdef FLIP_X
                            float nonflippedX = 1.0 - uv.x;
                        #else
                            float nonflippedX = uv.x;
                        #endif
                        #ifdef FLIP_Y
                            float nonflippedY = uv.y;
                        #else
                            float nonflippedY = 1.0 - uv.y;
                        #endif

                        // Make the pixels transparent if they fall within the video rect cutout and the they're black.
                        // Keeping non-black pixels allows the video controls to still show up on top of the video.
                        bool pointIsInCutout = cutoutWidth != 0.0 &&
                                               cutoutHeight != 0.0 &&
                                               nonflippedX >= _VideoCutoutRect.x &&
                                               nonflippedX <= _VideoCutoutRect.x + cutoutWidth &&
                                               nonflippedY >= _VideoCutoutRect.y &&
                                               nonflippedY <= _VideoCutoutRect.y + cutoutHeight;

                        if (pointIsInCutout) {
                            // Use a threshold of 0.15 to consider a pixel as black.
                            bool pixelIsBlack = all(lessThan(col.xyz, vec3(0.15, 0.15, 0.15)));
                            if (pixelIsBlack) {
                                col = vec4(0.0, 0.0, 0.0, 0.0);
                            }
                        }

                        float cropWidth = _CropRect.z;
                        float cropHeight = _CropRect.w;
                        bool pointIsOutsideOfCrop = cropWidth != 0.0 &&
                                                    cropHeight != 0.0 &&
                                                    (nonflippedX < _CropRect.x || nonflippedX > _CropRect.x + cropWidth || nonflippedY < _CropRect.y || nonflippedY > _CropRect.y + cropHeight);
                        if (pointIsOutsideOfCrop) {
                            col = vec4(0.0, 0.0, 0.0, 0.0);
                        }

                        // Place color correction last so it doesn't effect cutout rect functionality.
                        #ifndef UNITY_COLORSPACE_GAMMA
                            col = vec4(GammaToLinearSpace(col.xyz), col.w);
                        #endif

                        // Multiply the alpha by the vertex color's alpha to support CanvasGroup.alpha.
                        gl_FragColor = vec4(col.xyz, col.w * vertexColor.w);
                    }
                #endif
            ENDGLSL
        }
    }
    Fallback "Unlit/Texture"
}
