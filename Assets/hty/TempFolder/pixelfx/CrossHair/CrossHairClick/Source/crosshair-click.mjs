import {draw as drawBase} from '../../TargetingCrosshair/Source/targeting-crosshair.mjs';
import {hex} from '../../MetalSparks/Source/pixelfx/src/core/index.js';
export const meta={name:'V1 click impulse enhanced',desc:'A decisive center impulse propagates into the original sight and settles.',size:[320,180],origin:[160,90],duration:7/24,fps:24,loop:false,options:{seed:1}};
export const scales=[1,.96,.91,1.04,1.015,.99,1,1];
// Render immutable V1 geometry through a centered recoil transform, followed by a short UI impulse; inputs painter,time,options; return void.
export function draw(p,t,o){const f=Math.min(7,Math.floor(t*24+1e-7)),s=scales[f],orange=hex('#ff795d'),cream=hex('#fff0ce'),red=hex('#ff334d');
const proxy={rect(x,y,w,h,c){const left=Math.round(x*s),top=Math.round(y*s),right=Math.round((x+w)*s),bottom=Math.round((y+h)*s);if(f===2&&Math.hypot(x,y)>51&&Math.hypot(x,y)<59&&w===1&&h===1&&c===red)c=orange;p.rect(left,top,Math.max(1,right-left),Math.max(1,bottom-top),c);}};drawBase(proxy,13/16,{});
if(f===2){const r=Math.round(56*s);for(const sign of [-1,1]){p.rect(sign*r-1,-6,3,13,cream);p.rect(-6,sign*r-1,13,3,cream);}}
if(f>=1&&f<=5){const radius=[0,10,20,35,49,57][f],thickness=f===2?1.4:.7;for(let y=-60;y<=60;y++)for(let x=-60;x<=60;x++)if(Math.abs(Math.hypot(x,y)-radius)<=thickness)p.rect(x,y,1,1,f===2?cream:f<4?orange:red);
const length=[0,14,29,21,12,6][f];p.rect(-length,-1,length*2+1,3,red);p.rect(-1,-length,3,length*2+1,red);p.rect(-length,0,length*2+1,1,f<=3?cream:orange);p.rect(0,-length,1,length*2+1,f<=3?cream:orange);
}
}


