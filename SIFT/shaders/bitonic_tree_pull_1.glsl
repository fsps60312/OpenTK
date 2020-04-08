#version 460

layout(std430,  binding = 0) readonly  buffer t    { int buf_t[]; };
layout(std430,  binding = 1) readonly  buffer l    { int buf_l[]; };
layout(std430,  binding = 2) readonly  buffer r    { int buf_r[]; };
layout(std430,  binding = 3) writeonly buffer l_ret{ int buf_l_ret[]; };
layout(std430,  binding = 4) writeonly buffer r_ret{ int buf_r_ret[]; };

void merge_segment(const int i) {
	buf_l_ret[i] = buf_l[i];
	buf_r_ret[i] = buf_r[i];
	if ((buf_t[i] & 1) == 0) {
		const int j = buf_r[i] + 1;
		if (j >= buf_t.length() || (buf_t[j] >> 1) != (buf_t[i] >> 1)) return;
		buf_r_ret[i] = buf_r[j];
	} else {
		const int j = buf_l[i] - 1;
		if (j < 0 || (buf_t[j] >> 1) != (buf_t[i] >> 1)) return;
		buf_l_ret[i] = buf_l[j];
	}
}

void main() {
	// Get Index in Global Work Group
	const int i = int(gl_GlobalInvocationID.x);
	if (i >= buf_t.length()) return;
	merge_segment(i);
}
