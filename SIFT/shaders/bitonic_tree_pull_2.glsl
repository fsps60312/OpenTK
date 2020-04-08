#version 460

layout(std430,  binding = 0)           buffer t    { int buf_t[]; };

void main() {
	// Get Index in Global Work Group
	const int i = int(gl_GlobalInvocationID.x);
	if (i >= buf_t.length()) return;
	buf_t[i] = buf_t[i] >> 1;
}
