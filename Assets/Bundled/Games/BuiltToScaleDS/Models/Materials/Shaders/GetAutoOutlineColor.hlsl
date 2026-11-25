// This is shared between the outline shader and the divider shader
// If for whatever reason someone wants to write a more complex way to determine nice looking outline colors you can do it here

#ifndef AUTO_OUTLINE_COLOR
#define AUTO_OUTLINE_COLOR

float4 GetAutoOutlineColor(float4 color)
{
    return color * .325;
}

#endif
