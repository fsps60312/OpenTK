#version 450

layout(std430, binding = 0) readonly  buffer s    { int buf_s[]; };
layout(std430, binding = 1) writeonly buffer t    { int buf_t[]; };

uniform uint global_invocation_id_x_offset;

void main() {
	// Get Index in Global Work Group
	const int i = int(global_invocation_id_x_offset + gl_GlobalInvocationID.x);
	const uint n = buf_s.length();
	if (i >= n) return;
	buf_t[i] = buf_s[i];
}
