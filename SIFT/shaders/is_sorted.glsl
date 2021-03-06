﻿#version 450

layout(std430, binding = 0) buffer s    { int buf_s[]; };
layout(std430, binding = 1) buffer flag { int buf_flag; };

uniform uint global_invocation_id_x_offset;

void main() {
	// Get Index in Global Work Group
	const int i = int(global_invocation_id_x_offset + gl_GlobalInvocationID.x);
	const uint n = buf_s.length();
	if (i == 0 || i >= n) return;
	if (buf_s[i - 1] > buf_s[i]) buf_flag = 0;
}
