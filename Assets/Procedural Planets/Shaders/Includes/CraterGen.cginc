struct Crater
{
    float radius;
    float3 centre;
    float floorHeight;
};

StructuredBuffer<Crater> craters;

float SmoothMin(float a, float b, float k)
{
    float h = saturate((b - a + k) / (2 * k));
    return a * h + b * (1 - h) - h * k * (1 - h);
}

float SmoothMax(float a, float b, float k)
{
    k = -k;
    float h = saturate((b - a + k) / (2 * k));
    return a * h + b * (1 - h) - h * k * (1 - h);
}

float CalculateCraterHeight(int numCraters, float3 vertexPos, float floorHeight, float rimWidth, float rimSteepness, float smoothness)
{
    //Set a variable to manipulate the height values of the vertices
    float craterHeight = 0;
    
    //Loop over to check if the current vertex is in a crater
    for (int i = 0; i < numCraters; i++)
    {
        //calculate the distance of the point to the crater between 0 and 1
        float x = length(vertexPos - craters[i].centre) / craters[i].radius;
        
        //Define cavity as a simple parabola
        float cavity = x * x - 1;
        
        //Define the rim as a parabola mirrored around x axis
        //rimWidth changes 
        float rimX = min(x - 1 - rimWidth, 0);
        float rim = rimSteepness * rimX * rimX;
        
        float craterShape = SmoothMax(cavity, floorHeight, smoothness);
        craterShape = SmoothMin(craterShape, rim, smoothness);
        
        craterHeight += craterShape * craters[i].radius;

    }
    return craterHeight;
}