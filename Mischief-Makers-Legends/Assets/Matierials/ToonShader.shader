Shader "Custom/ToonShader" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.01
        _RampTex ("Ramp Texture", 2D) = "white" {}
    }

    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert nofog nolightmap

        sampler2D _MainTex;
        sampler2D _RampTex;
        float _OutlineWidth;
        float4 _OutlineColor;

        struct Input {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldNormal;
        };

        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.uv_MainTex = v.texcoord;
            o.viewDir = normalize(UnityWorldSpaceViewDir(v.vertex));
            o.worldNormal = UnityObjectToWorldNormal(v.normal);
        }

        void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
            o.Specular = 0;
            o.Gloss = 0;

            float3 normalDirection = normalize(IN.worldNormal);
            float NdotV = dot(normalDirection, IN.viewDir);
            float outline = NdotV * (1.0 - _OutlineWidth);
            o.Emission = _OutlineColor.rgb * smoothstep(0.0, 1.0, outline);

            float diffuse = dot(normalDirection, _WorldSpaceLightPos0.xyz);
            float ramp = tex2D(_RampTex, float2(diffuse, 0.5)).r;
            o.Albedo *= ramp;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
