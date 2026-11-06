import * as left from '../../MouseClick/Source/mouse.mjs';
export const meta={...left.meta,name:'Right Mouse Click',desc:'Right mouse button press and mirrored click accents'};
export const frame=left.frame;
// Mirror painter coordinates across canvas center; p painter; returns drawing adapter.
function mirror(p){return {poly:(pts,c)=>p.poly(pts.map(([x,y])=>[127-x,y]),c),line:(x,y,xx,yy,c,a)=>p.line(127-x,y,127-xx,yy,c,a),rect:(x,y,w,h,c)=>p.rect(128-x-w,y,w,h,c),blend:(x,y,c,a)=>p.blend(127-x,y,c,a)};}
// Draw mirrored mouse with right button response; p painter, f frame; returns void.
export function mouse(p,f){left.mouse(mirror(p),f);}
// Draw right-side click effects; p painter, f frame; returns void.
export function accents(p,f){left.accents(mirror(p),f);}
// Draw full right-click animation; p painter, t seconds, o options; returns void.
export function draw(p,t,o){const f=frame(t);mouse(p,f);accents(p,f);}
