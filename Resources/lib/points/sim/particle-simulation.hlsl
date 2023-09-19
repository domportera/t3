#include "lib/shared/point.hlsl"

cbuffer Params : register(b0)
{
    float AddNewPoints;    
    float UseAging;
    float AgingRate;
    float MaxAge;

    float ClampAtMaxAge;
    float Reset;
    float DeltaTime;
    float ApplyMovement;

    float Speed; 
    float Drag;

    float SetInitialVelocity;
    float InitialVelocity;
}

// struct SimPoint
// {
//     float3 Velocity;
//     float w;
//     float4 Test;
// };

cbuffer IntParams : register(b1)
{
    int CollectCycleIndex;
}

StructuredBuffer<Point> NewPoints : t0;
RWStructuredBuffer<Point> CollectedPoints : u0;
RWStructuredBuffer<SimPoint> SimPoints : u1; 

[numthreads(64,1,1)]
void main(uint3 i : SV_DispatchThreadID)
{
    uint newPointCount, pointStride;
    NewPoints.GetDimensions(newPointCount, pointStride);

    uint collectedPointCount, pointStride2;
    CollectedPoints.GetDimensions(collectedPointCount, pointStride2);

    uint gi = i.x;
    if(gi >= collectedPointCount)
        return;

    if(Reset > 0.5)
    {
        CollectedPoints[gi].w =  sqrt(-1);
        CollectedPoints[gi].position =  0;
        SimPoints[gi].Velocity =  0;
        return;
    }

    int addIndex = (gi - CollectCycleIndex) % collectedPointCount;

    // Insert emit points
    if( AddNewPoints > 0.5 && addIndex >= 0 && addIndex < (int)newPointCount )
    {
        CollectedPoints[gi] = NewPoints[addIndex];

        if(UseAging > 0.5) 
        {
            CollectedPoints[gi].w = 0.0001;
        }

        if(SetInitialVelocity > 0.5) 
        {
            //CollectedPoints[gi].rotation = q_encode_v(CollectedPoints[gi].rotation, InitialVelocity);
        }
        SimPoints[gi].Velocity = float3(0,0.042,0); // Fixme
    }


    // Update other points
    else if(UseAging > 0.5 || ApplyMovement > 0.5)
    {
        if(UseAging > 0.5 ) 
        {
            float age = CollectedPoints[gi].w;

            if(!isnan(age)) 
            {    
                if(age <= 0)
                {
                    CollectedPoints[gi].w = sqrt(-1); // Flag non-initialized points
                }
                else if(age < MaxAge)
                {
                    CollectedPoints[gi].w = age+  DeltaTime * AgingRate;
                }
                else if(ClampAtMaxAge) {
                    CollectedPoints[gi].w = MaxAge;
                }
            }
        }

        if(ApplyMovement > 0.5) 
        {            
            Point p = CollectedPoints[gi];
            float3 velocity = SimPoints[gi].Velocity;
            p.position += velocity * Speed * 0.01;
            velocity *= (1-Drag);
            SimPoints[gi].Velocity = velocity;
            CollectedPoints[gi] = p;
        }
    }
}
