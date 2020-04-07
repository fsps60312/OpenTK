#version 460

layout(std430,  binding = 0) buffer a    { int buf_a[]; };
layout(std430,  binding = 1) buffer b    { int buf_b[]; };
layout(std430,  binding = 2) buffer c    { int buf_c[]; };

void main()
{
	// Get Index in Global Work Group
	const uint i = gl_GlobalInvocationID.x;
	// buf_a.length() is int not uint
	if(i < buf_c.length()) buf_c[i] = buf_a[i] + buf_b[i];
}
