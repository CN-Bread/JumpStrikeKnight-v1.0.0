shader_type canvas_item;

void vertex() {
	// Called for every vertex the material is visible on.
	if(VERTEX.y < 0.0){
		vec2 circle = vec2(cos(TIME * 2.0) * 2.0 , sin(TIME * 2.0) * 2.0);
		VERTEX += circle;
	}
}

void fragment() {
	// Called for every pixel the material is visible on.
}
