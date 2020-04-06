#version 430

layout(rgba16f, binding = 0) uniform image2D img_output;

struct Triangle{
	mat3 verticez;
	mat3x2 uv;
	int material_id;
};
struct Material{
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
	float specular_exp;
	ivec3 diffuse_texture;
	float alpha;
};

layout(std430,  binding = 0) buffer trianglez    { Triangle buf_triangle[]; };
layout(std430,  binding = 1) buffer materials    { Material buf_material[]; };
layout(std430,  binding = 2) buffer textures     { vec4     buf_texture[];  };
layout(std430,  binding = 3) buffer lights       { int light_count; vec4 buf_light[]; };
layout(std430,  binding = 4) buffer bvh_nodes    { ivec3 buf_bvh_node[];    };
layout(std430,  binding = 5) buffer bvh_aabbs    { mat2x3 buf_bvh_aabb[];   };
layout(std430,  binding = 6) buffer bvh_ranges   { ivec2 buf_bvh_range[];   };

const float PI    = acos(-1);
// https://stackoverflow.com/questions/16069959/glsl-how-to-ensure-largest-possible-float-value-without-overflow
const float FLT_MAX=3.402823466e+38f;

uniform int tri_num;

uniform vec3  eye;   // vec3( -0.75f,  1.40f,  2.25f );
uniform vec3  view;  // horizontalAngle = 2.5f, verticalAngle = -0.5f;
uniform vec3  up;
uniform float fov;   // 90.0f;
uniform uint initial_random_seed;

struct Ray {
	int   pre_obj, obj;
	float t, i;
	vec3  pos, dir, n;
	vec2  uv;
};

// Returns a random number based on a vec3.
uint random_seed;
uint random() {
	random_seed*=0xdefaced;
	random_seed+=0x20190918;
	random_seed+=random_seed>>20;
	return random_seed;
}
//https://stackoverflow.com/questions/4200224/random-noise-functions-for-glsl
float random_float(){
	const uint ieee_mantissa=0x007FFFFFu;
	const uint ieee_one    =0x3F800000u;
	uint r=random();
	r&=ieee_mantissa;
	r|=ieee_one;
	float f=uintBitsToFloat(r);
	return f-1.0f;
}

mat3 GetTriangle(in int obj_id){
	mat3 tri=buf_triangle[obj_id].verticez;
	return tri;
}

void TriangleIntersect(inout Ray r,in int obj_id){
	if(obj_id==r.pre_obj)return;
	mat3 tri=GetTriangle(obj_id);
	
	vec3  o  = r.pos - tri[0],
		  t1 = tri[1] - tri[0],
		  t2 = tri[2] - tri[0];
	
	if(dot(cross(t1,t2),-r.dir)<0.0f)return;
	/*
	// Face Culling
	if(dot(cross(t1, t2), -r.dir) < 0.0f)
	{
		continue;
	}
	*/
	vec3 t_c=cross(t1,t2);
	vec3 s1_c=cross(r.dir,t2);
	vec3 s2_c=cross(r.dir,t1);
	float t_i=dot(t_c,r.dir);
	float s1_i=dot(s1_c,t1);
	float s2_i=dot(s2_c,t2);
	
	if(t_i != 0.0f && s1_i!=0.0f && s2_i!=0.0f)
	{
		float s1 = dot(s1_c, o) / s1_i,
			  s2 = dot(s2_c, o) / s2_i,
			  t  = dot(t_c, -o) / t_i;
		
		if(t > 0.0f && t < r.t && s1 >= 0.0f && s2 >= 0.0f && s1+s2 <= 1.0f)
		{
			r.obj = obj_id;
			r.t   = t;
			r.uv  = vec2(s1,s2);
		}
	}
}

float AABBIntersect(in Ray r, in int aabb_id){
	// if(aabb_id==-1)return FLT_MAX;
	// r.pos, r.dir
	const mat2x3 aabb=buf_bvh_aabb[aabb_id];
	const vec3 v1=(aabb[0]-r.pos)/r.dir;
	const vec3 v2=(aabb[1]-r.pos)/r.dir;
	const vec3 mn=min(v1,v2),mx=max(v1,v2);
	const float mn_v=max(max(mn.x,mn.y),mn.z),mx_v=min(min(mx.x,mx.y),min(mx.z,r.t));
	return mn_v<=mx_v?mn_v:FLT_MAX;
}

void BVHIntersect(inout Ray r){
	//for(int i=0;i<tri_num;i++)TriangleIntersect(r,i);
	int id=1;
	uint goto_sibling=0;
	while(true){
		// trace down
		while(true){
			const ivec3 node=buf_bvh_node[id];
			if(node.x==-1){
				for(int i=buf_bvh_range[id].x;i<=buf_bvh_range[id].y;i++)TriangleIntersect(r,i);
				break;
			}
			const float tx=AABBIntersect(r,node.x);
			const float ty=AABBIntersect(r,node.y);
			if(min(tx,ty)==FLT_MAX)break;
			id=tx<=ty?node.x:node.y;
			goto_sibling<<=1;
			if(max(tx,ty)!=FLT_MAX)goto_sibling|=1;
		}
		// trace back
		while(goto_sibling!=0&&(goto_sibling&1)==0){
			goto_sibling>>=1;
			id=buf_bvh_node[id].z;
		}
		if(goto_sibling==0)return;
		id^=1;
		goto_sibling^=1;
	}
}

