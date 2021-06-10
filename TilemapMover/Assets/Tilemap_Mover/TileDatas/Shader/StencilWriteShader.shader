Shader "Unlit/StencilWriteShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    _OutlineWidth("Outline Width", float) = 0.1
        _Color("Bottom Color", Color) = (1,1,1,1)

    }
        SubShader
    {
         Tags {  "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            Stencil{
                Ref 2
                Comp always
                Pass replace
            }


            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                half4 vertex : POSITION;
                half3 normal : NORMAL;
            };

            struct v2f
            {
                half4 pos : SV_POSITION;
            };

            half _OutlineWidth;

            v2f vert(appdata v)
            {
                v2f o = (v2f)0;

                o.pos = UnityObjectToClipPos(v.vertex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return 0;
            }
            ENDCG
        }
        Pass
        {
            Cull off//これをつけないと、裏面にして接地したタイルが描画されなくなってしまう

            Stencil
            {
                Ref 3
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                half4 vertex : POSITION;
                half3 normal : NORMAL;
            };

            struct v2f
            {
                half4 pos : SV_POSITION;
            };

            half _OutlineWidth;
            fixed4 _Color;

            v2f vert(appdata v)
            {
                v2f o = (v2f)0;

                o.pos = UnityObjectToClipPos(v.vertex);

                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                fixed4 col = float4(_Color.r,_Color.g,_Color.b,0);
                return col;
            }
            ENDCG
        }

    }
}
