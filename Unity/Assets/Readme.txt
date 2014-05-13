
This package includes 34 main effects + 26 effects of a collision.

Effects are made using particle systems, and uv- animated textures with resolutions from 512x512 to 2048x2048.
Some effects use their own shaders distortion. Mobile and free version of Unity, the package includes shaders without distortion.
Dimensions are approximate to reality. Demo scene has a line height of 2m.
As well, the package contains a set of scripts for ready use of effects, as well as scripts for collision detection based on raycast.

All prefabs effects (except for the effects of the environment ) have a basic status (appearance , motion , waiting, disappearance , collision, destruction).
Well as additional settings ( the Target, the flight speed of the projectile, the distance of the projectile, the time of appearance and disappearance, and other homing ).
To handle collisions shells used raycast, which allows to get rid of physics rigidbody and configure any speed.


Example of how you can create the effect of the projectile and shoot at a target(C#): 

var go = Instantiate (ball1, position, rotation) as GameObject; 
var prefabSettings = go.GetComponent <PrefabSettings> (); 
prefabSettings.Target = target; 
prefabSettings.PrefabStatus = PrefabStatus.FadeInMoveToTarget; 

Alternatively, you can handle the event of collision: 
prefabSettings.CollisionEnter + = (sender, info) => {Someone code}; 

For more detailed settings, refer to file public methods "PrefabSettings". 

Please note that not all fields are working with prefabs. For example, the effect of the aura may not have the speed or purpose, but has the ability to adjust FPS uv-animation.