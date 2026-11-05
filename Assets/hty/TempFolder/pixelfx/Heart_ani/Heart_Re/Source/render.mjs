import * as mod from './heart-heal.mjs';
import {renderFrames,spriteSheet,contactSheet,save} from '../../MetalSparks/Source/pixelfx/tools/lib/fxlib.mjs';
import {encodePNG} from '../../MetalSparks/Source/pixelfx/tools/lib/png.mjs';
import {readFileSync} from 'node:fs';import {createHash} from 'node:crypto';
const fx={id:'ui/heart-heal',meta:{bg:'dark',...mod.meta},mod},r=renderFrames(fx),out=new URL('../Exports/',import.meta.url).pathname.replace(/^\/([A-Z]:)/,'$1');
const base=readFileSync(new URL('../../HeartShimmer/Exports/heart-base.png',import.meta.url));
const hash=b=>createHash('sha256').update(b).digest('hex').toUpperCase();
if(hash(base)!=='C5DC55E45BA8ADA7088F3859D020419B8F51508CFE10785B0E568EFD1501A10C')throw Error('Original changed');
for(let i=0;i<r.n;i++)save(out+`Heart_Heal_${String(i+1).padStart(2,'0')}.png`,encodePNG(r.w,r.h,r.frames[i].clear));
const sheet=spriteSheet(fx,r,'Heart_Heal_Spritesheet');save(out+'Heart_Heal_Spritesheet.png',sheet.png);save(out+'Heart_Heal_Spritesheet.json',JSON.stringify(sheet.json,null,2));save(out+'Contact.png',contactSheet(fx,r,{max:10,zoom:4}));
const finalHash=hash(encodePNG(48,48,r.frames[9].clear));if(finalHash!==hash(base))throw Error('Final frame differs from original');
save(out+'Validation.json',JSON.stringify({frames:r.n,fps:15,playSeconds:10/15,size:[48,48],baseHash:hash(base),finalHash,PNGBytesIdentical:true,opaquePixels:r.frames.map(f=>f.clear.filter((v,i)=>i%4===3&&v>0).length)},null,2));
// Encode exact-color, transparent single-play GIF; input frames; return GIF buffer. GIF timing is quantized to centiseconds.
function gif(frames){const parts=[Buffer.from('GIF89a'),Buffer.from([48,0,48,0,0,0,0])];const delays=[7,6,7,7,6,7,7,6,7,7];
for(let i=0;i<frames.length;i++){const rgba=frames[i].clear,palette=[0],map=new Map([[0,0]]),indices=[];for(let j=0;j<rgba.length;j+=4){const key=rgba[j+3]?(rgba[j]<<16|rgba[j+1]<<8|rgba[j+2]):0;if(!map.has(key)){map.set(key,palette.length);palette.push(key);}indices.push(map.get(key));}if(palette.length>256)throw Error('GIF palette overflow');
parts.push(Buffer.from([33,249,4,9,delays[i],0,0,0,44,0,0,0,0,48,0,48,0,135]));const pal=Buffer.alloc(768);palette.forEach((c,k)=>{pal[k*3]=c>>16;pal[k*3+1]=c>>8;pal[k*3+2]=c;});parts.push(pal,Buffer.from([8]));const codes=[];indices.forEach((v,k)=>{if(k%200===0)codes.push(256);codes.push(v);});codes.push(257);let bits=0,acc=0;const bytes=[];for(const c of codes){acc|=c<<bits;bits+=9;while(bits>=8){bytes.push(acc&255);acc>>>=8;bits-=8;}}if(bits)bytes.push(acc&255);for(let k=0;k<bytes.length;k+=255){const sub=bytes.slice(k,k+255);parts.push(Buffer.from([sub.length,...sub]));}parts.push(Buffer.from([0]));}parts.push(Buffer.from([59]));return Buffer.concat(parts);}
save(out+'Heart_Heal_Preview.gif',gif(r.frames));console.log('10 frames; final PNG SHA256 equals canonical original');
