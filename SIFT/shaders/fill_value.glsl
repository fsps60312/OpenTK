#version 450

// warning: only length of power of 2 supported

layout(std430,  binding = 0) buffer s    { int buf_s[]; };

uniform uint global_invocation_id_x_offset;
uniform int value;

void main()
{
	// Get Index in Global Work Group
	const uint i = global_invocation_id_x_offset + gl_GlobalInvocationID.x;
	const uint n = buf_s.length();
	if (i >= n) return;
	buf_s[i] = value;
}
