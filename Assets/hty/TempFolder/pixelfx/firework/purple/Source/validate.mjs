import {effect} from './fireworks.mjs';
import {renderFrames,save} from '../../MetalSparks/Source/pixelfx/tools/lib/fxlib.mjs';
const results=[];for(let kind=0;kind<2;kind++){
 const mod=effect(kind),r=renderFrames({id:'test',meta:{bg:'dark',...mod.meta},mod});let borderClear=true;const counts=[];
 for(const frame of r.frames){let count=0;for(let y=0;y<160;y++)for(let x=0;x<160;x++){const a=frame.clear[(y*160+x)*4+3];if(a)count++;if(x===0||x===159||y===0||y===159)borderClear&&=a===0;}counts.push(count);}
 const result={kind:kind?'RedGreen':'PurpleBlue',frames:r.n,size:[r.w,r.h],fps:r.fps,playbackSeconds:r.n/r.fps,center:[80,80],transparentBorder:borderClear,finalFrameEmpty:counts.at(-1)===0,visiblePixelCounts:counts};if(!borderClear||!result.finalFrameEmpty||r.n!==16)throw Error(JSON.stringify(result));results.push(result);
}save(new URL('../Exports/Validation.json',import.meta.url).pathname.replace(/^\/([A-Z]:)/,'$1'),JSON.stringify(results,null,2));console.log(results);
