#version 450

layout(std430,  binding = 0) readonly buffer s    { int buf_s[]; };
layout(std430,  binding = 1) readonly buffer l    { int buf_l[]; };
layout(std430,  binding = 2) readonly buffer r    { int buf_r[]; };
layout(std430,  binding = 3) writeonly buffer o    { int buf_o[]; };

uniform int start_level;
uniform int level;

void merge(const in int offset, const in int i, const in int n) {
	const bool ascend = (bitCount(i >> start_level) & 1) == 0;
	const int j = i ^ (1 << (level - 1));
	if (i < j) {
		if ((buf_s[offset + i] <= buf_s[offset + j]) != ascend) {
			buf_o[offset + j] = buf_s[offset + i];
		} else {
			buf_o[offset + i] = buf_s[offset + i];
		}
	} else {
		if ((buf_s[offset + j] <= buf_s[offset + i]) != ascend) {
			buf_o[offset + j] = buf_s[offset + i];
		} else {
			buf_o[offset + i] = buf_s[offset + i];
		}
	}
}

void main() {
	// Get Index in Global Work Group
	const int i = int(gl_GlobalInvocationID.x);
	if (i >= buf_s.length()) return;
	merge(buf_l[i], i - buf_l[i], buf_r[i] - buf_l[i] + 1);
}
