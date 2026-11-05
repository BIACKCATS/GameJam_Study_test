import * as mod from './typing.mjs';
import {renderFrames,save} from '../../MetalSparks/Source/pixelfx/tools/lib/fxlib.mjs';
const fx={id:'ui/typing',meta:{bg:'dark',...mod.meta},mod},a=renderFrames(fx),b=renderFrames({...fx,mod:{...mod,draw(p,t,o){mod.draw(p,t+1.5,o);}}});
let periodic=true,stableAlpha=true,borderClear=true;const unique=new Set();
for(let i=0;i<a.n;i++){const f=a.frames[i].clear;periodic&&=Buffer.from(f).equals(Buffer.from(b.frames[i].clear));unique.add(Buffer.from(f).toString('base64'));for(let y=0;y<40;y++)for(let x=0;x<64;x++){const k=(y*64+x)*4+3;stableAlpha&&=f[k]===(a.frames[0].clear[((y-(mod.offset(i/24,x)-mod.offset(0,x)))*64+x)*4+3]??0);if(x===0||y===0||x===63||y===39)borderClear&&=f[k]===0;}}
const result={frames:a.n,width:a.w,height:a.h,fps:a.fps,durationSeconds:1.5,periodicPixelMatch:periodic,stableColumnSilhouetteAfterLocalMotionCompensation:stableAlpha,transparentBorder:borderClear,uniqueFrames:unique.size,dotPeakFrames:[10,22,34],sheetColumns:5,sheetRows:8};
if(!periodic||!stableAlpha||!borderClear||unique.size<10)throw Error(JSON.stringify(result));
save(new URL('../Exports/Validation.json',import.meta.url).pathname.replace(/^\/([A-Z]:)/,'$1'),JSON.stringify(result,null,2));console.log(result);


