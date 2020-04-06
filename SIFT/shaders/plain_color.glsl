#version 460

layout(rgba16f, binding = 0) uniform image2D img_output;

uniform vec3  color;

void main()
{
	// Get Index in Global Work Group
	uvec2 pixel_coords = gl_GlobalInvocationID.xy;
	ivec2 dims   = imageSize(img_output);
	
	vec4 pixel = vec4(float(pixel_coords.x)/dims.x, float(pixel_coords.y)/dims.y, 0.0f, 1.0f);
	
	// Output Color
	imageStore(img_output, ivec2(pixel_coords), pixel);
}
