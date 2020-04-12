#version 450

layout(std430,  binding = 0) readonly buffer s    { int buf_s[]; };
layout(std430,  binding = 1) readonly buffer l    { int buf_l[]; };
layout(std430,  binding = 2) readonly buffer r    { int buf_r[]; };
layout(std430,  binding = 3) writeonly buffer o    { int buf_o[]; };
//layout(std430,  binding = 4) writeonly buffer debug    { int buf_debug[]; };

uniform uint global_invocation_id_x_offset;
uniform int level;

int search(const in int offset_a, const in int n_a, const in int offset_b, const in int n_b, const in int a_plus_b) { // offset >> offset + a >> offset + a + b
	// 0 <= a <= n_a, 0 <= b <= n_b, a == i - b
	int a_l = max(0, a_plus_b - n_b), a_r = min(a_plus_b, n_a);
	while (a_l < a_r) {
		const int a = (a_l + a_r + 1) >> 1;
		const int b = a_plus_b - a;
		if (a - 1 < 0 || b >= n_b || buf_s[offset_a + a - 1] <= buf_s[offset_b + b]) a_l = a;
		else a_r = a - 1;
	}
	// goal: s_a[a - 1] <= s_b[b]
	// if (s_a[0] <= s_b[0]): (1, 0)
	// else:                  (0, 1)
//	if (buf_s[offset_a + a_r] < buf_s[offset_b + (a_plus_b - a_r)]) buf_o[offset_a + a_plus_b] = 1;
	return a_r;
}

int merge2(const in int offset, const in int i, const in int n) {
	const int mid = 1 << (level - 1);
	if (mid >= n) return buf_s[offset + i];
//	buf_debug[offset + i] = 0;
	const int a1 = search(offset, mid, offset + mid, n - mid, i); // b1 = i - a1
	const int a2 = search(offset, mid, offset + mid, n - mid, i + 1);
//	buf_debug[offset + i] = 0;
	return a1 < a2 ? buf_s[offset + a1] : buf_s[offset + mid + (i - a1)];
//	buf_debug[offset + i] = i * 100 + a1*10 + a2;
}

void merge(const in int offset, const in int i, const in int n) { // offset >> offset + i >> offset + n
//	if (((i >> (level - 1)) & 1) != 1) return;
	const int left_bound = i >> level << level;
	buf_o[offset + i] = merge2(offset + left_bound, i - left_bound, min(n - left_bound, 1 << level));
}

void main() {
	// Get Index in Global Work Group
	const int i = int(global_invocation_id_x_offset + gl_GlobalInvocationID.x);
	if (i >= buf_s.length()) return;
	merge(buf_l[i], i - buf_l[i], buf_r[i] - buf_l[i] + 1);
}
