import {hex} from '../../MetalSparks/Source/pixelfx/src/core/index.js';
export const meta={name:'Three-part waving typing bubble',desc:'Connected bubble sections follow their corresponding dots.',size:[64,40],origin:[0,0],duration:1.5,fps:24,loop:true,options:{seed:1}};
// Return the normalized bounce of dot i at time t seconds.
export function bounce(t,i){const phase=((t/1.5-i/3)%1+1)%1;return phase<.5?Math.sin(phase*Math.PI*2)**2:0;}
// Return the column displacement, blending adjacent dot regions without gaps.
export function offset(t,x){const a=Math.max(0,Math.min(2,(x-21)/11)),i=Math.min(1,Math.floor(a)),u=a-i,s=u*u*(3-2*u);return -Math.round(4*((1-s)*bounce(t,i)+s*bounce(t,i+1)));}
// Draw a connected silhouette and synchronized dots; inputs painter, seconds, options; returns void.
export function draw(p,t,o){
 const outline=hex('#39475b'),fill=hex('#e4ebf2');
 for(let x=4;x<=59;x++){
  const dy=offset(t,x),top=x<7||x>56?10:7;
  let bottom=x<7||x>56?27:30;
  if(x>=13&&x<=22)bottom=Math.max(bottom,35-Math.floor((x-13)*5/9));
  p.rect(x,top+dy,1,bottom-top+1,outline);
  if(x>4&&x<59){const innerTop=x<8||x>55?11:8;let innerBottom=x<8||x>55?26:29;
   if(x>=14&&x<=21)innerBottom=Math.max(innerBottom,33-Math.floor((x-14)*4/7));
   p.rect(x,innerTop+dy,1,innerBottom-innerTop+1,fill);
  }
 }
 for(let i=0;i<3;i++){
  const beat=bounce(t,i),x=21+i*11;
  p.disc(x,20+offset(t,x),2.6,hex(beat>.65?'#347fac':beat>.15?'#647e97':'#8292a3'),1);
 }
}
