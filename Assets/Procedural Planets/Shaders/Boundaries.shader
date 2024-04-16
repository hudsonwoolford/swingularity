Shader "Hidden/Boundaries"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };
            float2 _CamSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                float4 vert = float4(o.vertex.xy, 0, 1);
                float3 objW = mul(unity_ObjectToWorld, vert).xyz;
                o.worldPos = (float3(objW.x * (_CamSize.x / 2), objW.y * (_CamSize.y / 2), 0) + _WorldSpaceCameraPos);
                o.worldPos.z = 0;
                return o;
            }

            sampler2D _MainTex;
            float3 polygonP[6];
            fixed4 _Color;

            #define NO 0
            #define YES 1
            #define COLLINEAR 2

            int areIntersecting(
                float v1x1, float v1y1, float v1x2, float v1y2,
                float v2x1, float v2y1, float v2x2, float v2y2
            ) {
                float d1, d2;
                float a1, a2, b1, b2, c1, c2;

                // Convert vector 1 to a line (line 1) of infinite length.
                // We want the line in linear equation standard form: A*x + B*y + C = 0
                a1 = v1y2 - v1y1;
                b1 = v1x1 - v1x2;
                c1 = (v1x2 * v1y1) - (v1x1 * v1y2);

                // Every point (x,y), that solves the equation above, is on the line,
                // every point that does not solve it, is not. The equation will have a
                // positive result if it is on one side of the line and a negative one 
                // if is on the other side of it. We insert (x1,y1) and (x2,y2) of vector
                // 2 into the equation above.
                d1 = (a1 * v2x1) + (b1 * v2y1) + c1;
                d2 = (a1 * v2x2) + (b1 * v2y2) + c1;

                // If d1 and d2 both have the same sign, they are both on the same side
                // of our line 1 and in that case no intersection is possible. Careful, 
                // 0 is a special case, that's why we don't test ">=" and "<=", 
                // but "<" and ">".
                if (d1 > 0 && d2 > 0) return NO;
                if (d1 < 0 && d2 < 0) return NO;

                // The fact that vector 2 intersected the infinite line 1 above doesn't 
                // mean it also intersects the vector 1. Vector 1 is only a subset of that
                // infinite line 1, so it may have intersected that line before the vector
                // started or after it ended. To know for sure, we have to repeat the
                // the same test the other way round. We start by calculating the 
                // infinite line 2 in linear equation standard form.
                a2 = v2y2 - v2y1;
                b2 = v2x1 - v2x2;
                c2 = (v2x2 * v2y1) - (v2x1 * v2y2);

                // Calculate d1 and d2 again, this time using points of vector 1.
                d1 = (a2 * v1x1) + (b2 * v1y1) + c2;
                d2 = (a2 * v1x2) + (b2 * v1y2) + c2;

                // Again, if both have the same sign (and neither one is 0),
                // no intersection is possible.
                if (d1 > 0 && d2 > 0) return NO;
                if (d1 < 0 && d2 < 0) return NO;

                // If we get here, only two possibilities are left. Either the two
                // vectors intersect in exactly one point or they are collinear, which
                // means they intersect in any number of points from zero to infinite.
                if ((a1 * b2) - (a2 * b1) == 0.0f) return COLLINEAR;

                // If they are not collinear, they must intersect in exactly one point.
                return YES;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                //for(int k; k < polygonP.Length; k++)
                //{
                //    if(length(i.worldPos - polygonP[k]) < 1)
                //    {
                //        col.rgb = 1 - col.rgb;
                //        return col;
                //    }
                
                //}
                //return col;

                float3 wp = i.worldPos.xyz;

                float3 outsidePoint = (100, 100);
                int intersections = 0;
                bool colLin = false;
                for(int x = 0; x<5; x++)
                {
                    float check = areIntersecting(outsidePoint.x, outsidePoint.y, wp.x, wp.y, polygonP[x].x, polygonP[x].y, polygonP[x+1].x, polygonP[x+1].y);
                    intersections += check;
                }
                float check = areIntersecting(outsidePoint.x, outsidePoint.y, wp.x, wp.y, polygonP[polygonP.Length-1].x, polygonP[polygonP.Length-1].y, polygonP[0].x, polygonP[0].y);
                intersections += check;

                if(intersections % 2 == 0) 
                {
                    return _Color;
                }
                else
                {
                    return col;
                }
                
                // just invert the colors
                
            }
            ENDCG
        }
    }
}
