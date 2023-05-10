#include "lib/shared/point.hlsl"

cbuffer Params : register(b0)
{
    float CollectCycleIndex;
    float AddNewPoints;    
    float AgingRate;
    float MaxAge;

    float ClampAtMaxAge;
    float Reset;
    float DeltaTime;
}

StructuredBuffer<Point> NewPoints : t0;         // input
RWStructuredBuffer<Point> CollectedPoints : u0;    // output

[numthreads(64,1,1)]
void main(uint3 i : SV_DispatchThreadID)
{
    uint newPointCount, pointStride;
    NewPoints.GetDimensions(newPointCount, pointStride);

    uint collectedPointCount, pointStride2;
    CollectedPoints.GetDimensions(collectedPointCount, pointStride2);

    uint gi = i.x;
    if(i.x >= collectedPointCount)
        return;

    if(Reset > 0.5) 
    {
        CollectedPoints[gi].w =  sqrt(-1);
        return;
    }

    int spawnIndex = (int)CollectCycleIndex % collectedPointCount;

    // if(Mode < 0.5) 
    // {
        int addIndex = gi - CollectCycleIndex;
        if(AddNewPoints > 0.5 && addIndex >= 0 && addIndex < (int)newPointCount ) 
        {
            // uint trailLength = (uint)(TrailLength + 0.5);
            // uint bufferLength = (uint)(PointCount + 0.5) * trailLength;
            // uint cycleIndex = (uint)(CycleIndex + 0.5);
            // uint targetIndex = (cycleIndex + gi * trailLength) % bufferLength;
            CollectedPoints[gi] = NewPoints[addIndex];
            CollectedPoints[gi].w = 0.0001;
        }
        else 
        {
            

            float age = CollectedPoints[gi].w;
            if(isnan(age))
                return;

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
            //CollectedPoints[gi].w = 0.1;
        }
    // }
    // else 
    // {
    //     int targetIndex = ( (int)CollectCycleIndex  + i.x) % collectedPointCount;
    //     if( i.x == 0 || targetIndex >= collectedPointCount -1 || targetIndex <= 1) 
    //     {
    //         CollectedPoints[targetIndex].w = sqrt(-1); 
    //         return;
    //     }


    //     if(i.x >= newPointCount) 
    //     {
    //         return;
    //     }        
    //     int sourceIndex = gi;

    //     CollectedPoints[targetIndex] = NewPoints[sourceIndex];
    //     //CollectedPoints[gi].w = 0.0001;
    // }


    //float3 lastPos = CollectedPoints[(targetIndex-1) % bufferLength ].position;
    //CollectedPoints[targetIndex].rotation = normalize(q_look_at(NewPoints[gi].position, lastPos));

    //Point p = NewPoints[i.x];
    //CollectedPoints[targetIndex].w = 0.4;

    // Flag follow position W as NaN line devider
    //CollectedPoints[(targetIndex + 1) % bufferLength].w = sqrt(-1);
}