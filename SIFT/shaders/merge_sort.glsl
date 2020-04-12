#version 450

layout(std430,  binding = 0) readonly buffer s    { int buf_s[]; };
layout(std430,  binding = 1) readonly buffer l    { int buf_l[]; };
layout(std430,  binding = 2) readonly buffer r    { int buf_r[]; };
layout(std430,  binding = 3) writeonly buffer o    { int buf_o[]; };

uniform uint global_invocation_id_x_offset;
uniform int level;

int search(const in int offset_a, const in int n_a, const in int offset_b, const in int n_b, const in int a_plus_b) { // offset >> offset + a >> offset + a + b
	// 0 <= a <= n_a, 0 <= b <= n_b, a == i - b
	int a_l = max(0, a_plus_b - n_b), a_r = min(a_plus_b, n_a);
	while (a_l < a_r) {
		const int a_m = (a_l + a_r) >> 1;
		if (buf_s[offset_a + a_m] < buf_s[offset_b + (a_plus_b - a_m)]) a_l = a_m + 1;
		else a_r = a_m;
	}
	// exactly s_a[a] >= s_b[i - a]
	return a_r;
}

void merge2(const in int offset, const in int i, const in int n) {
	const int mid = 1 << (level - 1);
	if (mid >= n) return;
	const int a1 = search(offset, mid, offset + mid, n - mid, i); // b1 = i - a1
	const int a2 = search(offset, mid, offset + mid, n - mid, i + 1);
	buf_o[offset + i] = a1 < a2 ? buf_s[offset + a1] : buf_s[offset + (i - a1)];
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
