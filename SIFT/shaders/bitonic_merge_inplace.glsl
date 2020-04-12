#version 450

layout(std430,  binding = 0) buffer s    { int buf_s[]; };
layout(std430,  binding = 1) readonly buffer l    { int buf_l[]; };
layout(std430,  binding = 2) readonly buffer r    { int buf_r[]; };

uniform uint global_invocation_id_x_offset;
uniform int start_level;
uniform int level;
uniform int reverse;

void merge2(const in int offset, const in int i, const in int rigt_bound) { // offset <<2^level>> n
	const int n = 1 << level; // [1 ~ n/2-1][n/2 ~ n-1]
	const int j = reverse == 1 ? n - 1 - i : i ^ (1 << (level - 1));
	if (j < i) { // j < i
		const int vj = buf_s[offset + j], vi = buf_s[offset + i];
		if (vj > vi) { // need exchange
			buf_s[offset + j] = vi;
			buf_s[offset + i] = vj;
		}
	}
}

void merge(const in int offset, const in int i, const in int n) { // offset >> offset + i >> offset + n
//	if (((i >> (level - 1)) & 1) != 1) return;
	const int left_bound = i >> level << level;
	merge2(offset + left_bound, i - left_bound, n - left_bound);
}

void main() {
	// Get Index in Global Work Group
	const int i = int(global_invocation_id_x_offset + gl_GlobalInvocationID.x);
	if (i >= buf_s.length()) return;
	merge(buf_l[i], i - buf_l[i], buf_r[i] - buf_l[i] + 1);
}
