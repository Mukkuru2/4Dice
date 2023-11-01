Shader "Unlit/WorldSpaceNormals"
{
    // no Properties block this time!
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // include file that contains UnityObjectToWorldNormal helper function
            #include "UnityCG.cginc"

            struct v2f
            {
                // we'll output world space normal as one of regular ("texcoord") interpolators
                half3 worldNormal : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 colnoise : COLOR0;
            };
            
            float nrand(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Perlin noise function
            // Source: https://gist.github.com/patriciogonzalezvivo/670c22f3966e662d2f83
            float noise(float2 n)
            {
                const float2 d = float2(0.0, 1.0);
                float2 b = floor(n), f = smoothstep(float2(0.0, 0.0), float2(1.0, 1.0), frac(n));
                float2 o = lerp(lerp(nrand(b), nrand(b + d.yx), f.x), lerp(nrand(b + d.xy), nrand(b + d.yy), f.x), f.y);
                // Scale n between -1 and 1
                o = o - 0.5;
                return o;
            }

            // vertex shader: takes object space normal as input too
            v2f vert(float4 vertex : POSITION, float3 normal : NORMAL)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                o.worldNormal = UnityObjectToWorldNormal(normal);
                o.colnoise = noise((o.worldNormal * 10 + o.pos * 0 + _Time * 0.1) * 10);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = 0;
                // normal is a 3D vector with xyz components; in -1..1
                // range. To display it as color, bring the range into 0..1
                // and put into red, green, blue components.

                // After this, add a bit of noise to it to make it more interesting.

                c.rgb = i.worldNormal * 0.5 + 0.5;
                c.a = 1;

                c.rgb += i.colnoise;

                return c;
            }
            ENDCG
        }
    }
}