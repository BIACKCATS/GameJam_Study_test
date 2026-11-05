import {hex} from '../../MetalSparks/Source/pixelfx/src/core/index.js';
export const meta={name:'Large modern targeting overlay',desc:'A bold circular target assembles with long axes and clean segmented accents.',size:[320,180],origin:[160,90],duration:13/16,fps:16,loop:false,options:{seed:1}};
// Draw deterministic concentric assembly with long screen axes; parameters painter,time,options; return void.
export function draw(p,t,o){const f=Math.min(13,Math.floor(t*16+1e-7)),k=Math.min(1,f/10),radius=[68,68,66,64,62,60,58,57,56,56,56,56,56,56][f],reveal=Math.min(1,(f+1)/8),len=Math.round(12+136*Math.min(1,f/5)),height=Math.min(80,len),dark=hex('#410f1b'),red=hex(f<2?'#9b2035':'#ff334d'),hot=hex('#ff785c');
// Dark-red keylines maintain separation on bright scenes without filling the background.
p.rect(-len,-1,len*2+1,3,dark);p.rect(-1,-height,3,height*2+1,dark);p.rect(-len,0,len*2+1,1,red);p.rect(0,-height,1,height*2+1,red);
for(let y=-72;y<=72;y++)for(let x=-72;x<=72;x++){const d=Math.hypot(x,y),a=(Math.atan2(y,x)+Math.PI*2)%(Math.PI/2),active=a<reveal*Math.PI/2;
if(active&&Math.abs(d-radius)<=3)p.rect(x,y,1,1,dark);
if(active&&Math.abs(d-radius)<=1.5)p.rect(x,y,1,1,red);
if(f>=6&&Math.abs(d-(radius+8))<=1.6&&a>.24&&a<.24+.68*Math.min(1,(f-5)/5))p.rect(x,y,1,1,dark);
if(f>=6&&Math.abs(d-(radius+8))<=.65&&a>.24&&a<.24+.68*Math.min(1,(f-5)/5))p.rect(x,y,1,1,hot);
}
if(f>=7){for(const s of [-1,1]){p.rect(s*radius-2,-5,5,11,red);p.rect(-5,s*radius-2,11,5,red);p.rect(s*radius-1,-3,3,7,hot);p.rect(-3,s*radius-1,7,3,hot);}}
if(f>=9){for(const sx of [-1,1])for(const sy of [-1,1]){p.rect(sx*74,sy*10,1,7,red);p.rect(sx*74-(sx>0?7:0),sy*10,8,1,red);}p.rect(-2,-2,5,5,dark);p.rect(-1,-1,3,3,hot);for(const s of [-1,1]){p.rect(s*20-2,-1,5,3,red);p.rect(-1,s*20-2,3,5,red);}}
}


