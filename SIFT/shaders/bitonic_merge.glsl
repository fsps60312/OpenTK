#version 460

layout(std430,  binding = 0) buffer s    { int buf_s[]; };
layout(std430,  binding = 1) buffer t    { int buf_t[]; };
layout(std430,  binding = 2) buffer l    { int buf_l[]; };
layout(std430,  binding = 3) buffer r    { int buf_r[]; };
// return
layout(std430,  binding = 4) buffer ret  { int buf_ret[]; };

uniform int level;

int cal_dest(const int i, const bool ascend) {
	const int l = buf_l[i], r = buf_r[i];
	const int mid = (l + r) / 2;
	const int n = r - l + 1;
	const int v = buf_s[i];
	if (i <= mid) {
		const int j = i + (n + 1) / 2;
		if ((n & 1) == 0) { // even: OOOOXXXX
			return (v < buf_s[j]) == ascend ? i: j;
		} else {            // odd: OOOOXXX
			bool need_shift = (buf_s[mid] < buf_s[r]) != ascend;
			if (i == mid) return need_shift? r: i;
			else {
				return (v < buf_s[j]) == ascend ? i: (need_shift ? j - 1: j);
			}
		}
	} else {
		const int j = i - (n + 1) / 2;
		if ((n & 1) == 0) { // even: OOOOXXXX
			return (buf_s[j] < v) == ascend ? i: j;
		} else {            // odd: OOOOXXX
			bool need_shift = (buf_s[mid] < buf_s[r]) != ascend;
			return (buf_s[j] < v) == ascend ? (need_shift ? i - 1: i): j;
		}
	}
}

void main() {
	// Get Index in Global Work Group
	const int i = int(gl_GlobalInvocationID.x);
	if (i >= buf_s.length()) return;
	buf_ret[cal_dest(i, (bitCount(buf_t[i] >> level) & 1) == 0)] = buf_s[i];
}
