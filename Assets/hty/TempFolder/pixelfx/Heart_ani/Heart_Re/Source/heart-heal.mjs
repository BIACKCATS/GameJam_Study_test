import {readFileSync} from 'node:fs';
import {hex} from '../../MetalSparks/Source/pixelfx/src/core/index.js';
const pixels=JSON.parse(readFileSync(new URL('./canonical-pixels.json',import.meta.url),'utf8').replace(/^\uFEFF/,''));
export const meta={name:'Canonical heart heal',desc:'The original heart fills upward and settles into its exact source pixels.',size:[48,48],origin:[24,24],duration:.6,fps:15,loop:false,options:{seed:1}};
// Draw source pixels in their immutable positions with a rising restoration front; inputs painter,time,options; returns void.
export function draw(p,t,o){const f=Math.min(9,Math.floor(t*15+1e-7)),front=[36,33,30,27,24,21,17,13,8,0][f];
for(const [x,y,c] of pixels){const edge=front+(f<8?([0,1,0,-1][Math.floor(x/4)%4]):0);if(y<edge){p.rect(x-24,y-24,1,1,hex(f===0?'#260d18':f===1?'#391121':'#531529'));continue;}let color=c;if(f>0&&f<8&&y===edge)color='#ffb2bf';p.rect(x-24,y-24,1,1,hex(color));}
if(f>0&&f<8){const x=f%2?14:33,y=front-3;p.rect(x-24,y-24,1,1,hex('#ffe9ec'));}
if(f===8){for(const [x,y] of [[17,16],[16,17],[17,17],[18,17],[17,18]])p.rect(x-24,y-24,1,1,hex('#ffe9ec'));}
}


