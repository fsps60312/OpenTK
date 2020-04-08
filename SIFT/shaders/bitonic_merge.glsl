#version 460

layout(std430,  binding = 0) buffer s    { int buf_s[]; };
layout(std430,  binding = 1) buffer t    { int buf_t[]; };
layout(std430,  binding = 2) buffer l    { int buf_l[]; };
layout(std430,  binding = 3) buffer r    { int buf_r[]; };
// return
layout(std430,  binding = 4) buffer ret  { int buf_ret[]; };
layout(std430,  binding = 5) buffer shift{ int buf_shift[]; };

uniform int level;

int cal_dest(const int i, const bool ascend) {
	const int l = buf_l[i], r = buf_r[i];
	const int mid = (l + r) / 2;
	const int n = r - l + 1;
	if ((n & 1) == 0) {
		buf_shift[i] = 0;
		if (i <= mid) {
			const int j = i + n / 2;
			return (buf_s[i] < buf_s[j]) == ascend ? i: j;
		} else {
			const int j = i - n / 2;
			return (buf_s[j] < buf_s[i]) == ascend ? i: j;
		}
	} else {
		bool no_shift = (buf_s[mid] < buf_s[r]) == ascend;
		buf_shift[i] = no_shift ? 0: 1;
		if (i == mid) return no_shift? i: r;
		else if (i < mid) {
			const int j = i + (n + 1) / 2;
			return (buf_s[i] < buf_s[j]) == ascend ? i: (no_shift ? j: j - 1);
		} else {
			const int j = i - (n + 1) / 2;
			return (buf_s[j] < buf_s[i]) == ascend ? (no_shift ? i: i - 1): j;
		}
	}
}

void main() {
	// Get Index in Global Work Group
	const int i = int(gl_GlobalInvocationID.x);
	if (i >= buf_s.length()) return;
	buf_ret[cal_dest(i, (bitCount(buf_t[i] >> level) & 1) == 0)] = buf_s[i];
}
