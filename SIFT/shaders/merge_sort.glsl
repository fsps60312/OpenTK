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
		const int a = (a_l + a_r) >> 1;
		const int b = a_plus_b - a;
		if (b - 1 >= 0 && buf_s[offset_a + a] <= buf_s[offset_b + b - 1]) a_l = a + 1;
		else a_r = a;
	}
	return a_r;
}

int merge2(const in int offset, const in int i, const in int n) {
	const int n_a = 1 << (level - 1);
	if (n_a >= n) return offset + i;
	const int n_b = n - n_a;
	const int a = search(offset, n_a, offset + n_a, n_b, i); // b1 = i - a1
	const int b = i - a;
	return a < n_a && (b >= n_b || buf_s[offset + a] <= buf_s[offset + n_a + b]) ? offset + a : offset + n_a + b;
}

int merge(const in int offset, const in int i, const in int n) { // offset >> offset + i >> offset + n
	const int left_bound = i >> level << level;
	return merge2(offset + left_bound, i - left_bound, min(n - left_bound, 1 << level));
}

void main() {
	// Get Index in Global Work Group
	const int i = int(global_invocation_id_x_offset + gl_GlobalInvocationID.x);
	if (i >= buf_s.length()) return;
	buf_o[i] = buf_s[merge(buf_l[i], i - buf_l[i], buf_r[i] - buf_l[i] + 1)];
}
