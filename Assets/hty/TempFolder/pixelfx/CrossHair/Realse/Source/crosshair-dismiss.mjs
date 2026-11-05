import {draw as drawBase} from '../../TargetingCrosshair/Source/targeting-crosshair.mjs';
export const meta={name:'V1 crosshair dismiss',desc:'The original targeting assembly unwinds and clears completely.',size:[320,180],origin:[160,90],duration:11/20,fps:20,loop:false,options:{seed:1}};
export const sourceFrames=[13,8,7,6,5,4,3,2,1,0,-2,-1];
// Reverse the original V1 assembly, with a fully empty terminal frame; inputs painter,time,options; returns void.
export function draw(p,t,o){const f=Math.min(11,Math.floor(t*20+1e-7));if(f===11)return;if(f===10){const proxy={rect(x,y,w,h,c){if(w===1&&h===1)return;p.rect(Math.round(x/3),Math.round(y/3),Math.max(1,Math.round(w/3)),Math.max(1,Math.round(h/3)),c);}};drawBase(proxy,0,{});return;}drawBase(p,sourceFrames[f]/16,{});}


