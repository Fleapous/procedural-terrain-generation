Shader "Custom/TerrainShader1" {
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _ColorStart ("Start Color", Color) = (1, 0, 0, 1) // Red as default start color
        _ColorEnd ("End Color", Color) = (0, 0, 1, 1)   // Blue as default end color
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert

        struct Input {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        fixed4 _ColorStart;
        fixed4 _ColorEnd;

        // Vertex function to calculate the Y position of each vertex
        void vert(inout appdata_full v)
        {
            UNITY_INITIALIZE_OUTPUT(appdata_full, v);
            v.texcoord.x = v.vertex.y; // Store the Y coordinate in the texcoord.x channel
        }

        // Fragment shader function (Lambert in this case)
        void surf (Input IN, inout SurfaceOutput o)
        {
            // Calculate the color based on the Y position
            fixed4 gradientColor = lerp(_ColorStart, _ColorEnd, saturate(IN.uv_MainTex.x));

            // Assign the albedo color to the gradient color
            o.Albedo = gradientColor.rgb;

            // Sample the main texture for the surface
            o.Albedo *= tex2D(_MainTex, IN.uv_MainTex).rgb;
        }
        ENDCG
    }

    FallBack "Diffuse"
}

