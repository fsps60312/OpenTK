#version 460

layout(std430,  binding = 0) buffer t    { int buf_t[]; };
layout(std430,  binding = 1) buffer l    { int buf_l[]; };
layout(std430,  binding = 2) buffer r    { int buf_r[]; };
layout(std430,  binding = 3) buffer shift{ int buf_shift[]; };

void main()
{
	// Get Index in Global Work Group
	const uint i = gl_GlobalInvocationID.x;
	if (i >= buf_t.length()) return;
	const int l = buf_l[i], r = buf_r[i];
	const int mid = buf_shift[i] == 1 ? (l + r - 1) / 2: (l + r) / 2;
	buf_t[i] = (buf_t[i] << 1) + (i <= mid ? 0: 1);
	if (i <= mid) buf_r[i] = mid;
	else buf_l[i] = mid + 1;
}
