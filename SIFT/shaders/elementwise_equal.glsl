#version 460

layout(std430, binding = 0) buffer s    { int buf_s[]; };
layout(std430, binding = 1) buffer t    { int buf_t[]; };
layout(std430, binding = 2) buffer flag { int buf_flag; };

uniform int value;

void main() {
	// Get Index in Global Work Group
	const int i = int(gl_GlobalInvocationID.x);
	const uint n = buf_s.length();
	if (i >= n) return;
	if (buf_s[i] != buf_t[i]) buf_flag = 0;
}
