import {hex} from '../../MetalSparks/Source/pixelfx/src/core/index.js';
export const meta={name:'Cold stone ground impact',desc:'A punched-in crater and branching fractures remain after the debris settles.',size:[160,128],origin:[80,80],duration:11/20,fps:20,loop:false,options:{layer:'combined',seed:1}};
const dark=hex('#233345'),shade=hex('#465c72'),stone=hex('#8298ad'),light=hex('#bfcedb');
const cracks=[[[15,-4],[26,-9],[32,-8],[42,-17],[50,-16],[64,-23]],[[-16,-4],[-25,-11],[-33,-9],[-40,-17],[-55,-18],[-63,-26]],[[4,-9],[10,-18],[7,-23],[17,-31],[15,-37]],[[-7,-8],[-14,-18],[-22,-20],[-25,-30]],[[18,5],[29,10],[33,8],[43,16],[55,18],[64,23]],[[-16,6],[-25,13],[-35,12],[-41,20],[-58,24]],[[5,9],[14,17],[12,23],[23,28]],[[-6,10],[-10,20],[-20,25],[-18,32]]];
// Render a growing polyline with a light chipped lip and a dark core; inputs painter,path,progress,width; return void.
function fracture(p,pts,k,width){const n=(pts.length-1)*k;for(let i=0;i<Math.ceil(n);i++){if(i>=pts.length-1)break;const a=pts[i],b=pts[i+1],u=Math.min(1,n-i),x=Math.round(a[0]+(b[0]-a[0])*u),y=Math.round(a[1]+(b[1]-a[1])*u);p.line(a[0],a[1]-1,x,y-1,stone);p.line(a[0],a[1],x,y,dark);if(width>1)p.line(a[0],a[1]+1,x,y+1,dark);}}
// Draw jagged raised lips, recessed interior wall and floor chips; inputs painter,frame; return void.
function crater(p,f){if(f===0)return;const k=[0,.42,.78,.96,1,1,1,1,1,1,1,1][f],grow=Math.min(1,f/4);for(let i=0;i<cracks.length;i++)fracture(p,cracks[i],grow,i%3===0?2:1);if(f>=3){fracture(p,[[32,-8],[39,-5],[47,-7]],grow,1);fracture(p,[[-35,12],[-40,7],[-50,6]],grow,1);fracture(p,[[12,23],[5,27],[3,32]],grow,1);}
const poly=(points,c)=>p.poly(points.map(([x,y])=>[Math.round(x*k),Math.round(y*k)]),c);
poly([[-26,-2],[-20,-11],[-10,-13],[-3,-10],[7,-14],[18,-10],[26,-5],[24,4],[19,12],[8,15],[-4,13],[-15,14],[-25,7]],stone);
poly([[-21,-2],[-16,-8],[-6,-9],[3,-7],[10,-10],[21,-3],[20,6],[12,11],[-2,10],[-16,9]],dark);
poly([[-19,0],[-12,2],[-5,6],[7,5],[17,0],[16,7],[7,10],[-5,9],[-16,7]],shade);
poly([[-25,0],[-20,-10],[-10,-12],[-13,-8],[-20,-5]],light);
poly([[6,12],[18,8],[24,1],[22,6],[15,12],[5,14],[-8,12],[-15,13],[-23,8],[-24,5],[-14,10]],light);
poly([[-20,-2],[-16,-7],[-6,-8],[3,-6],[10,-9],[20,-3],[15,2],[8,-2],[-3,1],[-12,-3],[-17,2]],shade);
poly([[-8,6],[-3,4],[2,7],[-2,9]],stone);poly([[8,4],[13,2],[15,6],[11,8]],stone);
}
// Draw the brief contact flash and physically arcing blue-gray stone chips; inputs painter,frame; return void.
function debris(p,f){if(f===0||f>=8)return;if(f<=2){const r=f===1?30:38;for(let i=0;i<7;i++){const a=i*Math.PI*2/7+.13,l=r*(i%2?.8:1);p.tri(Math.cos(a+.3)*5,Math.sin(a+.3)*3,Math.cos(a)*l,Math.sin(a)*l*.5,Math.cos(a-.3)*5,Math.sin(a-.3)*3,f===1?light:stone);}if(f===1)p.poly([[-9,-4],[-4,-10],[0,-5],[6,-8],[10,-1],[5,4],[-4,5]],hex('#edf3f7'));}
for(let i=0;i<9;i++){const age=f-1,side=i%2?1:-1,v=2.4+(i%4)*1.1,x=side*(9+age*v),y=-4-age*(7+(i%3)*2)+age*age*.9+(i%3)*4,size=Math.max(1,4-Math.floor(age/2));p.poly([[x-size,y],[x-1,y-size],[x+size,y-1],[x+size-1,y+size],[x-size+1,y+size]],i%2?shade:stone);if(size>1)p.line(x-size+1,y-1,x,y-size+1,light);}}
// Compose persistent damage and temporary debris; inputs painter,time,options; return void.
export function draw(p,t,o){const f=Math.min(11,Math.floor(t*20+1e-7));if(o.layer!=='debris')crater(p,f);if(o.layer!=='crack')debris(p,f);}


