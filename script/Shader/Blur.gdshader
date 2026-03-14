shader_type canvas_item;

// 模糊强度，值越大越模糊
uniform float blur_size : hint_range(0.0, 10.0) = 2.0;

void fragment() {
    // 获取当前像素的UV坐标和纹理
    vec2 uv = UV;
    vec4 color = texture(TEXTURE, uv);
    
    // 如果blur_size为0，就不模糊，直接显示原图
    if (blur_size > 0.0) {
        // 计算每个采样步长（一个像素在UV空间中的大小）
        vec2 ps = TEXTURE_PIXEL_SIZE * blur_size;
        
        // 采样周围9个点的颜色并平均
        color = texture(TEXTURE, uv + vec2(-ps.x, -ps.y)) * 0.0625; // 左上
        color += texture(TEXTURE, uv + vec2(0.0, -ps.y)) * 0.125;   // 中上
        color += texture(TEXTURE, uv + vec2(ps.x, -ps.y)) * 0.0625; // 右上
        color += texture(TEXTURE, uv + vec2(-ps.x, 0.0)) * 0.125;   // 左中
        color += texture(TEXTURE, uv) * 0.25;                       // 中心（权重最高）
        color += texture(TEXTURE, uv + vec2(ps.x, 0.0)) * 0.125;    // 右中
        color += texture(TEXTURE, uv + vec2(-ps.x, ps.y)) * 0.0625; // 左下
        color += texture(TEXTURE, uv + vec2(0.0, ps.y)) * 0.125;    // 中下
        color += texture(TEXTURE, uv + vec2(ps.x, ps.y)) * 0.0625;  // 右下
    }
    
    // 输出最终颜色
    COLOR = color;
}