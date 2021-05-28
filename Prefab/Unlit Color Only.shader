Shader "Custom/UnlitColorOnly" {

    Properties{
        _UnlitColor("UnlitColor", Color) = (1,1,1)
    }

    SubShader{
        Tags { "RenderType" = "Opaque" }
        LOD 100
        Pass {
            Lighting Off
            ZWrite On
            Cull Back
            SetTexture[_] {
                constantColor[_UnlitColor]
                Combine constant
            }
        }
    }

    SubShader{
        Tags { "RenderType" = "Transparent" }
        LOD 100
        Pass {
            Lighting Off
            ZWrite On
            Cull Back
            SetTexture[_] {
                constantColor[_Color]
                Combine constant
            }
        }
    }
}