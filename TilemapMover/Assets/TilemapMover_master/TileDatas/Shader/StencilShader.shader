Shader "Unlit/StencilShader"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
    _MainColor("_MainColor",Color) = (1,1,1,1)
        _OutlineWidth("Outline Width", float) = 0.1
        _Color("Bottom Color", Color) = (1,1,1,1)
        _Color2("Top Color", Color) = (1,1,1,1)
         _Scale("Scale", Float) = 1
    }
        SubShader
    {
      Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }
        LOD 100
         Blend One OneMinusSrcAlpha
        Zwrite Off



        Pass
        {
            Cull back
            Stencil
            {
                Ref 3
                Comp Equal
            }

            CGPROGRAM
       #pragma vertex vert
                    #pragma fragment frag

                    #include "UnityCG.cginc"

                    struct appdata_t {
                        float4 vertex : POSITION;
                        float2 texcoord : TEXCOORD0;
                    };

                    struct v2f {
                        float4 vertex : SV_POSITION;
                        half2 texcoord : TEXCOORD0;
                        fixed4 col : COLOR;
                    };

                    sampler2D _MainTex;
                    float4 _MainTex_ST;
                    fixed4 _Color;
                    fixed4 _Color2;
                    fixed  _Scale;

                    v2f vert(appdata_t v)
                    {
                        v2f o;
                        o.col = lerp(_Color,_Color2, v.vertex.y * _Scale + 0.3f * abs(sin(_Time.y)));
                        o.vertex = UnityObjectToClipPos(v.vertex);
                        o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                        return o;
                    }

                    fixed4 frag(v2f i) : SV_Target
                    {
                        fixed4 col = tex2D(_MainTex, i.texcoord) * i.col;
                        return col;
                    }
                ENDCG
        }

        Pass
        {

            ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Off

            Stencil
            {
                Ref 2
                Comp Equal
        Fail Keep
        ZFail Keep
        Pass Keep
            }
            CGPROGRAM
            #pragma vertex vert_img
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

                o.pos = UnityObjectToClipPos(v.vertex + v.normal * _OutlineWidth);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return 0;
            }
            ENDCG
        }
    }
}
