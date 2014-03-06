var scrollX = 5.0* (60.0f/100.0f);
var scrollY = 5.0* (60.0f/100.0f);

function Update() {
    var offset : float = (60.0f/100.0f) * scrollX;
    var offset2 : float = (60.0f/100.0f) * scrollY;
    renderer.material.SetTextureOffset ("_ShieldTexture", Vector2(offset, offset2));
    renderer.material.SetTextureOffset ("_ShieldTex2", Vector2(offset, offset2)*2);
}