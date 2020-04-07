#version 460

layout(std430,  binding = 0) buffer s    { int buf_s[]; };

void main()
{
	// Get Index in Global Work Group
	const int i = int(gl_GlobalInvocationID.x);
	const int n = buf_s.length();

	for (int offset = 1; 0 <= i - offset; offset <<= 1) {
		const int t = buf_s[i - offset] + buf_s[i];
		barrier();
		buf_s[i] = t;
		barrier();
	}
}
