import * as mod from './subway-door-wide.mjs';
import {renderFrames,contactSheet,save} from '../../MetalSparks/Source/pixelfx/tools/lib/fxlib.mjs';
import {encodePNG} from '../../MetalSparks/Source/pixelfx/tools/lib/png.mjs';
import {readFileSync} from 'node:fs';import {createHash} from 'node:crypto';
const fx={id:'props/subway-door-wide',meta:{bg:'dark',...mod.meta},mod},r=renderFrames(fx),out=new URL('../Exports/',import.meta.url).pathname.replace(/^\/([A-Z]:)/,'$1');
for(let i=0;i<r.n;i++)save(out+`SubwayDoor_Wide_OpenHoldClose_${String(i+1).padStart(2,'0')}.png`,encodePNG(r.w,r.h,r.frames[i].clear));
const w=426*6,h=307*6,rgba=new Uint8ClampedArray(w*h*4),frames=[];
for(let i=0;i<36;i++){const x=(i%6)*426,y=Math.floor(i/6)*307;for(let row=0;row<307;row++)rgba.set(r.frames[i].clear.subarray(row*426*4,(row+1)*426*4),((y+row)*w+x)*4);frames.push({index:i+1,frame:{x,y,w:426,h:307},durationMs:1000/12,phase:i<9?'opening':i<27?'hold':'closing',offset:mod.offset(i)});}
save(out+'SubwayDoor_Wide_OpenHoldClose_Spritesheet.png',encodePNG(w,h,rgba));save(out+'SubwayDoor_Wide_OpenHoldClose_Spritesheet.json',JSON.stringify({size:[w,h],frameSize:[426,307],fps:12,columns:6,rows:6,frames},null,2));
save(out+'Contact.png',contactSheet(fx,{...r,n:8,frames:[0,3,6,9,26,29,32,35].map(i=>r.frames[i])},{max:8,zoom:1}));
const raw=readFileSync(new URL('./Reference.rgba',import.meta.url)),eq=(a,b)=>Buffer.from(a).equals(Buffer.from(b)),firstExact=eq(raw,r.frames[0].clear),lastExact=eq(raw,r.frames[35].clear),holdExact=r.frames.slice(9,27).every(f=>eq(f.clear,r.frames[9].clear));if(!firstExact||!lastExact||!holdExact||r.n!==36)throw Error('Source or hold validation failed');
save(out+'Validation.json',JSON.stringify({frames:36,fps:12,totalSeconds:3,fullyOpenFrames:[10,27],holdTicks:18,holdSeconds:1.5,firstAndLastRGBAIdenticalToBlackWindowBaseline:true,allHoldFramesIdentical:true,offsets:frames.map(f=>f.offset),referenceSHA256:createHash('sha256').update(readFileSync(new URL('../../../Textures/스크린도어확장.png',import.meta.url))).digest('hex')},null,2));
console.log('36 frames; exact black-window endpoints; fully open ticks 9..26 = 1.5 seconds');
// Store the 1.5-second hold as one GIF image; PNG sequence keeps all eighteen hold ticks for ordinary 12 FPS clips.
const {encodePreview}=await import('./gif.mjs');const delays=Array.from({length:9},(_,i)=>Math.round((i+1)*100/12)-Math.round(i*100/12));const preview=[...r.frames.slice(0,9).map((f,i)=>({rgba:f.clear,delay:delays[i]})),{rgba:r.frames[9].clear,delay:150},...r.frames.slice(27).map((f,i)=>({rgba:f.clear,delay:delays[i]}))];save(out+'SubwayDoor_Wide_OpenHoldClose_Preview.gif',encodePreview(preview,426,307));
let fixedFrameDifferences=0;for(const f of r.frames)for(let y=0;y<307;y++)for(let x=0;x<426;x++){if(x>=144&&x<280&&y>=78&&y<261)continue;const i=(y*426+x)*4;for(let c=0;c<4;c++)if(f.clear[i+c]!==raw[i+c])fixedFrameDifferences++;}if(fixedFrameDifferences)throw Error('Fixed frame moved');save(out+'FramePreservation.json',JSON.stringify({testedFrames:36,fixedFrameRGBADifferences:fixedFrameDifferences,openingStartSeconds:0,fullyOpenStartSeconds:.75,closingStartSeconds:2.25,clipEndSeconds:3,referenceUnmodified:true},null,2));


const mask=JSON.parse(readFileSync(new URL('./WindowMask.json',import.meta.url)));let tested=0,failures=0;for(let f=0;f<36;f++)for(const [x,y] of mask){const xx=x+(x<213?-mod.offset(f):mod.offset(f));if(xx<144||xx>=280)continue;const i=(y*426+xx)*4;tested++;if(r.frames[f].clear[i]||r.frames[f].clear[i+1]||r.frames[f].clear[i+2]||r.frames[f].clear[i+3]!==255)failures++;}if(failures)throw Error('Visible glass is not opaque black');save(out+'BlackWindowValidation.json',JSON.stringify({testedVisibleWindowPixels:tested,nonBlackOrNonOpaquePixels:failures,windowColor:'#000000',referenceFileUnmodified:true},null,2));

