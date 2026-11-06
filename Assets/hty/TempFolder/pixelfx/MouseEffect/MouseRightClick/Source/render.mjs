import * as mod from './mouse.mjs';
import {renderFrames,contactSheet,save} from '../../MetalSparks/Source/pixelfx/tools/lib/fxlib.mjs';
import {encodePNG} from '../../MetalSparks/Source/pixelfx/tools/lib/png.mjs';
const out=new URL('../Exports/',import.meta.url).pathname.replace(/^\/([A-Z]:)/,'$1');
const fx={id:'Mouse_RightClick',meta:{bg:'dark',...mod.meta},mod},r=renderFrames(fx),e=renderFrames({...fx,mod:{...mod,draw(p,t){mod.accents(p,mod.frame(t));}}}),b=renderFrames({...fx,mod:{...mod,draw(p,t){mod.mouse(p,mod.frame(t));}}});
// Pack 24 RGBA frames into a six-column sprite sheet; frames input, name output stem; returns void.
function sheet(frames,name){const pixels=new Uint8Array(768*512*4);frames.forEach((f,i)=>{for(let y=0;y<128;y++)pixels.set(f.clear.subarray(y*512,(y+1)*512),((Math.floor(i/6)*128+y)*768+(i%6)*128)*4);});save(out+name+'.png',encodePNG(768,512,pixels));}
for(let i=0;i<24;i++)save(out+`Mouse_RightClick_${String(i+1).padStart(2,'0')}.png`,encodePNG(128,128,r.frames[i].clear));
sheet(r.frames,'Mouse_RightClick_Spritesheet');sheet(e.frames,'Mouse_RightClick_Effect_Spritesheet');sheet(b.frames,'Mouse_RightClick_Body_Spritesheet');
save(out+'Mouse_RightClick_Base.png',encodePNG(128,128,b.frames[0].clear));save(out+'Mouse_RightClick_Pressed.png',encodePNG(128,128,b.frames[7].clear));
save(out+'Mouse_RightClick_Preview.gif',gif(r.frames));save(out+'Contact.png',contactSheet(fx,r,{max:12,zoom:2}));
save(out+'Mouse_RightClick_Frames.json',JSON.stringify({width:128,height:128,columns:6,rows:4,fps:16,loop:true,frames:Array.from({length:24},(_,i)=>({file:`Mouse_RightClick_${String(i+1).padStart(2,'0')}.png`,x:i%6*128,y:Math.floor(i/6)*128,w:128,h:128,durationMs:62.5}))},null,2));
function gif(frames){const parts=[Buffer.from('GIF89a'),Buffer.from([128,0,128,0,0,0,0])];const delays=Array.from({length:24},(_,i)=>Math.round((i+1)*(100/16))-Math.round(i*(100/16)));
parts.push(Buffer.from([33,255,11,...Buffer.from("NETSCAPE2.0"),3,1,0,0,0]));for(let i=0;i<frames.length;i++){const rgba=frames[i].clear,palette=[0],map=new Map([[0,0]]),indices=[];for(let j=0;j<rgba.length;j+=4){const key=rgba[j+3]>[32,160,224,96][((j/4)%128)%2+2*(Math.floor(j/512)%2)]?(rgba[j]<<16|rgba[j+1]<<8|rgba[j+2]):0;if(!map.has(key)){map.set(key,palette.length);palette.push(key);}indices.push(map.get(key));}if(palette.length>256)throw Error('GIF palette overflow');
parts.push(Buffer.from([33,249,4,9,delays[i],0,0,0,44,0,0,0,0,128,0,128,0,135]));const pal=Buffer.alloc(768);palette.forEach((c,k)=>{pal[k*3]=c>>16;pal[k*3+1]=c>>8;pal[k*3+2]=c;});parts.push(pal,Buffer.from([8]));const codes=[];indices.forEach((v,k)=>{if(k%200===0)codes.push(256);codes.push(v);});codes.push(257);let bits=0,acc=0;const bytes=[];for(const c of codes){acc|=c<<bits;bits+=9;while(bits>=8){bytes.push(acc&255);acc>>>=8;bits-=8;}}if(bits)bytes.push(acc&255);for(let k=0;k<bytes.length;k+=255){const sub=bytes.slice(k,k+255);parts.push(Buffer.from([sub.length,...sub]));}parts.push(Buffer.from([0]));}parts.push(Buffer.from([59]));return Buffer.concat(parts);}









