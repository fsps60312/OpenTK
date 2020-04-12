#version 450

layout(std430,  binding = 0) readonly buffer s    { int buf_s[]; };
layout(std430,  binding = 1) readonly buffer l    { int buf_l[]; };
layout(std430,  binding = 2) readonly buffer r    { int buf_r[]; };
layout(std430,  binding = 3) buffer a    { int buf_a[]; };

uniform uint global_invocation_id_x_offset;
uniform int level;
uniform int stride_level;

int search(const in int offset_a, const in int n_a, const in int offset_b, const in int n_b, const in int a_plus_b, const in int a_min, const in int a_max) { // offset >> offset + a >> offset + a + b
	// 0 <= a <= n_a, 0 <= b <= n_b, a == i - b
	int a_l = max(a_min, a_plus_b - n_b), a_r = min(a_max, a_plus_b);
	while (a_l < a_r) {
		const int a = (a_l + a_r) >> 1;
		const int b = a_plus_b - a;
		if (b - 1 >= 0 && buf_s[offset_a + a] <= buf_s[offset_b + b - 1]) a_l = a + 1;
		else a_r = a;
	}
	return a_r;
}

int merge2(const in int offset, const in int i, const in int n) {
	const int n_a = 1 << (level - 1);
	if (n_a >= n) return i;
	const int l_i = i - (1 << stride_level), r_i = i + (1 << stride_level);
	const int a_min = l_i >= 0 ? buf_a[offset + l_i] : 0, a_max = r_i < n ? buf_a[offset + r_i] : n_a;
	return search(offset, n_a, offset + n_a, n - n_a, i, a_min, a_max); // b1 = i - a1
}

int merge(const in int offset, const in int i, const in int n) { // offset >> offset + i >> offset + n
	const int left_bound = i >> level << level;
	return merge2(offset + left_bound, i - left_bound, min(n - left_bound, 1 << level));
}

void main() {
	// Get Index in Global Work Group
	const int i = (1 << stride_level) - 1 + (int(global_invocation_id_x_offset + gl_GlobalInvocationID.x) << stride_level << 1);
	if (i >= buf_s.length()) return;
	const int l = buf_l[i], r = buf_r[i];
	buf_a[i] = merge(l, i - l, r - l + 1);
}
