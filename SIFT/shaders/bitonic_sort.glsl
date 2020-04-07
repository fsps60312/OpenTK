#version 460

layout(std430,  binding = 0) buffer s    { int buf_s[]; };

//void bitonic_merge(int n, int *s, bool ascend) {
//	for (int i = 0; i < n / 2; i++) {
//		if ((s[i] <= s[i + n / 2]) != ascend) {
//			int v = s[i];
//			s[i] = s[i + n / 2];
//			s[i + n / 2] = v;
//		}
//	}
//	bitonic_merge(n / 2, s, ascend);
//	bitonic_merge(n / 2, s + n / 2, ascend);
//}
//
//void bitonic_sort(int n, int *s, bool ascend) {
//	if (n == 1) return;
//	
//	bitonic_sort(n / 2, s, true);
//	bitonic_sort(n / 2, s + n / 2, false);
//
//	bitonic_merge(n, s, ascend);
//}

void main()
{
	// Get Index in Global Work Group
	const int i = int(gl_GlobalInvocationID.x);
	// n must be power of 2
	const int n = buf_s.length();
	if (bitCount(n) != 1) return;

	for (int b = 1; (n >> b) > 0; b++) {
		// sort chunks of size 1<<b, ascend = TFFTFTTF...
		bool ascend = (bitCount(i >> b) & 1) == 0;
		// bitonic merge
		for (int _ = b - 1; _ >= 0; _--) {
			const int j = i ^ (1 << _);
			if (j < i && (buf_s[j] < buf_s[i]) != ascend) {
				// swap
				int v = buf_s[i];
				buf_s[i] = buf_s[j];
				buf_s[j] = v;
			}
			memoryBarrierBuffer();
		}
	}
}
