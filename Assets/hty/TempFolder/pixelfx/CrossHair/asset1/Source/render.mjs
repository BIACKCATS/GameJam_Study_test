import * as mod from './targeting-crosshair.mjs';
import {renderFrames,spriteSheet,contactSheet,save} from '../../MetalSparks/Source/pixelfx/tools/lib/fxlib.mjs';
import {encodePNG} from '../../MetalSparks/Source/pixelfx/tools/lib/png.mjs';
const fx={id:'ui/targeting-crosshair',meta:{bg:'dark',...mod.meta},mod},r=renderFrames(fx),out=new URL('../Exports/',import.meta.url).pathname.replace(/^\/([A-Z]:)/,'$1');
for(let i=0;i<r.n;i++)save(out+`Targeting_Crosshair_${String(i+1).padStart(2,'0')}.png`,encodePNG(r.w,r.h,r.frames[i].clear));
const sheet=spriteSheet(fx,r,'Targeting_Crosshair_Spritesheet');save(out+'Targeting_Crosshair_Spritesheet.png',sheet.png);save(out+'Targeting_Crosshair_Spritesheet.json',JSON.stringify(sheet.json,null,2));save(out+'Contact.png',contactSheet(fx,r,{max:14,zoom:1}));save(out+'Targeting_Crosshair_Preview.gif',gif(r.frames));
const binary=r.frames.every(f=>f.clear.every((v,i)=>i%4!==3||v===0||v===255)),hold=[11,12,13].every(i=>Buffer.from(r.frames[i].clear).equals(Buffer.from(r.frames[10].clear)));
if(!binary||!hold||r.n!==14)throw Error('Validation failed');save(out+'Validation.json',JSON.stringify({frames:r.n,fps:16,seconds:14/16,resolution:[320,180],mainRingDiameter:112,accentDiameter:128,horizontalSpan:297,verticalSpan:161,binaryAlpha:binary,finalFourIdentical:hold,holdSeconds:.25},null,2));console.log('14 frames; binary alpha; last four identical');
function gif(frames){const parts=[Buffer.from('GIF89a'),Buffer.from([64,1,180,0,0,0,0])];const delays=Array.from({length:14},(_,i)=>Math.round((i+1)*6.25)-Math.round(i*6.25));
for(let i=0;i<frames.length;i++){const rgba=frames[i].clear,palette=[0],map=new Map([[0,0]]),indices=[];for(let j=0;j<rgba.length;j+=4){const key=rgba[j+3]?(rgba[j]<<16|rgba[j+1]<<8|rgba[j+2]):0;if(!map.has(key)){map.set(key,palette.length);palette.push(key);}indices.push(map.get(key));}if(palette.length>256)throw Error('GIF palette overflow');
parts.push(Buffer.from([33,249,4,9,delays[i],0,0,0,44,0,0,0,0,64,1,180,0,135]));const pal=Buffer.alloc(768);palette.forEach((c,k)=>{pal[k*3]=c>>16;pal[k*3+1]=c>>8;pal[k*3+2]=c;});parts.push(pal,Buffer.from([8]));const codes=[];indices.forEach((v,k)=>{if(k%200===0)codes.push(256);codes.push(v);});codes.push(257);let bits=0,acc=0;const bytes=[];for(const c of codes){acc|=c<<bits;bits+=9;while(bits>=8){bytes.push(acc&255);acc>>>=8;bits-=8;}}if(bits)bytes.push(acc&255);for(let k=0;k<bytes.length;k+=255){const sub=bytes.slice(k,k+255);parts.push(Buffer.from([sub.length,...sub]));}parts.push(Buffer.from([0]));}parts.push(Buffer.from([59]));return Buffer.concat(parts);}



