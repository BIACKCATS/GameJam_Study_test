import {hex} from '../../MetalSparks/Source/pixelfx/src/core/index.js';
export const meta={name:'Layered targeting crosshair V2',desc:'Connected layered rings, inset panels and blade-like axis joints. No scattered dots.',size:[320,180],origin:[160,90],duration:13/16,fps:16,loop:false,options:{seed:1}};
// Draw the connected targeting device with staged ring assembly; inputs painter,time,options; returns void.
export function draw(p,t,o){const f=Math.min(13,Math.floor(t*16+1e-7)),r=[66,66,64,62,60,58,57,56,56,56,56,56,56,56][f],progress=Math.min(1,(f+2)/9),len=Math.round(18+130*Math.min(1,f/5)),h=Math.min(80,len),dark=hex('#272635'),red=hex(f<2?'#b12541':'#e94a65'),hot=hex('#e6b84b'),mid=hex('#43bfc6');
p.rect(-len,-2,len*2+1,5,dark);p.rect(-2,-h,5,h*2+1,dark);p.rect(-len,0,len*2+1,1,red);p.rect(0,-h,1,h*2+1,red);
for(let y=-73;y<=73;y++)for(let x=-73;x<=73;x++){const d=Math.hypot(x,y),a=(Math.atan2(y,x)+Math.PI*2)%(Math.PI/2);if(a>progress*Math.PI/2)continue;
if(Math.abs(d-r)<=3.5)p.rect(x,y,1,1,dark);
if(Math.abs(d-r)<=2)p.rect(x,y,1,1,red);
if(f>=4&&Math.abs(d-(r-7))<=1.5)p.rect(x,y,1,1,dark);
if(f>=4&&Math.abs(d-(r-7))<=.6)p.rect(x,y,1,1,mid);
if(f>=6&&Math.abs(d-(r+8))<=1.5)p.rect(x,y,1,1,dark);
if(f>=6&&Math.abs(d-(r+8))<=.6)p.rect(x,y,1,1,red);
if(f>=7&&a>.16&&a<.64&&d>=r-2&&d<=r+2)p.rect(x,y,1,1,hot);
if(f>=8&&a>.83&&a<1.24&&d>=r-5&&d<=r-3)p.rect(x,y,1,1,red);
}
if(f>=7)for(const s of [-1,1]){p.rect(s*r-3,-6,7,13,dark);p.rect(-6,s*r-3,13,7,dark);p.rect(s*r-2,-5,5,11,hot);p.rect(-5,s*r-2,11,5,hot);}
if(f>=8)for(const s of [-1,1]){for(let d=67;d<=86;d++){const w=Math.max(1,Math.floor((87-d)/5));p.rect(s*d,-w,1,w*2+1,red);}p.rect(s*91-(s<0?48:0),-3,49,1,mid);p.rect(s*91-(s<0?48:0),3,49,1,mid);p.rect(s*91,-3,1,7,red);p.rect(s*139,-3,1,7,red);}
if(f>=8)for(const s of [-1,1]){p.rect(-2,s*70-4,5,9,red);p.rect(-1,s*70-3,3,7,hot);}
if(f>=9){p.rect(-5,-5,11,1,hot);p.rect(-5,5,11,1,hot);p.rect(-5,-5,1,11,hot);p.rect(5,-5,1,11,hot);}
}


