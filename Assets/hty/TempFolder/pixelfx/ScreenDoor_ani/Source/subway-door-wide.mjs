import {readFileSync} from 'node:fs';import {hex} from '../../MetalSparks/Source/pixelfx/src/core/index.js';
// Convert immutable source RGBA to PixelFX packed colors; input byte buffer; return color array.
function palette(raw){const colors=[];for(let i=0;i<raw.length;i+=4)colors.push(hex('#'+[raw[i],raw[i+1],raw[i+2]].map(v=>v.toString(16).padStart(2,'0')).join('')));return colors;}
const original=palette(readFileSync(new URL('./Reference.rgba',import.meta.url))),behind=palette(readFileSync(new URL('./Passage.rgba',import.meta.url)));
export const meta={name:'Wide subway interior door',desc:'Only the central reference doors move; the entire carriage environment stays fixed.',size:[426,307],origin:[0,0],duration:35/12,fps:12,loop:false,options:{seed:1}};
export const steps=[0,2,7,15,27,40,52,61,67,69];
// Map animation frame to symmetric slide distance; input frame; return integer pixels.
export function offset(f){return f<9?steps[f]:f<27?69:steps[35-f];}
// Draw the original wide interior, sliding just the two central leaves over a pixel-sharp passage; inputs painter,time,options; returns void.
export function draw(p,t,o){const f=Math.min(35,Math.floor(t*12+1e-7)),d=offset(f);for(let y=0;y<183;y++)for(let x=0;x<136;x++)p.rect(144+x,78+y,1,1,behind[y*136+x]);p.rect(146,257,132,4,hex('#697a8f'));p.rect(146,257,132,1,hex('#9aa6b4'));p.rect(144,78,136,3,hex('#333e51'));p.rect(144,81,2,180,hex('#4b586c'));p.rect(278,81,2,180,hex('#4b586c'));for(let y=0;y<307;y++)for(let x=0;x<426;x++){const panel=x>=144&&x<280&&y>=78&&y<261;let xx=x;if(panel){xx=x+(x<213?-d:d);if(xx<144||xx>=280)continue;}p.rect(xx,y,1,1,original[y*426+x]);}}


