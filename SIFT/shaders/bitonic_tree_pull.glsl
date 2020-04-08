#version 460

layout(std430,  binding = 0) buffer t    { int buf_t[]; };
layout(std430,  binding = 1) buffer l    { int buf_l[]; };
layout(std430,  binding = 2) buffer r    { int buf_r[]; };

void merge_segment(const int i) {
	if ((buf_t[i] & 1) == 0) {
		const int j = buf_r[i] + 1;
		if (j >= buf_t.length() || (buf_t[j] >> 1) != (buf_t[i] >> 1)) return;
		buf_r[i] = buf_r[j];
	} else {
		const int j = buf_l[i] - 1;
		if (j < 0 || (buf_t[j] >> 1) != (buf_t[i] >> 1)) return;
		buf_l[i] = buf_l[j];
	}
}

void main() {
	// Get Index in Global Work Group
	const int i = int(gl_GlobalInvocationID.x);
	if (i >= buf_t.length()) return;
	merge_segment(i);
	buf_t[i] = buf_t[i] >> 1;
}
