import {hex} from '../../MetalSparks/Source/pixelfx/src/core/index.js';
export const meta={name:'Left Mouse Click',desc:'Fixed mouse with left button press and click accents',size:[128,128],origin:[0,0],duration:1.5,fps:16,loop:true,options:{seed:1}};
const ink=hex('#343b49'),body=hex('#dce3eb'),white=hex('#fffdf2'),yellow=hex('#ffe598');
// Return the 0-based timeline frame for time t seconds.
export function frame(t){return Math.floor(Math.round((((t%1.5)+1.5)%1.5)*16*1e7)/1e7)%24;}
// Draw fixed body and left button depression; p painter, f timeline frame; returns void.
export function mouse(p,f){
 p.poly([[52,29],[75,29],[83,34],[88,43],[88,88],[83,99],[75,104],[52,104],[44,99],[39,88],[39,43],[44,34]],ink);
 p.poly([[53,32],[74,32],[81,36],[85,44],[85,87],[81,96],[74,101],[53,101],[46,96],[42,87],[42,44],[46,36]],body);
 p.line(44,74,44,87,hex('#f6f8fb'));p.line(83,74,83,87,hex('#aab6c6'));
 p.rect(62,32,3,39,ink);p.rect(42,69,43,3,ink);
 p.poly([[67,34],[74,34],[79,38],[83,45],[83,66],[67,66]],hex('#f5f7fa'));
 const down=f>=6&&f<=9?2:f===10?1:0,active=f>=4&&f<=10;
 p.poly([[53,34+down],[60,34+down],[60,66+down],[44,66+down],[44,45+down],[48,38+down]],active?(down?hex('#f5be56'):yellow):hex('#f5f7fa'));
 if(down)p.line(49,35,60,35,ink);
 p.rect(61,42,5,13,ink);p.rect(62,44,3,8,hex('#8491a4'));
}
// Draw expanding pixel ring and four glints; p painter, f timeline frame; returns void.
export function accents(p,f){
 if(f<8||f>14)return;const age=f-8,r=9+age*2;
 for(let i=0;i<48;i++){if(age>4&&i%2!==0)continue;const a=i*Math.PI/24;p.blend(Math.round(51+Math.cos(a)*r),Math.round(49+Math.sin(a)*r),yellow,(7-age)/7);}
 const points=[[31,30],[51,19],[28,54],[34,73]];
 if(age<5)for(let i=0;i<4;i++){const [x,y]=points[i],s=Math.max(1,3-Math.floor((age+i%2)/2));p.line(x-s,y,x+s,y,i%2?yellow:white);p.line(x,y-s,x,y+s,i%2?yellow:white);}
}
// Draw full animation; p painter, t seconds, o options; returns void.
export function draw(p,t,o){const f=frame(t);mouse(p,f);accents(p,f);}


