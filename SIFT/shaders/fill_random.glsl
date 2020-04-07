#version 460

// warning: only length of power of 2 supported

layout(std430,  binding = 0) buffer s    { int buf_s[]; };

uniform uint initial_seed;

uint random_next(uint seed) {
	seed *= 0xdefaced;
	seed += 904070042u;
	seed += seed >> 20;
	return seed;
}

void main()
{
	// Get Index in Global Work Group
	const uint i = gl_GlobalInvocationID.x;
	const uint n = buf_s.length();
	if (i >= n) return;
	uint seed = random_next(initial_seed ^ i);
	buf_s[i] = int(seed);
}
