// Copyright (c) 2022 Vuplex Inc. All rights reserved.
//
// Licensed under the Vuplex Commercial Software Library License, you may
// not use this file except in compliance with the License. You may obtain
// a copy of the License at
//
//     https://vuplex.com/commercial-library-license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
Shader "Vuplex/Viewport Shader" {
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

            CGPROGRAM
                #pragma multi_compile ___ FLIP_X
                #pragma multi_compile ___ FLIP_Y
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    // Pass the vertex color to the fragment shader
                    // so that it can be used for calculating alpha.
                    // This is needed, for example, to allow CanvasGroup.alpha
                    // to control the alpha.
                    float4 vertexColor : COLOR;
                    // For Single Pass Instanced stereo rendering
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    float4 vertexColor : COLOR0;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                Texture2D _MainTex;
                // Specify linear filtering by using a SamplerState
                // and specifying "linear" in its name.
                // https://docs.unity3d.com/Manual/SL-SamplerStates.html
                SamplerState linear_clamp_sampler;

                float4 _MainTex_ST;

                v2f vert(appdata v) {

                    v2f o;
                    // For Single Pass Instanced stereo rendering
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_OUTPUT(v2f, o);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.vertexColor =  v.vertexColor;
                    float2 untransformedUV = v.uv;

                    #ifdef FLIP_X
                        untransformedUV.x = 1.0 - untransformedUV.x;
                    #endif
                    #ifdef FLIP_Y
                        untransformedUV.y = 1.0 - untransformedUV.y;
                    #endif

                    o.uv = TRANSFORM_TEX(untransformedUV, _MainTex);
                    return o;
                }

                float4 _VideoCutoutRect;
                float4 _CropRect;

                fixed4 frag(v2f i) : SV_Target {

                    fixed4 col = _MainTex.Sample(linear_clamp_sampler, i.uv);
                    float cutoutWidth = _VideoCutoutRect.z;
                    float cutoutHeight = _VideoCutoutRect.w;

                    #ifdef FLIP_X
                        float nonflippedX = 1.0 - i.uv.x;
                    #else
                        float nonflippedX = i.uv.x;
                    #endif
                    #ifdef FLIP_Y
                        float nonflippedY = i.uv.y;
                    #else
                        float nonflippedY = 1.0 - i.uv.y;
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
                        bool pixelIsBlack = all(col.xyz < float3(0.15, 0.15, 0.15));
                        if (pixelIsBlack) {
                            col = float4(0.0, 0.0, 0.0, 0.0);
                        }
                    }

                    float cropWidth = _CropRect.z;
                    float cropHeight = _CropRect.w;
                    bool pointIsOutsideOfCrop = cropWidth != 0.0 &&
                                                cropHeight != 0.0 &&
                                                (nonflippedX < _CropRect.x || nonflippedX > _CropRect.x + cropWidth ||nonflippedY < _CropRect.y || nonflippedY > _CropRect.y + cropHeight);
                    if (pointIsOutsideOfCrop) {
                        col = float4(0.0, 0.0, 0.0, 0.0);
                    }

                    // Place color correction last so it doesn't effect cutout rect functionality.
                    #ifndef UNITY_COLORSPACE_GAMMA
                        col = float4(GammaToLinearSpace(col.xyz), col.w);
                    #endif

                    // Multiply the alpha by the vertex color's alpha to support CanvasGroup.alpha.
                    col = float4(col.xyz, col.w * i.vertexColor.w);
                    return col;
                }
            ENDCG
        }
    }
    Fallback "Unlit/Texture"
}
