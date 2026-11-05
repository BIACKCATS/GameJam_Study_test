import {readFileSync} from 'node:fs';
import {hex} from '../../MetalSparks/Source/pixelfx/src/core/index.js';
const pixels=JSON.parse(readFileSync(new URL('./canonical-pixels.json',import.meta.url),'utf8').replace(/^\uFEFF/,''));
export const meta={name:'Canonical heart fracture',desc:'The exact canonical red heart cracks, separates into red pieces and disappears.',tags:['heart','break'],size:[48,48],origin:[24,24],duration:.75,loop:false,fps:12,options:{seed:1}};
// Purpose: define a fixed zigzag split through the source heart; parameter: source y; return: source x.
function crack(y){return 24+[0,-1,0,2,1,-1,-2,0][Math.max(0,Math.floor((y-12)/3))%8];}
// Purpose: draw canonical source pixels as separating fragments; parameters: painter/time/options; return: void.
export function draw(p,t,o){
 const f=Math.min(9,Math.floor(t*12+.00001));if(f===9)return;
 for(const [x,y,c] of pixels){
  if(f<2){p.rect(x-24,y-24,1,1,hex(c));continue;}
  const split=crack(y),left=x<split;
  if(f===2||f===3){if(Math.abs(x-split)<(f===3?1.1:.6)&&(f===3||y<26))continue;p.rect(x-24,y-24,1,1,hex(c));continue;}
  const cut1=22+Math.floor((x-24)*.25),cut2=30-Math.floor((x-24)*.18); const band=y<cut1?0:y<cut2?1:2;
  const cx=left?17:30,cy=[17,25,32][band],dx=(left?-1:1)*[2,4,7,9,11][f-4],dy=[-1,0,2][band]*(f-3)*.6;
  const scale=[1,1,.76,.43,.18][f-4];
  if(f>=4 && (Math.abs(x-split)<1.1 || y===cut1 || y===cut2))continue;
  p.rect(Math.round(cx+(x-cx)*scale+dx)-24,Math.round(cy+(y-cy)*scale+dy)-24,1,1,hex(c));
 }
}


