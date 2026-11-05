import {effect} from './fireworks.mjs';
import {renderFrames,spriteSheet,contactSheet,save} from '../../MetalSparks/Source/pixelfx/tools/lib/fxlib.mjs';
import {encodePNG} from '../../MetalSparks/Source/pixelfx/tools/lib/png.mjs';
const out=new URL('../Exports/',import.meta.url).pathname.replace(/^\/([A-Z]:)/,'$1'),renders=[];
for(let kind=0;kind<2;kind++){
 const mod=effect(kind),name=kind?'Firework_RedGreen':'Firework_PurpleBlue',fx={id:name,meta:{bg:'dark',...mod.meta},mod},r=renderFrames(fx);renders.push(r);
 for(let i=0;i<r.n;i++)save(out+`${name}_${String(i+1).padStart(2,'0')}.png`,encodePNG(160,160,r.frames[i].clear));
 const s=spriteSheet(fx,r,name+'_Spritesheet');save(out+name+'_Spritesheet.png',s.png);save(out+name+'_Spritesheet.json',JSON.stringify(s.json,null,2));save(out+name+'_Preview.gif',gif(r.frames));save(out+name+'_Contact.png',contactSheet(fx,r,{max:8,zoom:2}));
}
const comparison=new Uint8Array(320*160*4);for(let y=0;y<160;y++)for(let k=0;k<2;k++)comparison.set(renders[k].frames[5].clear.subarray(y*160*4,(y+1)*160*4),(y*320+k*160)*4);save(out+'Fireworks_Comparison.png',encodePNG(320,160,comparison));
console.log(renders.map(r=>({frames:r.n,width:r.w,height:r.h,fps:r.fps})));
function gif(frames){const parts=[Buffer.from('GIF89a'),Buffer.from([160,0,160,0,0,0,0])];const delays=Array.from({length:16},(_,i)=>Math.round((i+1)*(100/16))-Math.round(i*(100/16)));
parts.push(Buffer.from([33,255,11,...Buffer.from("NETSCAPE2.0"),3,1,0,0,0]));for(let i=0;i<frames.length;i++){const rgba=frames[i].clear,palette=[0],map=new Map([[0,0]]),indices=[];for(let j=0;j<rgba.length;j+=4){const key=rgba[j+3]?(rgba[j]<<16|rgba[j+1]<<8|rgba[j+2]):0;if(!map.has(key)){map.set(key,palette.length);palette.push(key);}indices.push(map.get(key));}if(palette.length>256)throw Error('GIF palette overflow');
parts.push(Buffer.from([33,249,4,9,delays[i],0,0,0,44,0,0,0,0,160,0,160,0,135]));const pal=Buffer.alloc(768);palette.forEach((c,k)=>{pal[k*3]=c>>16;pal[k*3+1]=c>>8;pal[k*3+2]=c;});parts.push(pal,Buffer.from([8]));const codes=[];indices.forEach((v,k)=>{if(k%200===0)codes.push(256);codes.push(v);});codes.push(257);let bits=0,acc=0;const bytes=[];for(const c of codes){acc|=c<<bits;bits+=9;while(bits>=8){bytes.push(acc&255);acc>>>=8;bits-=8;}}if(bits)bytes.push(acc&255);for(let k=0;k<bytes.length;k+=255){const sub=bytes.slice(k,k+255);parts.push(Buffer.from([sub.length,...sub]));}parts.push(Buffer.from([0]));}parts.push(Buffer.from([59]));return Buffer.concat(parts);}







