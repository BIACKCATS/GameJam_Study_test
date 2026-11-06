import {hex} from '../../MetalSparks/Source/pixelfx/src/core/index.js';
const TAU=Math.PI*2,white=hex('#fff9e9');
const purple=['#673cba','#ab62ef','#658bff','#a4e9ff'].map(hex),red=['#a93254','#ff5265','#278451','#8bef7c'].map(hex);
// Draw a small pixel glint; painter p, position x/y, color c, radius s; returns void.
function glint(p,x,y,c,s=1){p.line(x-s,y,x+s,y,c);p.line(x,y-s,x,y+s,c);}
// Build either round chrysanthemum or branched comet effect; kind 0 or 1; returns PixelFX module.
export function effect(kind){return {meta:{name:kind?'Red Green Forked Comets':'Purple Blue Chrysanthemum',desc:'Celebration burst without rocket or smoke',size:[160,160],origin:[0,0],duration:15/16,fps:16,loop:false,options:{seed:1}},draw(p,t,o){
 const f=Math.min(15,Math.round(t*16));if(f===15)return;const C=kind?red:purple;
 if(f<3){glint(p,80,80,white,f===1?7:2);glint(p,80,80,C[3],f===1?10:3);p.disc(80,80,f===1?3:1,white,1);}
 if(f===0)return;
 const u=(f-1)/13,e=1-Math.pow(1-u,3),fade=Math.max(0,(f-9)/6);
 if(!kind){
  for(let i=0;i<80;i++){
   const a=i*TAU/(i<64?32:16)+(i>=32?.085:0),radius=(i<32?57:i<64?37:20)*(1+(i%3-1)*.045),r=3+radius*e,drop=9*u*u;
   if(f>9&&((i*7)%13)/13<fade)continue;
   const x=80+Math.cos(a)*r,y=80+Math.sin(a)*r+drop,c=C[i%2?1:2];
   if(f<10){const trail=(1-u)*(i<32?10:6);p.line(x-Math.cos(a)*trail,y-Math.sin(a)*trail,x,y,c);}
   const bright=(f+i)%4===0&&f<13;p.disc(x,y,bright?1.5:1,C[bright?3:i%2?1:2],1);
   if(bright&&f>4)glint(p,x,y,C[3],2);
  }
 }else{
  for(let i=0;i<10;i++){
   const a=i*TAU/10-.22,r=(i%2?49:64)*e,x=80+Math.cos(a)*r,y=80+Math.sin(a)*r+5*u*u,c=C[i%2?3:1];
   if(f<10){const tail=24*(1-u)+5;p.line(x-Math.cos(a)*tail,y-Math.sin(a)*tail,x,y,c);p.line(x-Math.cos(a)*tail*.8+1,y-Math.sin(a)*tail*.8,x+1,y,C[i%2?2:0]);}
   if(f<6)glint(p,x,y,white,1);
   if(f>=4)for(let j=-1;j<=1;j+=2){const v=Math.min(1,(f-3)/9),b=a+j*.85,dist=16*v,xx=x+Math.cos(b)*dist,yy=y+Math.sin(b)*dist;
    if(f>10&&(i+j+12)%4<(f-10))continue;
    p.line(xx-Math.cos(b)*(13*(1-v)),yy-Math.sin(b)*(13*(1-v)),xx,yy,c);
    if((f+i+j)%3===0)glint(p,xx,yy,C[i%2?3:1],1);
   }
  }
 }
}};}



