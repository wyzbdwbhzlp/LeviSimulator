Shader "Hidden/StencilMask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-10" }
        ColorMask 0     // 不写入颜色
        ZWrite Off      // 不写入深度（防止挡住角色本体）

        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace // 渲染时把 stencil 设置为 1
        }

        Pass { }
    }
}