bool HitTest(in Ray r){
	int id=1;
	uint goto_sibling=0;
	const float initial_t=r.t;
	while(true){
		// trace down
		while(true){
			const ivec3 node=buf_bvh_node[id];
			if(node.x==-1){
				for(int i=buf_bvh_range[id].x;i<=buf_bvh_range[id].y;i++)TriangleIntersect(r,i);
				if(r.t!=initial_t)return true;
				break;
			}
			const float tx=AABBIntersect(r,node.x);
			const float ty=AABBIntersect(r,node.y);
			if(min(tx,ty)==FLT_MAX)break;
			id=tx<=ty?node.x:node.y;
			goto_sibling<<=1;
			if(max(tx,ty)!=FLT_MAX)goto_sibling|=1;
		}
		// trace back
		while(goto_sibling!=0&&(goto_sibling&1)==0){
			goto_sibling>>=1;
			id=buf_bvh_node[id].z;
		}
		if(goto_sibling==0)return false;
		id^=1;
		goto_sibling^=1;
	}
}

void Intersect(inout Ray r){
	r.t=FLT_MAX;
	r.pre_obj=r.obj;
	r.obj=-1;
	BVHIntersect(r);
	if(r.obj != -1)
	{
		int  id = r.obj;
		mat3 tri=GetTriangle(id);
		
		vec3 t1 = tri[1] - tri[0],
			 t2 = tri[2] - tri[0];
		r.n = normalize(cross(t1, t2));
		r.pos+=r.dir*r.t;
	}
}

vec3 GetTextureColor(in ivec3 info,in vec2 uv){
	const int width=info.x,height=info.y,id=info.z;
	const int x=clamp(int(round(uv.x*(width-1))),0,width-1),y=clamp(int(round(uv.y*(height-1))),0,height-1);
	return buf_texture[id+y*width+x].rgb;
}

void GetColors(in Triangle tri,in vec2 uv,out vec3 ambient_color,out vec3 diffuse_color,out vec3 specular_color,out float ks_exp){
	const Material mtl=buf_material[tri.material_id];
	specular_color = mtl.specular;
	ks_exp         = mtl.specular_exp;
	if(mtl.diffuse_texture.z==-1){
		ambient_color  = mtl.ambient;
		diffuse_color  = mtl.diffuse;
	}else{
		diffuse_color=GetTextureColor(mtl.diffuse_texture,tri.uv[1]*uv.x+tri.uv[2]*uv.y+tri.uv[0]*(1.0f-uv.x-uv.y));
		ambient_color=diffuse_color*0.5f;
	}
}

vec3 PhongLighting(in Ray ray){
	vec3 ambient_color,diffuse_color,specular_color;
	float ks_exp;
	GetColors(buf_triangle[ray.obj],ray.uv,ambient_color,diffuse_color,specular_color,ks_exp);

	vec3 ret = ambient_color;
	for(int i=0;i<light_count;i++){
		const vec3 light_pos=buf_light[i].xyz;
		const float light_power=buf_light[i].w;

		const vec3 light_dir = normalize(light_pos - ray.pos);
		const float dist = distance(light_pos, ray.pos); // light_power = 100.0f;
		/*struct Ray {
			int   pre_obj, obj;
			float t, i;
			vec3  pos, dir, n;
		};*/
		Ray ray_light = Ray(-1, ray.obj, dist /*FLT_MAX*/, 0.0f, ray.pos + 1e-5f * light_dir, light_dir, vec3(0.0f),vec2(0.0f));

		if(!HitTest(ray_light)){
			vec3  h   = normalize(-ray.dir + light_dir);
			
			float diffuse_factor  = clamp(dot(ray.n, light_dir), 0.0f, 1.0f);
			float specular_factor = pow(clamp(dot(ray.n, h), 0.0f, 1.0f), ks_exp);

			ret += diffuse_color  * light_power * diffuse_factor  / (dist * dist);
			ret += specular_color * light_power * specular_factor / (dist * dist);
		}
	}
	return ret;
}

vec3 RayTracing(in Ray r)
{
	vec3 rgb = vec3(0.0f);
	
	while(r.i > 0.1f)
	{
		// Intersect
		Intersect(r);
		
		if(r.obj == -1)break;
		const float alpha = buf_material[buf_triangle[r.obj].material_id].alpha;
		rgb += PhongLighting(r) * r.i * alpha;
		r.i *= (1.0f - alpha);
		r.pos += 1e-5f * r.dir;
	}
	
	return rgb;
}

void main()
{
	random_seed=initial_random_seed^(gl_GlobalInvocationID.x)^(1000000007*gl_GlobalInvocationID.y);
	// Get Index in Global Work Group
	ivec2 pixel_coords = ivec2(gl_GlobalInvocationID.xy);
	
	ivec2 dims   = imageSize(img_output);
	vec3  dx     = normalize(cross(view, up));
    vec3  dy     = normalize(cross(view, dx));
	vec3  center = eye + view * (dims.x / 2.0) / tan(fov / 2.0);
	float x =   pixel_coords.x - dims.x/2.0;
	float y = -(pixel_coords.y - dims.y/2.0);
	
	// Generate Ray
	vec3 pos = eye;
	vec3 dir = normalize(center + x * dx + y * dy - eye);
	Ray  ray_start = Ray(-1, -1, FLT_MAX, 1, pos, dir, vec3(0.0f),vec2(0.0f));
	
	// Ray Tracing
	vec4 pixel = vec4(RayTracing(ray_start), 1.0f);
	
	// Output Color
	imageStore(img_output, pixel_coords, pixel);
}
