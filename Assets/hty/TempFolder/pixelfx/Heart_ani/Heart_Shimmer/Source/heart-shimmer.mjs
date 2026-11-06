import {hex,mix} from '../../MetalSparks/Source/pixelfx/src/core/index.js';
export const meta={name:'Canonical red heart shimmer',desc:'A fixed red heart silhouette with a restrained moving surface reflection.',tags:['heart','red','shimmer'],size:[48,48],origin:[24,24],duration:1,loop:true,fps:10,options:{seed:1}};
// Purpose: define immutable canonical heart coverage; parameters: canvas pixel x/y; return: boolean.
export function inside(x,y){const X=(x-23.5)/13,Y=-(y-24)/13;if(y<16 && Math.abs(x-23.5)<(16-y)*.65)return false;return Math.pow(X*X+Y*Y-1,3)-X*X*Y*Y*Y<=0;}
// Purpose: assign fixed base shading; parameters: canvas pixel x/y; return: packed RGBA color.
export function baseColor(x,y){
 if(!inside(x-1,y)||!inside(x+1,y)||!inside(x,y-1)||!inside(x,y+1))return hex('#94182d');
 if(x>31||y>30)return hex('#c82038');
 if(x<30 && y<30)return mix(hex('#e92c40'),hex('#f43c4a'),Math.max(0,1-Math.hypot(x-20,y-22)/16));
 return hex('#e92c40');
}
// Purpose: render unchanged heart with periodic surface shimmer; parameters: PixelFX painter/time/options; return: void.
export function draw(p,t,o){
 const phase=((t%1)+1)%1,envelope=Math.pow(Math.sin(Math.PI*phase),2),stripe=5+phase*45;
 for(let y=0;y<48;y++)for(let x=0;x<48;x++)if(inside(x,y)){
  let c=baseColor(x,y);const edge=!inside(x-1,y)||!inside(x+1,y)||!inside(x,y-1)||!inside(x,y+1);
  if(!edge){const distance=Math.abs(x+y*.35-stripe);const gleam=Math.max(0,1-distance/3)*envelope*.72;c=mix(c,hex('#ffb6ae'),gleam);
   const star=(x===17&&Math.abs(y-17)<=2)||(y===17&&Math.abs(x-17)<=2);if(star)c=mix(c,hex('#fff2df'),Math.pow(Math.max(0,Math.cos((phase-.35)*Math.PI*2)),6)*.85);
  }p.rect(x-24,y-24,1,1,c);
 }
}



