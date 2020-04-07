#version 460

// warning: only length of power of 2 supported

layout(std430,  binding = 0) buffer s    { int buf_s[]; };

uniform int value;

void main()
{
	// Get Index in Global Work Group
	const uint i = gl_GlobalInvocationID.x;
	const uint n = buf_s.length();
	if (i >= n) return;
	buf_s[i] = value;
}
