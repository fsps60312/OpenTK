#version 460

layout(std430,  binding = 0) buffer t    { int buf_t[]; };
layout(std430,  binding = 1) buffer l    { int buf_l[]; };
layout(std430,  binding = 2) buffer r    { int buf_r[]; };

void main()
{
	// Get Index in Global Work Group
	const uint i = gl_GlobalInvocationID.x;
	if (i >= buf_l.length()) return;
	const int l = buf_l[i], r = buf_r[i];
	const int mid = (l + r) / 2;
	if (i <= mid) buf_r[i] = mid;
	else buf_l[i] = mid + 1;
}
