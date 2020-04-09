#version 450

// Input data (from the vertex shaders)
in vec2 UV;

// Ouput data
layout (location = 0) out vec4 color;

// Values
uniform sampler2DRect RenderedTexture;

void main()
{
	const ivec2 sz = textureSize(RenderedTexture);
	color = texture(RenderedTexture, vec2(UV.x*sz.x,UV.y*sz.y));
}
