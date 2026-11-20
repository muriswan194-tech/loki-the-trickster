import * as THREE from 'three';
import './style.css';
import './overrides.css';

const $ = (id) => document.getElementById(id);
const ui = {
  menu: $('menu'), hud: $('hud'), profile: $('profile-modal'), options: $('options-modal'),
  end: $('end-card'), intro: $('intro-card'), toast: $('toast'), prompt: $('prompt'),
  map: $('map-panel'), mini: $('minimap'), crosshair: $('crosshair'),
  awareness: $('awareness-fill'), guardState: $('guard-state'), objective: $('objective-title'),
  subtitle: $('objective-subtitle'), ending: $('ending-line'), dialogue: $('dialogue-modal'),
  dialogueName: $('dialogue-name'), dialogueLine: $('dialogue-line'), dialogueOptions: $('dialogue-options')
};
const guideUi = { text: $('guide-text'), voice: $('voice-toggle'), sfx: $('sfx-toggle'), count: $('watch-count') };
const palette = {
  ink: 0x302b36, stone: 0xf0dfbd, stoneShade: 0xd6bd91, shadowStone: 0x917d68,
  gold: 0xe9b955, paleGold: 0xffdb7d, green: 0x174e37, green2: 0x377b4c,
  emerald: 0x63f27f, bronze: 0x9e7345, red: 0xa84543, teal: 0x61c9c4,
  sky: 0x9bc9c5, wood: 0x70442e, lilac: 0x8e75a6
};

const renderer = new THREE.WebGLRenderer({ canvas: $('world'), antialias: true, powerPreference: 'high-performance' });
renderer.setPixelRatio(Math.min(devicePixelRatio, 1.7));
renderer.setSize(innerWidth, innerHeight);
renderer.shadowMap.enabled = true;
renderer.shadowMap.type = THREE.PCFSoftShadowMap;
renderer.outputColorSpace = THREE.SRGBColorSpace;
renderer.toneMapping = THREE.ACESFilmicToneMapping;
renderer.toneMappingExposure = 1.12;

const scene = new THREE.Scene();
scene.background = new THREE.Color(0xb8d5c8);
scene.fog = new THREE.Fog(0xb8d5c8, 36, 82);
const camera = new THREE.PerspectiveCamera(48, innerWidth / innerHeight, .1, 130);
let district;
const clock = new THREE.Clock();
const raycaster = new THREE.Raycaster();
const pointer = new THREE.Vector2(0, 0);
const groundPlane = new THREE.Plane(new THREE.Vector3(0, 1, 0), 0);
const cameraBlockers = [];
const obstacles = [];
const movingObjects = [];
const npcs = [];
const watchPostObjects = [];
const watchers = [];
const keys = new Set();
let playing = false, paused = false, mapOpen = false, crisis = false, escaped = false, dialogueOpen = false;
let bridgeGateOpen=false, bridgeStarted=false, teleporting=false, montageQueued=false;
let player, guard, decoy, portal, guest, ground, worldRoot, dialogueNpc, bridgeGate, bifrostPortal, bifrostArrival;
let teleportStarted=0;
let yaw = 0, pitch = .3, cameraDistance = 5.2, cameraActualDistance = 5.2;
let moveVelocity = new THREE.Vector3(), cameraVelocity = new THREE.Vector3();
let guardState = 'PATROL', suspicion = 0, patrolIndex = 0;
let investigatePoint = new THREE.Vector3(), sound = null;
let disguisedUntil = 0, decoyUntil = 0, toastUntil = 0, crisisAt = 0, guardHoldUntil = 0;
let guardReactUntil = 0, lastSeenUntil = 0;
const lastSeenPlayer = new THREE.Vector3();
let guardTrust = .12;
let dialogueReaction = '';
let lastPlayerSpeed = 0, walkTime = 0, caughtUntil = 0;
let isDragging = false, activePointer = -1;
let fadedFoliage = null;
let mouseGround = new THREE.Vector3(0, 0, 1);
let animationRequest = 0;
let audioContext = null, voiceEnabled = true, sfxEnabled = true, lastGuideKey = '';

const toonRamp = (() => {
  const colors = new Uint8Array([82,82,92, 145,145,154, 205,205,212, 255,255,255]);
  const tex = new THREE.DataTexture(colors, 4, 1, THREE.RGBFormat);
  tex.minFilter = THREE.NearestFilter; tex.magFilter = THREE.NearestFilter;
  tex.generateMipmaps = false; tex.needsUpdate = true;
  return tex;
})();
function mat(color, opts = {}) {
  const material = new THREE.MeshToonMaterial({ color, gradientMap: toonRamp, ...opts });
  return material;
}
const M = {
  stone: mat(palette.stone), stone2: mat(palette.stoneShade), darkStone: mat(palette.shadowStone),
  gold: mat(palette.gold), brightGold: mat(palette.paleGold), bronze: mat(palette.bronze),
  green: mat(palette.green), green2: mat(palette.green2), dark: mat(0x23362f),
  red: mat(palette.red), teal: mat(palette.teal), wood: mat(palette.wood),
  cream: mat(0xfff0d1), ink: new THREE.MeshBasicMaterial({ color: palette.ink }),
  emerald: new THREE.MeshToonMaterial({ color: palette.emerald, gradientMap: toonRamp, emissive: 0x1db83f, emissiveIntensity: .65 }),
  glass: new THREE.MeshToonMaterial({ color: 0x7fe5d7, gradientMap: toonRamp, transparent: true, opacity: .6, side: THREE.DoubleSide }),
  redBlack: mat(0x9e3237), black: mat(0x28262c)
};

const hemi = new THREE.HemisphereLight(0xd7f0ee, 0x8b6845, 2.15); scene.add(hemi);
const sun = new THREE.DirectionalLight(0xffd58c, 3.2);
sun.position.set(-18, 27, -13); sun.castShadow = true;
sun.shadow.mapSize.set(2048, 2048);
sun.shadow.camera.left = -29; sun.shadow.camera.right = 29; sun.shadow.camera.top = 28; sun.shadow.camera.bottom = -28;
sun.shadow.bias = -.0002; sun.shadow.normalBias = .025; scene.add(sun);
const fill = new THREE.DirectionalLight(0xa8d7df, 1.05); fill.position.set(16, 13, 16); scene.add(fill);

function mesh(geometry, material, parent = worldRoot, opts = {}) {
  const object = new THREE.Mesh(geometry, material);
  object.castShadow = opts.castShadow !== false;
  object.receiveShadow = opts.receiveShadow !== false;
  if (opts.name) object.name = opts.name;
  if (opts.position) object.position.copy(opts.position);
  if (parent) parent.add(object);
  return object;
}
function group(parent = worldRoot, position = null) {
  const o = new THREE.Group(); if (position) o.position.copy(position); if (parent) parent.add(o); return o;
}
function outlined(object, scale = 1.045, color = palette.ink) {
  const shell = new THREE.Mesh(object.geometry, new THREE.MeshBasicMaterial({ color, side: THREE.BackSide, depthWrite: true }));
  shell.position.set(0, 0, 0); shell.rotation.set(0, 0, 0); shell.scale.setScalar(scale);
  shell.renderOrder = -1; object.add(shell); return object;
}
function box(parent, position, size, material, bevel = false, outline = false) {
  let geometry = new THREE.BoxGeometry(...size);
  const o = mesh(geometry, material, parent, { position: new THREE.Vector3(...position) });
  if (outline) outlined(o, 1.035);
  return o;
}
function sphere(parent, position, size, material, detail = 18, outline = false) {
  const o = mesh(new THREE.SphereGeometry(1, detail, Math.max(10, Math.floor(detail * .65))), material, parent, { position: new THREE.Vector3(...position) });
  o.scale.set(...size); if (outline) outlined(o, 1.04); return o;
}
function cylinder(parent, position, radiusTop, radiusBottom, height, material, segments = 16) {
  return mesh(new THREE.CylinderGeometry(radiusTop, radiusBottom, height, segments, 1), material, parent, { position: new THREE.Vector3(...position) });
}
function torus(parent, position, radius, tube, material, arc = Math.PI * 2, yawRot = 0, outline = false) {
  const o = mesh(new THREE.TorusGeometry(radius, tube, 10, 36, arc), material, parent, { position: new THREE.Vector3(...position) });
  o.rotation.y = yawRot; if (outline) outlined(o, 1.07); return o;
}
function tube(parent, points, radius, material, segments = 24) {
  const curve = new THREE.CatmullRomCurve3(points.map(p => new THREE.Vector3(...p)));
  return mesh(new THREE.TubeGeometry(curve, segments, radius, 8, false), material, parent);
}
function textSprite(text, color = '#fff0d1', bg = '#283b37') {
  const canvas = document.createElement('canvas'); canvas.width = 512; canvas.height = 128;
  const ctx = canvas.getContext('2d');
  ctx.fillStyle = bg; ctx.beginPath(); ctx.roundRect(8, 10, 496, 108, 18); ctx.fill();
  ctx.strokeStyle = '#dbb666'; ctx.lineWidth = 5; ctx.stroke();
  ctx.fillStyle = color; ctx.font = '800 38px system-ui'; ctx.textAlign = 'center'; ctx.textBaseline = 'middle'; ctx.fillText(text, 256, 65, 460);
  const texture = new THREE.CanvasTexture(canvas); texture.colorSpace = THREE.SRGBColorSpace;
  const sprite = new THREE.Sprite(new THREE.SpriteMaterial({ map: texture, transparent: true, depthTest: false }));
  sprite.scale.set(3.2, .8, 1); sprite.renderOrder = 5; return sprite;
}
function addBlocker(o) { cameraBlockers.push(o); return o; }

function createFloorTexture() {
  const c = document.createElement('canvas'); c.width = 512; c.height = 512;
  const x = c.getContext('2d'); x.fillStyle = '#cdb88f'; x.fillRect(0, 0, 512, 512);
  const tile = 64;
  for (let row = 0; row < 8; row++) for (let col = 0; col < 8; col++) {
    const shade = ((row * 11 + col * 7) % 5) - 2;
    x.fillStyle = `rgb(${211 + shade * 2},${190 + shade * 2},${153 + shade * 2})`;
    const offset = row % 2 ? 5 : 0;
    x.fillRect(col * tile + offset + 2, row * tile + 2, tile - 4, tile - 4);
    x.fillStyle = 'rgba(255,248,225,.23)'; x.fillRect(col * tile + offset + 3, row * tile + 3, tile - 6, 3);
    x.fillStyle = 'rgba(101,75,47,.15)'; x.fillRect(col * tile + offset + 2, row * tile + tile - 5, tile - 4, 3);
  }
  const texture = new THREE.CanvasTexture(c); texture.colorSpace = THREE.SRGBColorSpace;
  texture.wrapS = texture.wrapT = THREE.RepeatWrapping; texture.repeat.set(2.5, 2.5); texture.anisotropy = renderer.capabilities.getMaxAnisotropy();
  return texture;
}

function makePath(a, b, width, material = M.stone2, y = .018) {
  const dx = b.x - a.x, dz = b.z - a.z, len = Math.hypot(dx, dz);
  if (len < .001) return;
  const nx = -dz / len * width * .5, nz = dx / len * width * .5;
  const geo = new THREE.BufferGeometry();
  geo.setAttribute('position', new THREE.Float32BufferAttribute([
    a.x + nx, y, a.z + nz, a.x - nx, y, a.z - nz, b.x - nx, y, b.z - nz,
    a.x + nx, y, a.z + nz, b.x - nx, y, b.z - nz, b.x + nx, y, b.z + nz
  ], 3)); geo.computeVertexNormals();
  mesh(geo, material, worldRoot, { receiveShadow: true, castShadow: false });
  const trim = Math.max(.035, width * .018);
  for (const side of [-1, 1]) {
    const offset = side * width * .43;
    const x1 = a.x + (-dz / len) * offset, z1 = a.z + (dx / len) * offset;
    const x2 = b.x + (-dz / len) * offset, z2 = b.z + (dx / len) * offset;
    const d = Math.hypot(x2 - x1, z2 - z1);
    const line = mesh(new THREE.BoxGeometry(trim, .025, d), M.gold, worldRoot, { position: new THREE.Vector3((x1 + x2) / 2, y + .02, (z1 + z2) / 2), castShadow: false });
    line.rotation.y = Math.atan2(x2 - x1, z2 - z1);
  }
}

function makeColumn(parent, x, z, scale = 1, material = M.stone) {
  const g = group(parent, new THREE.Vector3(x, 0, z)); g.scale.setScalar(scale);
  cylinder(g, [0, .18, 0], .52, .67, .36, M.stone2, 16);
  cylinder(g, [0, .42, 0], .38, .42, .14, M.gold, 16);
  cylinder(g, [0, 1.65, 0], .23, .29, 2.35, material, 16);
  for (let i = 0; i < 8; i++) {
    const a = i * Math.PI / 4;
    const rib = cylinder(g, [Math.cos(a) * .245, 1.62, Math.sin(a) * .245], .035, .035, 2.08, M.stone2, 8);
  }
  cylinder(g, [0, 2.85, 0], .4, .3, .18, M.gold, 16);
  cylinder(g, [0, 3.02, 0], .56, .4, .22, M.stone, 16);
  const capital = sphere(g, [0, 3.15, 0], [.5, .2, .5], M.gold, 16);
  addBlocker(g);
  return g;
}
function makeArch(parent, position, width, height, material = M.stone, facing = 0, ornament = true) {
  const g = group(parent, new THREE.Vector3(...position)); g.rotation.y = facing;
  const r = width * .5, tubeSize = width * .065;
  // Paired curved mouldings make each arch read as carved stone rather than a flat ring.
  torus(g, [0, height - r, 0], r, tubeSize, material, Math.PI, 0, true);
  torus(g, [0, height - r, .11], r - tubeSize * 1.55, tubeSize * .34, M.gold, Math.PI, 0);
  makeColumn(g, -r, 0, .75, material); makeColumn(g, r, 0, .75, material);
  if (ornament) {
    sphere(g, [0, height + .05, .01], [.22, .28, .16], M.brightGold, 16);
    const gem = sphere(g, [0, height + .06, .15], [.075, .11, .05], M.teal, 12);
  }
  addBlocker(g);
  return g;
}
function makeDome(parent, position, radius, height, material = M.stone) {
  const g = group(parent, new THREE.Vector3(...position));
  cylinder(g, [0, height * .1, 0], radius * .92, radius, height * .2, M.gold, 24);
  const dome = mesh(new THREE.SphereGeometry(radius, 24, 14, 0, Math.PI * 2, 0, Math.PI * .53), material, g, { position: new THREE.Vector3(0, height * .2, 0) });
  dome.scale.y = height / radius;
  torus(g, [0, height * .55, 0], radius * .73, .07, M.gold);
  const fin = sphere(g, [0, height * .2 + height * .52, 0], [.14, .27, .14], M.gold, 14);
  return g;
}
function makeTower(parent, position, radius, height, color = M.stone) {
  const g = group(parent, new THREE.Vector3(...position));
  cylinder(g, [0, height * .48, 0], radius * .8, radius, height * .96, color, 20);
  for (let y = .35; y < height; y += Math.max(1, height / 4)) {
    torus(g, [0, y, 0], radius * .84, .075, M.gold);
    const w = .11, h = .36;
    const win = box(g, [0, y + .48, radius * .83], [w, h, .035], M.ink);
    const glow = box(g, [0, y + .49, radius * .85], [w * .58, h * .68, .025], M.teal);
  }
  makeDome(g, [0, height * .92, 0], radius * 1.04, height * .36, M.gold);
  return g;
}

function buildWorld() {
  worldRoot = group(scene);
  const floorMat = new THREE.MeshToonMaterial({ map: createFloorTexture(), gradientMap: toonRamp });
  ground = mesh(new THREE.PlaneGeometry(34, 24.5), floorMat, worldRoot, { position: new THREE.Vector3(0, 0, -.7), receiveShadow: true, castShadow: false });
  ground.rotation.x = -Math.PI / 2;
  // District rim: patterned low balustrade and warm cut-stone boundary.
  box(worldRoot, [0, -.12, -12.45], [34, .3, .38], M.shadowStone);
  for (const x of [-15.8, -13, -10, -7, -4, -1, 2, 5, 8, 11, 14, 15.8]) {
    sphere(worldRoot, [x, .12, 11], [.2, .28, .2], M.gold, 10);
  }

  // Follow the editable route data for pavements and their gold inset edges.
  for (const route of district.routes.filter(r=>r.id!=='bridge-deck')) for (let i = 1; i < route.points.length; i++) {
    const a = route.points[i - 1], b = route.points[i]; makePath(a, b, route.width, M.stone2, .02);
  }

  buildPalace(); buildArcades(); buildHomes(); buildFountain(); buildGardens(); buildBifrost(); buildPalaceCourtyard(); buildBridge(); buildBifrostPavilion(); buildMarket();
  createPlayer(); createGuard(); createProps(); createNpcs();
  // Small, colorful floating skyline hints at a city beyond the playful district.
  for (let i = 0; i < 15; i++) {
    const x = -28 + i * 4.2, z = 98 + (i % 3) * 2.2;
    const h = 8 + (i * 7 % 13);
    const tint = [M.stone, M.stone2, M.gold][i % 3];
    makeTower(worldRoot, [x, -1.1, z], 1.25 + (i % 3) * .25, h, tint);
  }
  buildCloudBanks();
}

function buildPalace() {
  const p = group(worldRoot, new THREE.Vector3(0, 0, 0)); p.name='PalaceFacade';
  // Closed monumental north façade: layered cream stone, gold tracery and a high spire.
  box(p,[-12.5,4.2,50.25],[13,8.4,2],M.stone2);
  box(p,[12.5,4.2,50.25],[13,8.4,2],M.stone2);
  for(const x of [-14,14]) { makeTower(p,[x,3.4,49.8],1.8,10,M.stone); makeDome(p,[x,9.4,49.8],2,3.2,M.gold); }
  box(p,[-13.4,2.25,48.15],[13.2,4.5,.85],M.stone);
  box(p,[13.4,2.25,48.15],[13.2,4.5,.85],M.stone);
  makeArch(p,[0,0,47.62],11.8,8.8,M.gold,0,true);
  const leftDoor=group(p,new THREE.Vector3(-3.4,0,47.72)); leftDoor.name='Bridge Gate · west leaf';
  box(leftDoor,[1.7,3.15,0],[3.35,6.05,.24],M.wood);
  for(const x of [.25,1.1,2.25,3.1]) box(leftDoor,[x,3.15,.14],[.08,5.95,.07],M.gold);
  const rightDoor=group(p,new THREE.Vector3(3.4,0,47.72)); rightDoor.name='Bridge Gate · east leaf';
  box(rightDoor,[-1.7,3.15,0],[3.35,6.05,.24],M.wood);
  for(const x of [-.25,-1.1,-2.25,-3.1]) box(rightDoor,[x,3.15,.14],[.08,5.95,.07],M.gold);
  box(p,[0,6.46,47.62],[9.9,.17,.16],M.gold);
  for(const x of [-10.1,10.1]) {
    const pole=cylinder(p,[x,5.25,47.7],.07,.07,5.9,M.gold,10);
    const cloth=new THREE.Shape(); cloth.moveTo(0,0); cloth.lineTo(1.55,-.12); cloth.lineTo(1.1,-2.6); cloth.lineTo(.74,-2.18); cloth.lineTo(.36,-2.68); cloth.closePath();
    const banner=mesh(new THREE.ShapeGeometry(cloth),M.red,p,{position:new THREE.Vector3(x+.08,7.8,47.56)}); outlined(banner,1.025);
    box(p,[x,8.3,47.65],[2,.12,.12],M.gold);
  }
  makeTower(p,[0,8.3,53.5],1.35,17,M.gold);
  for(const x of [-5.4,5.4]) makeColumn(p,x,48.1,1.15,M.gold);
  bridgeGate=group(worldRoot,new THREE.Vector3(0,0,48.15)); bridgeGate.name='Bridge Gate';
  bridgeGate.userData.kind='bridgeGate'; bridgeGate.userData.range=3.2; bridgeGate.userData.doors=[leftDoor,rightDoor];
  bridgeGate.userData.open=false; movingObjects.push(bridgeGate);
  obstacles.push({kind:'rect',minX:-3.4,maxX:3.4,minZ:47.35,maxZ:48.2,bridgeGate:true});
  addBlocker(p);
}

function buildPalaceCourtyard() {
  const court=group(worldRoot); court.name='PalaceCourtyard · 40m × 40m';
  const floorMat=new THREE.MeshToonMaterial({map:createFloorTexture(),gradientMap:toonRamp});
  const floor=mesh(new THREE.PlaneGeometry(40,40),floorMat,court,{position:new THREE.Vector3(0,.025,28.15),receiveShadow:true,castShadow:false});
  floor.rotation.x=-Math.PI/2;
  // The old Bifrost arch stays as the south entrance. Wall segments meet its shoulders.
  for(const side of [-1,1]) {
    if(side<0) {
      const segment=box(court,[-12.2,2.25,8.15],[15.6,4.5,.9],M.stone2); addBlocker(segment);
      const wallA=box(court,[-20,2.3,26.275],[.9,4.6,36.25],M.stone2); addBlocker(wallA);
      const wallB=box(court,[-20,2.3,48.025],[.9,4.6,.25],M.stone2); addBlocker(wallB);
      obstacles.push({kind:'rect',minX:-20.45,maxX:-19.55,minZ:8.15,maxZ:44.4},{kind:'rect',minX:-20.45,maxX:-19.55,minZ:47.9,maxZ:48.15});
      makeArch(court,[-20,0,46.2],3.5,3.8,M.gold,Math.PI/2,true);
    } else {
      const segmentA=box(court,[10.25,2.25,8.15],[11.7,4.5,.9],M.stone2); addBlocker(segmentA);
      const segmentB=box(court,[19.2,2.25,8.15],[1.6,4.5,.9],M.stone2); addBlocker(segmentB);
      // The south court gate is an open threshold. The Bifrost itself is at the far bridge pavilion.
      const low=box(court,[20,2.3,16.825],[.9,4.6,17.35],M.stone2);
      const high=box(court,[20,2.3,39.325],[.9,4.6,17.65],M.stone2);
      addBlocker(low); addBlocker(high);
      // A narrow service opening feeds the east wall gate from the old orchard lane.
      makeArch(court,[20,0,28.15],5.1,4.25,M.gold,Math.PI/2,true);
      const serviceSign=textSprite('EAST SERVICE GATE','#fff0cf','#534537'); serviceSign.scale.set(3.6,.56,1); serviceSign.position.set(20.15,5.1,28.15); court.add(serviceSign);
      obstacles.push({kind:'rect',minX:19.55,maxX:20.45,minZ:8.15,maxZ:25.55},{kind:'rect',minX:19.55,maxX:20.45,minZ:30.75,maxZ:48.15});
    }
    for(const z of [11,19,29,39,46]) makeColumn(court,side*19.35,z,.92,M.stone);
    for(const z of [16,28,40]) {
      makeColumn(court,side*14.7,z,1.05,M.stone);
      obstacles.push({x:side*14.7,z,radius:.62,object:court,kind:'pillar'});
    }
  }
  // Crisp gold coping unifies the courtyard walls and provides a readable silhouette.
  for(const side of [-1,1]) box(court,[side*20,4.65,28.15],[1.2,.22,40],M.gold);
  box(court,[-13.4,4.65,48.15],[13.2,.22,1.2],M.gold);
  box(court,[13.4,4.65,48.15],[13.2,.22,1.2],M.gold);
  obstacles.push({kind:'rect',minX:-20,maxX:-6.8,minZ:47.55,maxZ:48.75},{kind:'rect',minX:6.8,maxX:20,minZ:47.55,maxZ:48.75});
  // Transform-only watch posts remain data driven; each is a stop for the one existing guard.
  watchPostObjects.length=0;
  const postRoot=group(court); postRoot.name='WatchPosts';
  for(const stop of district.guard.points.filter(p=>p.post)) {
    const marker=group(postRoot,new THREE.Vector3(stop.x,0,stop.z)); marker.name=stop.post;
    const look=stop.look ?? {x:0,z:-1}; marker.rotation.y=Math.atan2(look.x,look.z);
    watchPostObjects.push(marker);
  }
  // Low, paired entrance stairs flank the central landing without pinching either lane.
  for(const side of [-1,1]) for(let i=0;i<3;i++) {
    const step=box(court,[side*7.1,.08+i*.055,9.3+i*.42],[3.1,.16+i*.1,1.12],i%2?M.stone2:M.stone);
    step.userData.noCameraCollision=true;
  }
  box(court,[0,.035,12.25],[13,.07,3.8],M.stone);
  // Three safe, readable patrol lanes wrap the rectangular fountain.
  for(const side of [-1,1]) for(const z of [20.5,35.5]) {
    const planter=group(court,new THREE.Vector3(side*11.7,0,z));
    box(planter,[0,.32,0],[2.5,.64,4.1],M.stone2);
    const leaves=[];
    for(let i=0;i<5;i++){const leaf=sphere(planter,[-.8+i*.4,1.02,0],[.52,.63,.48],(i%2?M.green2:M.green).clone(),12,true);leaf.material.transparent=true;leaves.push(leaf);}
    planter.userData.foliage=leaves;
    obstacles.push({x:side*11.7,z,radius:1.25,object:planter,kind:'tree'}); addBlocker(planter);
  }
  // Compact six-pillar rhythm from the reference; both side lanes remain wide open.
  for(const side of [-1,1]) for(const z of [16,28,40]) {
    makeColumn(court,side*14.7,z,1.05,M.stone);
    obstacles.push({x:side*14.7,z,radius:.62,object:court,kind:'pillar'});
  }
  // Rectangular low basin and original golden sun-statue landmark.
  const basin=group(court,new THREE.Vector3(0,0,28.2));
  box(basin,[0,.23,0],[5.2,.46,6.1],M.stone2);
  box(basin,[0,.48,0],[4.6,.16,5.5],M.teal);
  for(const x of [-2.42,2.42]) box(basin,[x,.68,0],[.38,.48,6.1],M.stone);
  for(const z of [-2.86,2.86]) box(basin,[0,.68,z],[5.2,.48,.38],M.stone);
  for(const x of [-2.55,2.55]) for(const z of [-2.95,2.95]) sphere(basin,[x,.94,z],[.32,.28,.32],M.gold,12,true);
  cylinder(basin,[0,.82,0],.62,.82,.54,M.gold,16);
  sphere(basin,[0,1.52,0],[.42,.64,.4],M.brightGold,16,true);
  sphere(basin,[0,2.16,0],[.24,.32,.24],M.gold,14,true);
  for(const side of [-1,1]) {
    const arm=tube(basin,[[side*.2,1.85,0],[side*.62,2.15,0],[side*.78,2.42,0]],.11,M.gold,12);
    sphere(basin,[side*.78,2.45,0],[.18,.2,.18],M.brightGold,10);
  }
  obstacles.push({x:0,z:28.2,radius:3.35,object:basin,kind:'fountain'}); addBlocker(basin);
  // North doors remain closed. Palace scale reads beyond them without an interior promise.
  const plaque=textSprite('THE PALACE  ·  CLOSED BY ORDER', '#fff0cf','#544333'); plaque.scale.set(5.2,.7,1); plaque.position.set(0,9.2,47.2); worldRoot.add(plaque);
}

function buildArcades() {
  // Seven repeating bays establish rhythm; each corridor remains open and walkable.
  for (const side of [-1, 1]) {
    const backX = side * 15.1;
    if (side < 0) {
      // Leave the marked west gallery arch physically open as an alternate escape.
      addBlocker(box(worldRoot, [backX, 2.1, -2.95], [.55, 4.2, 16.3], M.stone2));
      addBlocker(box(worldRoot, [backX, 2.1, 10.05], [.55, 4.2, .5], M.stone2));
    } else addBlocker(box(worldRoot, [backX, 2.1, -.6], [.55, 4.2, 21], M.stone2));
    for (let z = -9; z <= 7; z += 4) {
      makeArch(worldRoot, [side * 14.65, .1, z], 3.25, 3.8, M.stone, Math.PI / 2, false);
      const panel = box(worldRoot, [side * 15.38, 1.95, z], [.06, 1.9, .72], M.ink);
      const lit = box(worldRoot, [side * 15.34, 1.95, z], [.08, 1.45, .46], z % 8 === 7 ? M.red : M.teal);
      cylinder(worldRoot, [side * 14.62, 3.91, z], .23, .23, .12, M.gold, 12);
    }
    // Gallery handrail runs around the district and divides skyline from walkway.
    const rail = cylinder(worldRoot, [side * 15.55, .72, -.7], .09, .09, 21.5, M.gold, 12);
    rail.rotation.x = Math.PI / 2;
    for (let z = -10; z <= 9; z += 2.2) cylinder(worldRoot, [side * 15.55, .39, z], .055, .055, .72, M.stone, 10);
  }
  // Southern colonnade closes the district without cutting off its garden promenade.
  box(worldRoot, [0, .04, -11.8], [31, .16, 1.25], M.stone2);
  for (let x = -13.5; x <= 13.5; x += 4.5) {
    makeColumn(worldRoot, x, -11.1, .7, M.stone);
    makeArch(worldRoot, [x + 2.25, .05, -11.1], 4.5, 3.3, M.stone, 0, false);
  }
}

function buildHomes() {
  const homes = [
    { x: -8.8, z: -5.55, color: M.green2, sign: "ODIN'S STUDY", roof: M.gold },
    { x: 8.8, z: -5.55, color: M.red, sign: "FRIGGA'S HOUSE", roof: M.teal }
  ];
  for (const home of homes) {
    const g = group(worldRoot, new THREE.Vector3(home.x, 0, home.z));
    cylinder(g, [0, .7, 0], 2.5, 2.9, 1.4, M.stone, 28);
    cylinder(g, [0, 1.52, 0], 2.22, 2.35, .25, M.gold, 28);
    const door = cylinder(g, [0, .8, 2.55], .6, .6, 1.6, M.wood, 18); door.scale.x = .73;
    makeArch(g, [0, .1, 2.64], 1.55, 2.4, M.gold, 0, false);
    for (const s of [-1, 1]) {
      const w = box(g, [s * 1.3, 1.15, 2.82], [.32, .62, .08], M.teal);
      torus(g, [s * 1.3, 1.35, 2.88], .28, .045, M.gold, Math.PI, 0);
    }
    makeDome(g, [0, 1.66, 0], 2.2, 2.6, home.roof);
    // Woven pennant gives each home its own readable color accent.
    const pennant = box(g, [2.45, 2.1, .1], [.08, 1.45, .08], M.gold); pennant.rotation.z = -.08;
    const cloth = new THREE.Shape(); cloth.moveTo(0, .65); cloth.lineTo(1.05, .48); cloth.lineTo(.74, 0); cloth.lineTo(0, .1); cloth.closePath();
    const flag = mesh(new THREE.ShapeGeometry(cloth), home.color, g, { position: new THREE.Vector3(2.5, 2.42, .13) }); flag.rotation.y = -.12;
    g.add(textSprite(home.sign, '#f5e9ca', '#34433b')).position.set(0, 3.65, 2.05);
    // Keep the front doorway approach open; collide with the two wings and rear wall only.
    obstacles.push(
      { kind:'rect', minX:home.x-2.55, maxX:home.x-.78, minZ:home.z-2.55, maxZ:home.z+2.55 },
      { kind:'rect', minX:home.x+.78, maxX:home.x+2.55, minZ:home.z-2.55, maxZ:home.z+2.55 },
      { kind:'rect', minX:home.x-.78, maxX:home.x+.78, minZ:home.z-2.55, maxZ:home.z+2.48 }
    ); addBlocker(g);
  }
}

function buildFountain() {
  const { x, z } = district.landmarks.find(p => p.id === 'fountain') ?? { x: 6.2, z: -3.8 };
  const g = group(worldRoot, new THREE.Vector3(x, 0, z));
  cylinder(g, [0, .22, 0], 2.35, 2.55, .42, M.stone, 32);
  torus(g, [0, .45, 0], 2.25, .15, M.gold);
  cylinder(g, [0, .52, 0], 1.98, 2.12, .28, M.teal, 32);
  const water = cylinder(g, [0, .69, 0], 1.75, 1.75, .06, mat(0x60c7bd, { transparent: true, opacity: .78 }), 32);
  cylinder(g, [0, 1.05, 0], .62, .88, .72, M.stone, 20);
  torus(g, [0, 1.38, 0], .72, .11, M.gold);
  cylinder(g, [0, 1.58, 0], .39, .54, .32, M.stone2, 20);
  const orb = sphere(g, [0, 2.15, 0], [.48, .63, .48], M.teal, 18, true);
  for (let i = 0; i < 4; i++) {
    const a = i * Math.PI / 2;
    tube(g, [[0, 1.45, 0], [Math.cos(a) * .52, 1.93, Math.sin(a) * .52], [Math.cos(a) * .9, 1.46, Math.sin(a) * .9]], .045, M.brightGold);
  }
  obstacles.push({ x, z, radius: 2.35, object: g, kind: 'fountain' });
  addBlocker(g);
}

function buildGardens() {
  const plants = [
    [-11.4, -7.7], [-8.7, -8.2], [-4.6, -8.7], [4.8, -8.6], [8.5, -8], [11.6, -7.5],
    [-10.5, 2.2], [10.3, 2.2], [-8.3, 6.2], [8.3, 6.2], [-3, -4.5], [3.1, -4.6]
  ];
  plants.forEach((p, i) => makeTopiary(p[0], p[1], i % 3));
  for (const [x, z] of [[-11.5, -3], [11.3, -3], [-5.6, -7.9], [5.8, -7.9]]) makeBench(x, z);
}
function makeTopiary(x, z, variant = 0) {
  const g = group(worldRoot, new THREE.Vector3(x, 0, z));
  cylinder(g, [0, .42, 0], .78, .92, .8, M.stone2, 16);
  torus(g, [0, .82, 0], .76, .06, M.gold);
  const trunk = cylinder(g, [0, 1.42, 0], .2, .29, 1.25, M.wood, 12);
  const leaf = mat([0x4c934e, 0x548b64, 0x618a54][variant]);
  const leaves=[sphere(g, [0, 2.35, 0], [1.0, 1.05, .88], leaf.clone(), 18, true)];
  for(const l of leaves) l.material.transparent=true;
  addBlocker(g);
  leaves.push(sphere(g, [-.43, 2.55, .03], [.52, .62, .55], leaf.clone(), 14));
  leaves.push(sphere(g, [.43, 2.55, .03], [.52, .62, .55], leaf.clone(), 14));
  leaves.push(sphere(g, [0, 2.95, 0], [.56, .6, .54], M.green2.clone(), 14));
  leaves.forEach(l=>{l.material.transparent=true;});
  g.userData.foliage=leaves;
  const fruit = sphere(g, [.4, 2.02, .56], [.15, .18, .15], M.red, 12);
  const base = group(worldRoot, new THREE.Vector3(x, .03, z));
  obstacles.push({ x, z, radius: .68, object: g, kind: 'tree' });
}
function makeBench(x, z) {
  const g = group(worldRoot, new THREE.Vector3(x, 0, z));
  const seat = box(g, [0, .64, 0], [1.8, .18, .62], M.wood); outlined(seat, 1.025);
  for (const s of [-1, 1]) {
    box(g, [s * .68, .36, 0], [.15, .58, .54], M.gold);
    box(g, [s * .68, 1, -.2], [.16, .8, .16], M.stone2);
  }
}

function buildBifrost() {
  const { x = 0, z = 8.15 } = district.gate;
  const gate = group(worldRoot, new THREE.Vector3(x, .05, z));
  // Nested golden arch rings form a distinct destination, readable from across the square.
  makeArch(gate, [0, 0, -.25], 6.8, 6.0, M.stone, 0, true);
  makeArch(gate, [0, 0, -.06], 5.45, 5.2, M.gold, 0, true);
  cylinder(gate, [-3.35, 1.55, 0], .42, .56, 3.1, M.gold, 16);
  cylinder(gate, [3.35, 1.55, 0], .42, .56, 3.1, M.gold, 16);
  const threshold = box(gate, [0, .1, -.55], [5.7, .2, .75], M.teal); outlined(threshold, 1.035, 0x184e55);
  box(gate, [0, .25, -.55], [4.6, .05, .1], M.brightGold);
  for (let i = 0; i < 11; i++) {
    const xbar = -2.4 + i * .48;
    if (Math.abs(xbar) < .72) continue;
    const bar = cylinder(gate, [xbar, 1.63, -.18], .042, .042, 2.6, i % 3 === 0 ? M.red : M.gold, 10);
    if (i % 2 === 0) sphere(gate, [xbar, 2.98, -.18], [.095, .15, .095], M.brightGold, 10);
  }
  const center = sphere(gate, [0, 3.45, -.1], [.5, .68, .24], M.teal, 18, true);
  center.material = new THREE.MeshToonMaterial({ color: 0x72e8d8, gradientMap: toonRamp, emissive: 0x2eaaa2, emissiveIntensity: .42, transparent: true, opacity: .88 });
  for (const s of [-1, 1]) {
    for (let i = 0; i < 5; i++) {
      const step = box(gate, [s * (3.7 + i * .44), .1 + i * .12, -1.4 - i * .28], [.88, .22, .85], i % 2 ? M.stone2 : M.stone);
      step.rotation.y = s * -.09;
    }
  }
  addBlocker(center);
  gate.name = 'South court gate';
  const label = textSprite('PALACE COURT  /  MAIN GATE', '#fff3d4', '#38504a');
  label.position.set(0, 6.7, z); scene.add(label);
  const side = group(worldRoot, new THREE.Vector3(-14.1, .05, 7.35));
  makeArch(side, [0, 0, 0], 3.35, 3.6, M.stone, Math.PI / 2, false);
  const sign = textSprite('WEST GALLERY', '#fff0ce', '#4b5043'); sign.position.set(-13.7, 4, 7.4); scene.add(sign);
}

function buildBridge() {
  const bridge=group(worldRoot); bridge.name='RainbowBridge';
  box(bridge,[0,-.18,65],[6,.4,30],M.stone2);
  // Restrained color inlays sit beneath the deck edges and leave a wide, clean walking lane.
  const spectrum=[0xd95765,0xe88b4b,0xe4c75c,0x6ea969,0x53a8bd,0x697ac0,0x9270b5];
  for(let i=0;i<spectrum.length;i++) box(bridge,[-2.55+i*.85,-.405,65],[.72,.045,29.7],mat(spectrum[i]));
  for(const side of [-1,1]) {
    const rail=cylinder(bridge,[side*2.92,.55,65],.075,.075,30,M.gold,12); rail.rotation.x=Math.PI/2;
    for(let z=50;z<=80;z+=3) {
      cylinder(bridge,[side*2.92,.31,z],.055,.055,.58,M.brightGold,10);
      obstacles.push({kind:'rect',minX:side<0?-3.35:2.75,maxX:side<0?-2.75:3.35,minZ:z-1.55,maxZ:z+1.55});
    }
  }
}
function buildBifrostPavilion() {
  const p=group(worldRoot,new THREE.Vector3(0,0,83.5)); p.name='BifrostPavilion';
  cylinder(p,[0,-.12,0],5.25,5.55,.45,M.stone2,40);
  cylinder(p,[0,.12,0],4.85,5.05,.12,M.gold,40);
  cylinder(p,[0,.2,0],4.1,4.1,.08,M.stone,40);
  const ring=torus(p,[0,.29,0],3.15,.13,M.emerald); ring.rotation.x=Math.PI/2;
  const inner=torus(p,[0,.3,0],2.45,.045,M.teal); inner.rotation.x=Math.PI/2;
  makeArch(p,[0,0,-1.3],5.4,5.6,M.gold,0,true);
  // A visible walk-through portal marks the actual arrival/mission trigger.
  bifrostPortal=group(p,new THREE.Vector3(0,2.35,-1.24)); bifrostPortal.name='BifrostPortal';
  torus(bifrostPortal,[0,0,0],1.62,.12,M.emerald);
  torus(bifrostPortal,[0,0,.015],1.39,.045,M.brightGold);
  const portalSurface=mesh(new THREE.CircleGeometry(1.42,40),new THREE.MeshBasicMaterial({color:0x4bd4ba,transparent:true,opacity:.34,side:THREE.DoubleSide,depthWrite:false}),bifrostPortal,{position:new THREE.Vector3(0,0,-.035)});
  portalSurface.userData.bifrostSurface=true;
  bifrostArrival=new THREE.Object3D(); bifrostArrival.name='BifrostArrival'; bifrostArrival.position.set(3.2,0,5.5); bifrostArrival.rotation.y=Math.PI; scene.add(bifrostArrival);
  for(const side of [-1,1]) for(let i=0;i<4;i++) box(p,[side*6.1,.08+i*.1,-2.5+i*.72],[1.6,.16+i*.12,1.25],i%2?M.stone2:M.stone);
  const title=textSprite('BIFROST  /  ASGARD THIS WAY', '#fff0cf','#3b5148'); title.scale.set(4.8,.62,1); title.position.set(0,6.3,0); p.add(title);
}

function buildMarket() {
  const { x, z } = district.landmarks.find(p => p.id === 'market') ?? { x: 6.9, z: -.15 };
  const g = group(worldRoot, new THREE.Vector3(x, 0, z));
  // Round stall with a scalloped canopy, avoiding a box-like backdrop.
  const counter = cylinder(g, [0, .63, 0], 1.15, 1.3, 1.2, M.wood, 20); counter.scale.z = .74;
  torus(g, [0, 1.16, 0], 1.14, .1, M.gold);
  for (let i = 0; i < 8; i++) {
    const a = i * Math.PI / 4, x = Math.cos(a) * 1.1, z = Math.sin(a) * .75;
    cylinder(g, [x, 1.8, z], .06, .08, 1.35, M.gold, 10);
  }
  const canopy = mesh(new THREE.ConeGeometry(1.65, .85, 8), M.red, g, { position: new THREE.Vector3(0, 2.55, 0) });
  canopy.rotation.y = Math.PI / 8; outlined(canopy, 1.035);
  torus(g, [0, 2.14, 0], 1.42, .08, M.brightGold);
  for (let i = 0; i < 5; i++) sphere(g, [-.76 + i * .38, 1.4, .28], [.22, .22, .22], i % 2 ? M.gold : M.red, 10);
  const plaque = textSprite('GOLDEN APPLES  ·  NONE MISSING', '#fff0cf', '#7a4c2d'); plaque.position.set(0, 3.23, .85); g.add(plaque);
}

function createCape(parent, width = .95, length = 1.3, material = M.green) {
  const geo = new THREE.BufferGeometry();
  const rows = 7, cols = 6, positions = [], indices = [];
  for (let r = 0; r <= rows; r++) for (let c = 0; c <= cols; c++) {
    const t = r / rows, u = c / cols - .5;
    const w = width * (.34 + t * .68);
    positions.push(u * w, .46 - t * length, -.22 - t * .22 + Math.abs(u) * .12);
  }
  for (let r = 0; r < rows; r++) for (let c = 0; c < cols; c++) {
    const a = r * (cols + 1) + c, b = a + cols + 1;
    indices.push(a, b, a + 1, b, b + 1, a + 1);
  }
  geo.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3)); geo.setIndex(indices); geo.computeVertexNormals();
  const capeMat = new THREE.MeshToonMaterial({ color: material.color, gradientMap: toonRamp, side: THREE.DoubleSide });
  const cape = mesh(geo, capeMat, parent); cape.userData.basePositions = geo.attributes.position.array.slice(); cape.userData.cape = true;
  // Fine gold piping follows both edges of the flared hem.
  const seam = new THREE.LineBasicMaterial({ color: palette.gold });
  for (const side of [-1, 1]) {
    const points = [];
    for (let r = 0; r <= rows; r++) {
      const t = r / rows, w = width * (.34 + t * .68);
      points.push(new THREE.Vector3(side * w * .5, .46 - t * length, -.22 - t * .22));
    }
    parent.add(new THREE.Line(new THREE.BufferGeometry().setFromPoints(points), seam));
  }
  return cape;
}

function createPlayer() {
  const start = district.playerStart;
  player = group(worldRoot, new THREE.Vector3(start.x, 0, start.z)); player.name = 'Loki · original wildfire trickster';
  const character = group(player, new THREE.Vector3(0, .04, 0)); player.userData.visual = character;
  const torso = mesh(new THREE.CapsuleGeometry(.35, .58, 5, 12), M.dark, character, { position: new THREE.Vector3(0, 1.15, 0) }); torso.scale.set(.86, 1.32, .72);
  const coatProfile = [new THREE.Vector2(.39, .15), new THREE.Vector2(.48, .24), new THREE.Vector2(.43, .54), new THREE.Vector2(.31, 1.13), new THREE.Vector2(.27, 1.56), new THREE.Vector2(0, 1.62)];
  const coat = mesh(new THREE.LatheGeometry(coatProfile, 20), M.green, character, { position: new THREE.Vector3(0, 0, .08) }); outlined(coat, 1.025);
  createCape(character, 1.36, 1.48, M.green);
  // Gold shoulder mantles and chest emblem break up the green silhouette.
  for (const s of [-1, 1]) {
    const pauldron = sphere(character, [s * .42, 1.68, 0], [.33, .21, .37], M.gold, 16, true); pauldron.rotation.z = s * -.25;
    const armPivot = group(character, new THREE.Vector3(s * .49, 1.4, 0));
    sphere(armPivot, [0, -.29, .02], [.17, .44, .18], M.green2, 14, true);
    sphere(armPivot, [0, -.64, .06], [.135, .16, .13], M.cream, 12);
    player.userData['arm' + s] = armPivot;
    const legPivot = group(character, new THREE.Vector3(s * .17, .33, 0));
    capsulePart(legPivot, [0, -.2, 0], .135, .39, M.dark);
    sphere(legPivot, [0, -.42, .12], [.17, .13, .29], M.green, 12);
    player.userData['leg' + s] = legPivot;
  }
  const belt = torus(character, [0, .64, .1], .37, .045, M.brightGold);
  belt.rotation.x = Math.PI / 2;
  const clasp = sphere(character, [0, .64, .42], [.12, .12, .05], M.emerald, 12);
  const neck = cylinder(character, [0, 1.92, 0], .13, .15, .22, M.cream, 12);
  const head = sphere(character, [0, 2.15, .03], [.34, .38, .31], M.cream, 20, true);
  sphere(character, [0, 2.39, -.03], [.36, .22, .32], M.dark, 16);
  // Tousled dark hair, bold brows, bright eyes and a sly, visible face.
  for (const s of [-1, 1]) {
    sphere(character, [s * .28, 2.08, -.05], [.12, .34, .18], M.dark, 14);
    sphere(character, [s * .125, 2.18, .31], [.075, .038, .028], M.cream, 12);
    sphere(character, [s * .125, 2.18, .34], [.026, .032, .018], M.ink, 10);
    const brow = sphere(character, [s * .13, 2.26, .31], [.11, .036, .027], M.dark, 10); brow.rotation.z = s * -.16;
    tube(character, [[s * .05, 2.42, -.12], [s * .12, 2.66, -.12], [s * .31, 2.78, -.08], [s * .47, 2.74, -.04]], .085, M.dark, 18);
    tube(character, [[s * .23, 2.46, -.1], [s * .42, 2.61, -.09], [s * .53, 2.57, -.03]], .065, M.dark, 16);
    tube(character, [[s * .08, 2.46, .02], [s * .27, 2.35, -.1], [s * .35, 2.2, -.12], [s * .39, 1.97, -.12]], .09, M.dark, 18);
  }
  // High folded collar and the cape's embroidered diamond, matching the supplied sheet.
  const collarL = mesh(new THREE.ConeGeometry(.24,.52,5), M.green, character, { position:new THREE.Vector3(-.31,1.73,-.02) }); collarL.rotation.z=.45; outlined(collarL,1.04);
  const collarR = mesh(new THREE.ConeGeometry(.24,.52,5), M.green, character, { position:new THREE.Vector3(.31,1.73,-.02) }); collarR.rotation.z=-.45; outlined(collarR,1.04);
  for (const y of [1.58,1.34,1.1]) { const stitch=box(character,[0,y,.31],[.42,.025,.025],M.gold); }
  const backSigil = group(character,new THREE.Vector3(0,1.05,-.48));
  const sigilTop=mesh(new THREE.OctahedronGeometry(.18,0),M.gold,backSigil); sigilTop.scale.set(.65,1.2,.12);
  const sigilBottom=mesh(new THREE.OctahedronGeometry(.12,0),M.brightGold,backSigil,{position:new THREE.Vector3(0,-.3,0)}); sigilBottom.scale.z=.2;
  sphere(character, [0, 2.13, .34], [.065, .09, .08], M.cream, 12);
  const smile = tube(character, [[-.1, 2.01, .31], [0, 1.985, .34], [.11, 2.025, .31]], .014, M.ink, 12);
  const claspGem = sphere(character, [0, 1.46, .33], [.14, .19, .06], M.gold, 12);
  outlined(claspGem, 1.08);
  // Contact shadow makes the graphic silhouette feel planted instead of floating.
  const shadow = sphere(player, [0, .045, 0], [.53, .025, .39], new THREE.MeshBasicMaterial({ color: 0x5e4d3b, transparent: true, opacity: .28 }), 18);
  shadow.userData.noShadow = true; shadow.renderOrder = 2;
  player.userData.shadow = shadow; player.userData.disguised = false; player.userData.face = head;
  player.userData.homeMaterials = character.children.map(o => o.material).filter(Boolean);
}
function capsulePart(parent, position, radius, length, material) {
  const o = mesh(new THREE.CapsuleGeometry(radius, length, 4, 10), material, parent, { position: new THREE.Vector3(...position) }); return o;
}

function createGuard() {
  district.guard.points.forEach((pt, index) => {
  const actor = group(worldRoot, new THREE.Vector3(pt.x, 0, pt.z));
  actor.name = `Royal watch · ${pt.post ?? `post ${index + 1}`}`;
  actor.rotation.y=Math.atan2(pt.look?.x ?? 0,pt.look?.z ?? -1);
  actor.userData.postName=pt.post ?? `Watch Post ${index + 1}`;
  actor.userData.postPosition=new THREE.Vector3(pt.x,0,pt.z);
  actor.userData.postYaw=actor.rotation.y;
  actor.userData.watchState='PATROL'; actor.userData.watchSuspicion=0;
  actor.userData.watchTarget=new THREE.Vector3(pt.x,0,pt.z);
  actor.userData.watchLastSeen=new THREE.Vector3(pt.x,0,pt.z);
  watchers.push(actor); if(index===0) guard=actor;
  const v = group(actor, new THREE.Vector3(0, 0, 0)); actor.userData.visual = v;
  const body = mesh(new THREE.CapsuleGeometry(.38, .67, 5, 12), M.red, v, { position: new THREE.Vector3(0, 1.15, 0) }); body.scale.set(1.1, 1.3, .92); outlined(body, 1.04);
  const breast = sphere(v, [0, 1.42, .2], [.43, .47, .19], M.bronze, 16, true);
  for (const s of [-1, 1]) {
    const plate = sphere(v, [s * .43, 1.69, .03], [.36, .24, .35], M.gold, 14, true); plate.rotation.z = s * -.2;
    const arm = group(v, new THREE.Vector3(s * .52, 1.37, .01));
    capsulePart(arm, [0, -.28, 0], .17, .42, M.bronze); sphere(arm, [0, -.61, .06], [.15, .16, .14], M.cream, 10);
    actor.userData['arm' + s] = arm;
    const leg = group(v, new THREE.Vector3(s * .2, .35, 0)); capsulePart(leg, [0, -.23, 0], .155, .42, M.red); actor.userData['leg' + s] = leg;
  }
  cylinder(v, [0, 1.89, 0], .15, .15, .22, M.cream, 12);
  sphere(v, [0, 2.16, 0], [.36, .38, .32], M.bronze, 18, true);
  // Closed faceplate with a narrow readable slit, rather than a visible human face.
  sphere(v,[0,2.16,.02],[.37,.39,.33],M.ink,16,true);
  const visor = box(v, [0, 2.18, .31], [.34, .055, .045], M.teal); outlined(visor, 1.05);
  const brow = box(v,[0,2.27,.3],[.42,.1,.08],M.bronze); outlined(brow,1.04);
  const helm = mesh(new THREE.SphereGeometry(.39,12,8,0,Math.PI*2,0,Math.PI*.53), M.gold, v, { position: new THREE.Vector3(0, 2.42, -.02) }); outlined(helm, 1.035);
  const crest=mesh(new THREE.ConeGeometry(.25,.68,10),M.red,v,{position:new THREE.Vector3(0,2.89,-.05)}); crest.rotation.z=Math.PI; crest.scale.set(1.15,.8,.45); outlined(crest,1.04);
  for(let i=0;i<5;i++){const plume=sphere(v,[-.2+i*.1,2.77,-.06],[.09,.22,.055],M.red,10);plume.rotation.z=(i-2)*-.16;}
  // A broad crimson sash crosses the bronze breastplate and hangs at the waist.
  const sash=box(v,[.02,1.18,.35],[.67,.16,.12],M.red); sash.rotation.z=-.46; outlined(sash,1.03);
  const sashTail=mesh(new THREE.ConeGeometry(.16,.52,4),M.red,v,{position:new THREE.Vector3(.12,.72,.3)}); outlined(sashTail,1.03);
  const shield = group(v, new THREE.Vector3(.67, .8, .2));
  const shieldBody = mesh(new THREE.CylinderGeometry(.48, .58, .16, 10), M.gold, shield); shieldBody.rotation.z = Math.PI / 2;
  shieldBody.scale.y = 1.25; outlined(shieldBody, 1.08);
  const badge = sphere(shield, [0, .03, .12], [.2, .27, .07], M.red, 12);
  const staff = cylinder(v, [-.78, .88, -.05], .045, .055, 2.0, M.wood, 8); staff.rotation.z = -.12;
  sphere(v, [-.82, 1.94, -.05], [.15, .18, .15], M.brightGold, 12);
  const spearTip=mesh(new THREE.ConeGeometry(.11,.38,8),M.brightGold,v,{position:new THREE.Vector3(-.79,2.18,-.05)}); spearTip.rotation.z=Math.PI;
  const badgeText = textSprite(pt.post?.toUpperCase() ?? 'WATCH', '#fff0cc', '#913e3e'); badgeText.scale.set(1.85, .38, 1); badgeText.position.set(0, 3.65, 0); actor.add(badgeText);
  const postRingMaterial = new THREE.MeshBasicMaterial({ color: 0xd8ad5e, transparent: true, opacity: .52, side: THREE.DoubleSide });
  const postRing = torus(actor, [0, .055, 0], .58, .035, postRingMaterial); postRing.rotation.x = Math.PI / 2;
  actor.userData.postRing = postRing; actor.userData.postRingMaterial = postRingMaterial;
  actor.userData.visualChildren = v.children;
  });
}

function createNpcs() {
  watchers.forEach((object,index)=>npcs.push({ id:`royal-watch-${index+1}`,type:'guard',name:object.userData.postName,talkRadius:2.7,object }));
  for (const data of district.npcs.filter(n => n.type !== 'guard')) {
    const object = createVendor(data); npcs.push({ ...data, object });
  }
}
function createVendor(data) {
  const g = group(worldRoot, new THREE.Vector3(data.x, 0, data.z)); g.name = data.name;
  const visual = group(g, new THREE.Vector3(0, 0, 0));
  const robe = mesh(new THREE.CapsuleGeometry(.33, .62, 5, 12), M.green2, visual, { position: new THREE.Vector3(0, 1.04, 0) }); robe.scale.set(1.15, 1.28, .92); outlined(robe, 1.04);
  const apron = mesh(new THREE.LatheGeometry([new THREE.Vector2(.19,.2),new THREE.Vector2(.25,.28),new THREE.Vector2(.22,.82),new THREE.Vector2(.16,1.2),new THREE.Vector2(0,1.22)],12),M.red,visual,{position:new THREE.Vector3(0,.15,.24)});
  for (const s of [-1,1]) {
    const arm = group(visual,new THREE.Vector3(s*.43,1.34,.02)); capsulePart(arm,[0,-.28,.02],.14,.38,M.stone2); sphere(arm,[0,-.53,.1],[.13,.12,.14],M.cream,10);
    g.userData['arm'+s]=arm;
  }
  cylinder(visual,[0,1.78,0],.13,.14,.18,M.cream,12);
  sphere(visual,[0,2.02,.02],[.32,.34,.29],M.cream,16,true);
  sphere(visual,[0,2.24,-.03],[.34,.18,.3],M.wood,14);
  const hat = mesh(new THREE.ConeGeometry(.43,.48,8),M.gold,visual,{position:new THREE.Vector3(0,2.48,-.02)}); outlined(hat,1.03);
  sphere(visual,[0,2.34,.02],[.42,.1,.36],M.gold,12);
  for (const s of [-1,1]) { sphere(visual,[s*.105,2.04,.28],[.052,.028,.02],M.ink,10); sphere(visual,[s*.13,1.96,.28],[.08,.026,.025],M.wood,10); }
  const baskets = group(g,new THREE.Vector3(.66,.9,.08));
  cylinder(baskets,[0,0,0],.39,.48,.56,M.wood,12); torus(baskets,[0,.29,0],.38,.05,M.gold);
  for(let i=0;i<4;i++) sphere(baskets,[-.2+i*.13,.38,.05],[.12,.14,.12],i%2?M.red:M.gold,10);
  g.userData.visual=visual; g.userData.npcType='vendor'; g.userData.phase=0;
  const marker=textSprite('ASK MIRA', '#fff0ce', '#48694a'); marker.position.set(0,3.15,0); marker.userData.interactionLabel=true; marker.visible=false; g.add(marker);
  return g;
}

function nearestNpc() {
  if (!player) return null;
  let nearest = null, best = Infinity;
  for (const npc of npcs) {
    const d = distanceXZ(player.position, npc.object.position);
    if (d <= npc.talkRadius && d < best) { nearest = npc; best = d; }
  }
  return nearest;
}
function talkToNearestNpc() {
  if (!playing) return;
  const npc = nearestNpc();
  if (!npc) { setToast('No one close enough to hear that. Try moving toward the watch or the apple stall.'); return; }
  dialogueNpc = npc; dialogueOpen = true; keys.clear();
  ui.dialogueName.textContent = npc.name;
  dialogueReaction = npc.type === 'guard'
    ? (guardState === 'CHASE' ? 'The guard is out of breath and very committed to procedure.' : 'The guard eyes Loki, then remembers Loki does not have horns today.')
    : 'Mira has heard every excuse in the Nine Realms. She is still listening.';
  ui.dialogueLine.textContent = dialogueReaction;
  renderDialogueChoices(); ui.dialogue.classList.remove('hidden');
}
function renderDialogueChoices() {
  if (!dialogueNpc) return;
  const options = dialogueNpc.type === 'guard' ? [
    '“The west gallery bell is ringing. You should check it.”',
    '“That helmet is magnificent. May I inspect it up close?”',
    '“Royal order: let me through. I am almost certain.”'
  ] : [
    '“I am investigating the apple. What did you see?”',
    '“Could you ring the counter bell? It sounds urgent.”',
    '“Official apple inspector. I need a very unofficial favor.”'
  ];
  ui.dialogueOptions.replaceChildren(...options.map((label, i) => {
    const button = document.createElement('button'); button.className = 'dialogue-choice';
    button.innerHTML = '<kbd>' + (i + 1) + '</kbd><span>' + label + '</span>';
    button.onclick = () => chooseDialogue(i); return button;
  }));
}
function chooseDialogue(index) {
  if (!dialogueOpen || !dialogueNpc) return;
  const isGuard = dialogueNpc.type === 'guard';
  if (isGuard) {
    if (index === 0) {
      if (guardState === 'CHASE' && guardTrust < .35) { suspicion = Math.min(1, suspicion + .25); dialogueReaction = '“A bell? I hear no bell. I do hear a suspicious amount of confidence.”'; }
      else { guardTrust = Math.min(1, guardTrust + .08); investigatePoint.set(10.1, 0, 1.8); guardHoldUntil = performance.now() + 7500; guardReactUntil=performance.now()+280; guardState = 'INVESTIGATE'; suspicion = Math.max(0, suspicion - .18); dialogueReaction = '“The west gallery? Good catch. Stay here, suspiciously.” The guard hurries toward the market instead.'; setToast('Your lie draws the guard away from both exits.', 3); }
    } else if (index === 1) {
      guardTrust = Math.min(1, guardTrust + .32); suspicion = Math.max(0, suspicion - .28); investigatePoint.set(3.4, 0, 7.4); guardHoldUntil = performance.now() + 6000; guardReactUntil=performance.now()+280; guardState = 'INVESTIGATE'; dialogueReaction = '“Finally, someone appreciates proper helmet ventilation.” The guard poses for a moment.'; setToast('Flattery buys a calm opening at the gate.', 3);
    } else if (guardTrust >= .34 || player.userData.disguised) {
      guardTrust = Math.max(0, guardTrust - .05); investigatePoint.set(2.8, 0, 7.1); guardHoldUntil = performance.now() + 6500; guardReactUntil=performance.now()+280; guardState = 'INVESTIGATE'; suspicion = Math.max(0, suspicion - .1); dialogueReaction = '“Very well. I will personally supervise… from over there.”';
    } else { suspicion = Math.min(1, suspicion + .35); guardState = 'SUSPICIOUS'; investigatePoint.copy(player.position); dialogueReaction = '“That order has a familiar Loki-shaped signature.” Your bluff makes the guard wary.'; setToast('The guard doubts your story. Break sight or try a better line.', 3); }
  } else {
    if (index === 0) { guardTrust = Math.min(1, guardTrust + .12); suspicion = Math.max(0, suspicion - .06); dialogueReaction = '“A tall guard asked about an apple, then walked toward the west gallery. Odd fellow.” Mira marks the alternate passage on your map.'; setToast('Mira’s rumor points toward the west side passage.', 3.4); }
    else if (index === 1) { const point = new THREE.Vector3(10.1, 0, 1.8); sound = { point, until: performance.now() + 3800 }; investigatePoint.copy(point); if (guardState !== 'CHASE') guardState = 'INVESTIGATE'; pulseAt(point, palette.gold); dialogueReaction = 'Mira rings the counter bell. The guard turns toward the market.'; setToast('Mira’s bell pulls the guard east.', 3); }
    else { suspicion = Math.min(1, suspicion + .16); dialogueReaction = '“Inspector? Then inspect this invoice.” Mira waves a very long bill; the guard notices the commotion.'; if (guardState !== 'CHASE') { investigatePoint.copy(dialogueNpc.object.position); guardState = 'SUSPICIOUS'; } setToast('Your title is unconvincing, but the commotion changes the patrol.', 3); }
  }
  if (isGuard) {
    const watched = dialogueNpc.object, d = watched.userData;
    if (guardState === 'INVESTIGATE') { d.watchState = 'INVESTIGATE'; d.watchTarget.copy(investigatePoint); d.watchReactUntil = performance.now() + 260; d.watchHoldUntil = guardHoldUntil || performance.now() + 5000; }
    else if (guardState === 'SUSPICIOUS') { d.watchState = 'SUSPICIOUS'; d.watchLastSeen.copy(player.position); d.watchSuspicion = Math.min(1, Math.max(d.watchSuspicion, suspicion)); }
  }
  if (!isGuard && index === 1) playSfx('bell');
  if (!isGuard && index === 2) playSfx('sound');
  ui.dialogueLine.textContent = dialogueReaction;
}
function closeDialogue() {
  dialogueOpen = false; dialogueNpc = null; ui.dialogue.classList.add('hidden'); keys.clear();
}

function createProps() {
  for (const prop of district.props) {
    if (prop.type === 'bell') createBell(prop); else if (prop.type === 'crate') createCrate(prop);
  }
}
function createBell(prop) {
  const g = group(worldRoot, new THREE.Vector3(prop.x, 0, prop.z)); g.name = prop.id;
  cylinder(g, [0, .3, 0], .56, .62, .58, M.stone2, 16);
  cylinder(g, [0, 1.07, 0], .1, .13, 1.1, M.gold, 12);
  sphere(g, [0, 1.73, 0], [.33, .34, .33], M.gold, 16, true);
  const bell = mesh(new THREE.SphereGeometry(.34, 16, 12, 0, Math.PI * 2, 0, Math.PI * .55), M.brightGold, g, { position: new THREE.Vector3(0, 1.8, 0) }); bell.rotation.z = Math.PI;
  torus(g, [0, 1.68, 0], .31, .055, M.bronze);
  cylinder(g, [0, 2.12, 0], .08, .08, .19, M.wood, 10);
  const clapper = sphere(g, [0, 1.56, 0], [.095, .11, .095], M.bronze, 10);
  const marker = textSprite('RING ME', '#fff2d2', '#80602e'); marker.scale.set(1.75, .44, 1); marker.position.set(0, 2.55, 0); marker.userData.interactionLabel=true; marker.visible=false; g.add(marker);
  g.userData.kind = 'bell'; g.userData.range = prop.radius; g.userData.noise = prop.noise; g.userData.label = prop.label; g.userData.bell = bell; g.userData.clapper = clapper;
  movingObjects.push(g); obstacles.push({ x: prop.x, z: prop.z, radius: .5, object: g, kind: 'bell' });
}
function createCrate(prop) {
  const g = group(worldRoot, new THREE.Vector3(prop.x, .05, prop.z)); g.name = prop.id;
  const body = mesh(new THREE.BoxGeometry(1.05, .92, .9, 2, 2, 2), M.wood, g, { position: new THREE.Vector3(0, .48, 0) }); outlined(body, 1.035);
  for (let i = 0; i < 4; i++) {
    const x = -.43 + i * .29;
    const plank = box(g, [x, .48, .47], [.22, .85, .055], i % 2 ? M.wood : M.bronze);
    plank.rotation.z = .02;
  }
  for (const y of [.18, .79]) {
    const band = box(g, [0, y, .51], [1.06, .1, .06], M.gold);
  }
  const apple = sphere(g, [0, 1.1, .1], [.24, .26, .24], M.red, 12);
  g.userData.kind = 'crate'; g.userData.range = prop.radius; g.userData.label = prop.label; g.userData.noise = prop.noise; g.userData.target = g.position.clone();
  movingObjects.push(g); obstacles.push({ x: prop.x, z: prop.z, radius: .67, object: g, kind: 'crate' });
}

function buildCloudBanks() {
  const cloudMat = new THREE.MeshToonMaterial({ color: 0xc1ddd4, gradientMap: toonRamp, transparent: true, opacity: .58 });
  for (let i = 0; i < 18; i++) {
    const x = -36 + i * 4.2, z = 18 + ((i * 5) % 15), y = 2 + (i % 4) * 1.25;
    const p = sphere(scene, [x, y, z], [4 + (i % 3), 2.6 + (i % 2), 3.5], cloudMat, 14);
    p.castShadow = false; p.receiveShadow = false;
  }
}

function beginGame() {
  unlockAudio(); lastGuideKey = '';
  closeDialogue(); guardTrust = .12; guardHoldUntil = 0; cameraActualDistance = cameraDistance;
  ui.menu.classList.add('hidden'); ui.hud.classList.remove('hidden'); ui.crosshair.classList.remove('hidden');
  ui.profile.classList.add('hidden'); ui.options.classList.add('hidden'); ui.end.classList.add('hidden');
  playing = true; paused = false; mapOpen = false; ui.map.classList.remove('hidden'); ui.map.classList.add('map-compact'); $('map-hint').textContent = 'M · EXPAND DISTRICT MAP';
  if (player) {
    player.position.set(district.playerStart.x, 0, district.playerStart.z);
    player.rotation.set(0, 0, 0); moveVelocity.set(0, 0, 0); player.userData.disguised = false;
    player.userData.visual.visible = true;
  }
  if (guard) {
    for(const w of watchers){const d=w.userData;w.position.copy(d.postPosition);w.rotation.y=d.postYaw;d.watchState='PATROL';d.watchSuspicion=0;d.watchTarget.copy(d.postPosition);d.watchLastSeen.copy(d.postPosition);d.lastHeard=null;d.lastDecoy=null;d.watchHoldUntil=0;}
    patrolIndex = 0;
  }
  suspicion = 0; crisis = false; escaped = false; guardState = 'PATROL'; sound = null;
  caughtUntil=0;lastPlayerSpeed=0;
  bridgeGateOpen=false; bridgeStarted=false; teleporting=false; montageQueued=false; bridgeGate.userData.open=false;
  if(player?.userData.visual) player.userData.visual.scale.setScalar(1);
  bridgeGate.userData.doors[0].rotation.y=0; bridgeGate.userData.doors[1].rotation.y=0;
  if (decoy) { scene.remove(decoy); decoy = null; }
  toast('Wander first. The court has a habit of noticing things.', 4);
  updateGuide();
  renderer.domElement.focus();
}
function setToast(message, duration = 2.8) {
  ui.toast.textContent = message; ui.toast.classList.remove('hidden'); toastUntil = performance.now() + duration * 1000;
}
function unlockAudio() {
  const AudioCtor = window.AudioContext || window.webkitAudioContext;
  if (!AudioCtor) return;
  if (!audioContext) audioContext = new AudioCtor();
  if (audioContext.state === 'suspended') audioContext.resume();
}
function playSfx(kind) {
  if (!sfxEnabled) return;
  unlockAudio(); if (!audioContext) return;
  const now = audioContext.currentTime;
  const notes = {
    illusion: [[520, .08], [780, .14], [1040, .2]],
    sound: [[250, .06], [410, .13]],
    bell: [[660, .04], [990, .2]],
    gate: [[180, .04], [330, .17], [494, .3]],
    caught: [[420, .02], [290, .1], [190, .22]],
    portal: [[392, .02], [587, .12], [784, .28]]
  }[kind] ?? [[440, .1]];
  for (const [frequency, offset] of notes) {
    const oscillator = audioContext.createOscillator(), gain = audioContext.createGain();
    oscillator.type = kind === 'sound' || kind === 'gate' ? 'triangle' : 'sine';
    oscillator.frequency.setValueAtTime(frequency, now + offset);
    gain.gain.setValueAtTime(.0001, now + offset);
    gain.gain.exponentialRampToValueAtTime(kind === 'caught' ? .075 : .045, now + offset + .012);
    gain.gain.exponentialRampToValueAtTime(.0001, now + offset + .22);
    oscillator.connect(gain); gain.connect(audioContext.destination);
    oscillator.start(now + offset); oscillator.stop(now + offset + .24);
  }
}
function speakGuide(text) {
  if (!voiceEnabled || !('speechSynthesis' in window)) return;
  window.speechSynthesis.cancel();
  const utterance = new SpeechSynthesisUtterance(text);
  utterance.rate = .96; utterance.pitch = 1.02; utterance.volume = .75;
  window.speechSynthesis.speak(utterance);
}
function updateGuide() {
  if (!playing || !player || !guideUi.text) return;
  let key, text, spoken;
  const nearGate = bridgeGate && distanceXZ(player.position, bridgeGate.position) < 7.2;
  const nearPassage = distanceXZ(player.position, district.escape.find(e => e.id === 'west-side-passage')) < 4.5;
  if (!crisis) {
    key = 'explore'; text = 'WASD explores · hold RMB and drag to orbit.<br>Walk north toward the palace gate to begin.'; spoken = 'Explore Asgard with W A S D. Hold the right mouse button and drag to orbit. Head north when you are ready.';
  } else if (escaped || bridgeStarted) {
    key = 'bridge'; text = 'Cross the Rainbow Bridge to the glowing Bifrost ring.<br>The five watch posts stay back in the courtyard.'; spoken = 'Cross the Rainbow Bridge and step into the glowing Bifrost ring.';
  } else if (nearGate) {
    key = 'gate'; text = 'The Bridge Gate opens as you approach.<br>The West Passage is an alternate route to the same bridge.'; spoken = 'The Bridge Gate opens when you approach. The West Passage is another way to the bridge.';
  } else if (nearPassage) {
    key = 'passage'; text = 'This West Passage skips the gate and joins the bridge.<br>Keep an eye on the minimap for all five watchers.'; spoken = 'The West Passage joins the bridge. Check your minimap for the five watchers.';
  } else {
    key = 'tricks'; text = 'Q places an illusion at the cursor · E sends a sound there.<br>Watch helmets turn, then use the opening. T lets you talk.'; spoken = 'Use Q to place an illusion and E to send a sound toward your cursor. Watch who turns, then move through the opening.';
  }
  guideUi.text.innerHTML = text;
  guideUi.count.textContent = `${watchers.length} WATCH POSTS`;
  if (key !== lastGuideKey) { lastGuideKey = key; speakGuide(spoken); }
}
function showCrisis() {
  if (crisis) return; crisis = true; crisisAt = performance.now();
  ui.objective.textContent = 'REACH THE BRIDGE';
  ui.subtitle.textContent = 'Use the Bridge Gate or slip through the West Passage.';
  ui.intro.classList.remove('hidden');
  setToast('Royal watch: “Nobody leaves until the golden apple turns up.”', 3.5);
  playSfx('gate'); lastGuideKey = ''; updateGuide();
  window.setTimeout(() => ui.intro.classList.add('hidden'), 3100);
}
function setMap(open) {
  mapOpen = open; ui.map.classList.remove('hidden'); ui.map.classList.toggle('map-expanded', open); ui.map.classList.toggle('map-compact', !open);
  $('map-hint').textContent = open ? 'M · CLOSE MAP' : 'M · EXPAND DISTRICT MAP';
  if (open) drawMap(true);
}
function pauseGame() {
  if (!playing) return;
  paused = !paused;
  if (paused) {
    const overlay = document.createElement('div'); overlay.id = 'pause-screen'; overlay.className = 'modal';
    overlay.innerHTML = '<span class="label">TIME FOR AN ALIBI</span><h2>Paused</h2><p>Take a breath. The guard is frozen mid-suspicion.</p><button class="button button-primary" id="resume-button"><span>BACK TO THE COURTYARD</span><b>↗</b></button>';
    document.body.append(overlay); $('resume-button').onclick = () => { overlay.remove(); paused = false; };
  } else $('pause-screen')?.remove();
}

function getAimPoint() {
  raycaster.setFromCamera(pointer, camera);
  const hit = new THREE.Vector3();
  if (raycaster.ray.intersectPlane(groundPlane, hit)) {
    hit.x = THREE.MathUtils.clamp(hit.x, district.bounds.minX + .5, district.bounds.maxX - .5);
    hit.z = THREE.MathUtils.clamp(hit.z, district.bounds.minZ + .5, district.bounds.maxZ - .5);
    return hit;
  }
  return mouseGround.clone();
}
function castIllusion() {
  if (!playing || !crisis) { setToast('A little theater is more convincing once the watch is on duty.'); return; }
  const point = getAimPoint();
  if (decoy) scene.remove(decoy);
  decoy = makeDecoy(point); decoyUntil = performance.now() + 8500;
  investigatePoint.copy(point);
  const noticing=watchers.filter(w=>distanceXZ(w.position,point)<11&&canSeePoint(w,point));
  for(const w of noticing){w.userData.watchState='INVESTIGATE';w.userData.watchTarget.copy(point);w.userData.watchHoldUntil=performance.now()+5200;w.userData.watchReactUntil=performance.now()+300;}
  playSfx('illusion');
  setToast(noticing.length ? `A green afterimage appears. ${noticing.length} watcher${noticing.length===1?'':'s'} turn${noticing.length===1?'s':''} to investigate.` : 'A green afterimage shimmers outside the watchers’ sightlines.', 3.5);
}
function makeDecoy(point) {
  const g = group(scene, new THREE.Vector3(point.x, 0, point.z));
  const translucent = new THREE.MeshToonMaterial({ color: 0x55f07b, gradientMap: toonRamp, transparent: true, opacity: .58, emissive: 0x23bd42, emissiveIntensity: .6 });
  const torso = mesh(new THREE.CapsuleGeometry(.31, .56, 4, 10), translucent, g, { position: new THREE.Vector3(0, 1.1, 0) });
  const cloak = mesh(new THREE.ConeGeometry(.55, 1.4, 9, 2, true), translucent, g, { position: new THREE.Vector3(0, .82, -.08) }); cloak.rotation.x = Math.PI;
  const head = sphere(g, [0, 2, 0], [.29, .33, .28], translucent, 12);
  for (const s of [-1, 1]) tube(g, [[s * .18, 2.15, 0], [s * .22, 2.43, 0], [s * .39, 2.66, 0]], .07, M.emerald, 12);
  const ring = torus(g, [0, .07, 0], .8, .035, M.emerald); ring.rotation.x = Math.PI / 2;
  g.userData.isDecoy = true; return g;
}
function misdirectSound() {
  if (!playing) return;
  const point = getAimPoint(); sound = { point: point.clone(), until: performance.now() + 2600 };
  const hearing=watchers.filter(w=>distanceXZ(w.position,point)<14);
  for(const w of hearing){if(w.userData.watchState!=='CHASE'){w.userData.watchState='INVESTIGATE';w.userData.watchTarget.copy(point);w.userData.watchHoldUntil=performance.now()+3600;w.userData.watchReactUntil=performance.now()+280;}}
  investigatePoint.copy(point); if (guardState !== 'CHASE') { guardState = 'INVESTIGATE'; guardReactUntil = performance.now() + 280; }
  pulseAt(point, 0xffd472); setToast(hearing.length ? `The clank carries. ${hearing.length} watcher${hearing.length===1?'':'s'} move${hearing.length===1?'s':''} to investigate.` : 'The sound falls outside the watchers’ hearing.', 3.2);
  playSfx('sound');
}
function shapeshift() {
  if (!playing || distanceXZ(player.position, guard.position) > 12) { setToast('You need a closer look at the royal watch to borrow its uniform.'); return; }
  player.userData.disguised = true; disguisedUntil = performance.now() + 9000;
  player.userData.visual.traverse(o => { if (o.isMesh && o.material && o.material.color) o.material = o.geometry.type === 'CapsuleGeometry' ? M.bronze : M.gold; });
  setToast('Borrowed armor! Walk calmly; sprinting makes the disguise suspicious.', 3.2);
  pulseAt(player.position, palette.emerald);
}
function endDisguise() {
  player.userData.disguised = false;
  // Rebuild the player visual once to restore original material assignments.
  const old = player.userData.visual; player.remove(old);
  const position = player.position.clone(); player.position.set(position.x, 0, position.z);
  const oldRoot = player; player = oldRoot;
  createPlayerVisualAgain(player);
  setToast('The borrowed uniform has reached the end of its very short career.', 2.5);
}
function createPlayerVisualAgain(root) {
  // Keep all original mesh references under the visual group for lightweight disguise resets.
  const clone = root.userData.originalVisual;
  if (clone) { clone.visible = true; root.userData.visual = clone; root.add(clone); return; }
  root.userData.visual = oldVisualFactory(root);
}
function oldVisualFactory(root) {
  // Material snapshots avoid recreating geometry after each disguise.
  const v = group(root);
  const torso = mesh(new THREE.CapsuleGeometry(.35, .58, 5, 12), M.dark, v, { position: new THREE.Vector3(0, 1.15, 0) }); torso.scale.set(.86, 1.32, .72);
  const coat = mesh(new THREE.LatheGeometry([new THREE.Vector2(.39,.15),new THREE.Vector2(.48,.24),new THREE.Vector2(.43,.54),new THREE.Vector2(.31,1.13),new THREE.Vector2(.27,1.56),new THREE.Vector2(0,1.62)],20),M.green,v); outlined(coat,1.025);
  createCape(v,1.36,1.48,M.green);
  for (const s of [-1,1]) { sphere(v,[s*.42,1.68,0],[.33,.21,.37],M.gold,16,true);const arm=group(v,new THREE.Vector3(s*.49,1.4,0));sphere(arm,[0,-.29,.02],[.17,.44,.18],M.green2);sphere(arm,[0,-.64,.06],[.135,.16,.13],M.cream);root.userData['arm'+s]=arm;const leg=group(v,new THREE.Vector3(s*.17,.33,0));capsulePart(leg,[0,-.2,0],.135,.39,M.dark);sphere(leg,[0,-.42,.12],[.17,.13,.29],M.green);root.userData['leg'+s]=leg;}
  sphere(v,[0,2.15,.03],[.34,.38,.31],M.cream,20,true);sphere(v,[0,2.39,-.03],[.36,.22,.32],M.dark,16);
  for(const s of [-1,1]){sphere(v,[s*.125,2.18,.31],[.075,.038,.028],M.cream,12);sphere(v,[s*.125,2.18,.34],[.026,.032,.018],M.ink,10);tube(v,[[s*.18,2.38,-.08],[s*.22,2.65,-.1],[s*.4,2.88,-.08],[s*.52,3.1,-.05]],.095,M.gold,18);}
  sphere(v,[0,2.13,.34],[.065,.09,.08],M.cream,12);return v;
}
function pulseAt(point, color) {
  const ring = mesh(new THREE.TorusGeometry(.22, .055, 8, 24), new THREE.MeshBasicMaterial({ color, transparent: true, opacity: .95 }), scene, { position: new THREE.Vector3(point.x, .18, point.z) });
  ring.rotation.x = Math.PI / 2; ring.userData.pulse = true; ring.userData.age = 0; movingObjects.push(ring);
}
function interact() {
  if (!playing || !crisis) { setToast('Look around. The apple trouble starts near the north gate.'); return; }
  let nearest = null, best = 3.2;
  for (const o of movingObjects) {
    if (!o.userData.kind) continue;
    const d = distanceXZ(player.position, o.position);
    if (d < best && d < (o.userData.range ?? 2.25)) { best = d; nearest = o; }
  }
  if (!nearest) { setToast('Nothing in reach. Move closer to a bell or a market crate.'); return; }
  if (nearest.userData.kind === 'bridgeGate') {
    openBridgeGate(true);
  } else if (nearest.userData.kind === 'bell') {
    nearest.userData.bell.rotation.z = -.22; nearest.userData.clapper.rotation.x += .7;
    sound = { point: nearest.position.clone(), until: performance.now() + 3200 };
    investigatePoint.copy(nearest.position); if (guardState !== 'CHASE') { guardState = 'INVESTIGATE'; guardReactUntil = performance.now() + 280; }
    pulseAt(nearest.position, palette.gold); setToast(`${nearest.userData.label} rings out. The guard goes to investigate.`, 3);
    playSfx('bell');
  } else if (nearest.userData.kind === 'crate') {
    const dir = new THREE.Vector3().subVectors(nearest.position, player.position).setY(0).normalize();
    nearest.userData.target.addScaledVector(dir, 1.45);
    nearest.userData.target.x = THREE.MathUtils.clamp(nearest.userData.target.x, district.bounds.minX + 1, district.bounds.maxX - 1);
    nearest.userData.target.z = THREE.MathUtils.clamp(nearest.userData.target.z, district.bounds.minZ + 1, district.bounds.maxZ - 1);
    sound = { point: nearest.userData.target.clone(), until: performance.now() + 1800 };
    investigatePoint.copy(sound.point); if (guardState !== 'CHASE') guardState = 'INVESTIGATE';
    setToast('The crate scrapes loudly into a new spot. It can break the guard’s sightline.', 3.3);
  }
}

function openBridgeGate(showFeedback = false) {
  if (bridgeGateOpen) return;
  bridgeGateOpen = true;
  if (bridgeGate) bridgeGate.userData.open = true;
  playSfx('gate');
  if (showFeedback) setToast('The Bridge Gate swings inward. The Rainbow Bridge is waiting beyond.', 3.4);
}

function distanceXZ(a, b) { return Math.hypot(a.x - b.x, a.z - b.z); }
function canSeePoint(subject, point) {
  const delta = new THREE.Vector3(point.x - subject.position.x, 0, point.z - subject.position.z);
  const d = delta.length(); if (d > 9.2 || d < .1) return false;
  const forward = new THREE.Vector3(0, 0, 1).applyQuaternion(subject.quaternion);
  if (forward.dot(delta.normalize()) < .28) return false;
  for (const o of obstacles) {
    if(o.kind==='rect') {
      const steps=Math.ceil(d/.28);
      for(let i=1;i<steps;i++){const t=i/steps,x=subject.position.x+(point.x-subject.position.x)*t,z=subject.position.z+(point.z-subject.position.z)*t;if(x>o.minX&&x<o.maxX&&z>o.minZ&&z<o.maxZ)return false;}
      continue;
    }
    const ox=o.object?.position.x??o.x,oz=o.object?.position.z??o.z;
    if(!Number.isFinite(ox)||!Number.isFinite(oz))continue;
    const t = THREE.MathUtils.clamp(((ox - subject.position.x) * (point.x - subject.position.x) + (oz - subject.position.z) * (point.z - subject.position.z)) / Math.max(.01, d * d), .08, .92);
    const cx = subject.position.x + (point.x - subject.position.x) * t, cz = subject.position.z + (point.z - subject.position.z) * t;
    if (Math.hypot(cx - ox, cz - oz) < (o.radius??.7)) return false;
  }
  return true;
}
function canSeePlayer(observer = guard) {
  if (player.userData.disguised && lastPlayerSpeed < 4.5) return false;
  return canSeePoint(observer, player.position.clone().add(new THREE.Vector3(0, 1, 0)));
}

function updateMovement(dt) {
  // Arrow-key strafing is explicitly mirrored to match the visible left/right camera directions.
  const x = (keys.has('KeyD') ? 1 : 0) - (keys.has('KeyA') ? 1 : 0)
    + (keys.has('ArrowLeft') ? 1 : 0) - (keys.has('ArrowRight') ? 1 : 0);
  const z = (keys.has('KeyW') || keys.has('ArrowUp') ? 1 : 0) - (keys.has('KeyS') || keys.has('ArrowDown') ? 1 : 0);
  const forward = new THREE.Vector3(Math.sin(yaw), 0, Math.cos(yaw));
  const right = new THREE.Vector3(Math.cos(yaw), 0, -Math.sin(yaw));
  const input = right.multiplyScalar(x).add(forward.multiplyScalar(z));
  if (input.lengthSq() > 1) input.normalize();
  const sprint = keys.has('ShiftLeft') || keys.has('ShiftRight');
  const targetSpeed = (sprint ? 5.6 : 3.65) * (player.userData.disguised ? .76 : 1);
  const desired = input.multiplyScalar(targetSpeed);
  moveVelocity.lerp(desired, 1 - Math.exp(-(desired.lengthSq() > .01 ? 15 : 20) * dt));
  const delta = moveVelocity.clone().multiplyScalar(dt);
  const candidate = player.position.clone().add(delta);
  candidate.x = THREE.MathUtils.clamp(candidate.x, district.bounds.minX + .9, district.bounds.maxX - .9);
  candidate.z = THREE.MathUtils.clamp(candidate.z, district.bounds.minZ + .9, district.bounds.maxZ - .9);
  for (const obstacle of obstacles) {
    if (obstacle.kind === 'crate') {
      if (Math.hypot(candidate.x - obstacle.object.position.x, candidate.z - obstacle.object.position.z) < obstacle.radius + .33) {
        obstacle.object.userData.target.addScaledVector(delta.clone().setY(0).normalize(), .5);
      }
      continue;
    }
    if (obstacle.kind === 'rect') {
      if (obstacle.bridgeGate && bridgeGateOpen) continue;
      if (candidate.x > obstacle.minX - .28 && candidate.x < obstacle.maxX + .28 && candidate.z > obstacle.minZ - .28 && candidate.z < obstacle.maxZ + .28) {
        const edges=[
          {d:Math.abs(candidate.x-(obstacle.minX-.29)),axis:'x',v:obstacle.minX-.29},
          {d:Math.abs(obstacle.maxX+.29-candidate.x),axis:'x',v:obstacle.maxX+.29},
          {d:Math.abs(candidate.z-(obstacle.minZ-.29)),axis:'z',v:obstacle.minZ-.29},
          {d:Math.abs(obstacle.maxZ+.29-candidate.z),axis:'z',v:obstacle.maxZ+.29}
        ].sort((a,b)=>a.d-b.d);
        candidate[edges[0].axis]=edges[0].v;
      }
      continue;
    }
    const d = Math.hypot(candidate.x - obstacle.x, candidate.z - obstacle.z);
    if (d < obstacle.radius + .28) {
      const old = Math.hypot(player.position.x - obstacle.x, player.position.z - obstacle.z);
      if (old > .01) { const n = new THREE.Vector2(candidate.x - obstacle.x, candidate.z - obstacle.z).normalize(); candidate.x = obstacle.x + n.x * (obstacle.radius + .28); candidate.z = obstacle.z + n.y * (obstacle.radius + .28); }
      else candidate.copy(player.position);
    }
  }
  const actualDistance = distanceXZ(player.position, candidate);
  player.position.copy(candidate); lastPlayerSpeed = actualDistance / Math.max(dt, .001);
  if (input.lengthSq() > .001) {
    const heading = Math.atan2(moveVelocity.x, moveVelocity.z);
    player.rotation.y = rotateToward(player.rotation.y, heading, 14 * dt);
    walkTime += dt * (sprint ? 14 : 9) * Math.min(1, lastPlayerSpeed / targetSpeed);
  }
  animateActor(player, walkTime, lastPlayerSpeed, dt, true);
}
function rotateToward(current, target, amount) {
  let d = (target - current + Math.PI * 3) % (Math.PI * 2) - Math.PI;
  return current + THREE.MathUtils.clamp(d, -amount, amount);
}
function animateActor(actor, phase, speed, dt, isPlayer = false) {
  for (const s of [-1, 1]) {
    const leg = actor.userData['leg' + s], arm = actor.userData['arm' + s];
    if (leg) leg.rotation.x = Math.sin(phase + (s < 0 ? Math.PI : 0)) * Math.min(.62, speed * .11);
    if (arm) arm.rotation.x = -Math.sin(phase + (s < 0 ? Math.PI : 0)) * Math.min(.34, speed * .06);
  }
  const v = actor.userData.visual;
  if (v && isPlayer && !actor.userData.disguised) {
    v.rotation.z = Math.sin(phase * .5) * Math.min(.025, speed * .004);
    for (const child of v.children) if (child.userData.cape) {
      const a = child.geometry.attributes.position, base = child.userData.basePositions;
      for (let i = 0; i < a.count; i++) {
        const row = Math.floor(i / 7), col = i % 7, t = row / 7, u = col / 6 - .5;
        a.array[i * 3 + 2] = base[i * 3 + 2] + Math.sin(phase * .65 + t * 4 + u * 5) * t * .12;
      }
      a.needsUpdate = true; child.geometry.computeVertexNormals();
    }
  }
}

function updateGuard(dt, now) {
  if (!crisis || now - crisisAt < 1800) {
    for(const w of watchers) w.rotation.y=rotateToward(w.rotation.y,w.userData.postYaw,4*dt);
    return;
  }
  const states=[]; let maxSuspicion=0;
  for(const w of watchers) {
    const d=w.userData;
    let sees=canSeePlayer(w);
    if(sees) { d.watchLastSeen.copy(player.position); d.watchLastSeenUntil=now+2400; }
    if(sound&&now<sound.until&&distanceXZ(w.position,sound.point)<14&&d.watchState!=='CHASE'&&d.lastHeard!==sound) {
      d.lastHeard=sound;d.watchTarget.copy(sound.point);d.watchState='INVESTIGATE';d.watchHoldUntil=now+3200;d.watchReactUntil=now+280;
    }
    if(decoy&&now<decoyUntil&&distanceXZ(w.position,decoy.position)<11&&canSeePoint(w,decoy.position)&&d.watchState!=='CHASE'&&d.lastDecoy!==decoy) {
      d.lastDecoy=decoy;d.watchTarget.copy(decoy.position);d.watchState='INVESTIGATE';d.watchHoldUntil=now+5200;d.watchReactUntil=now+300;
    }
    if(sees&&!player.userData.disguised) {
      d.watchSuspicion=Math.min(1,d.watchSuspicion+dt*(lastPlayerSpeed>4.5?.82:.5));
      if(d.watchSuspicion>.58)d.watchState='CHASE'; else if(d.watchState!=='INVESTIGATE')d.watchState='SUSPICIOUS';
    } else if(sees&&player.userData.disguised&&lastPlayerSpeed>4.5) {
      d.watchSuspicion=Math.min(.72,d.watchSuspicion+dt*.38);d.watchTarget.copy(player.position);
      if(d.watchSuspicion>.62)d.watchState='SUSPICIOUS';
    } else if(d.watchState!=='INVESTIGATE'&&d.watchState!=='CHASE') d.watchSuspicion=Math.max(0,d.watchSuspicion-dt*.18);
    if(d.watchState==='CHASE'&&!sees&&now>d.watchLastSeenUntil&&distanceXZ(w.position,d.watchLastSeen)<.65) {
      d.watchState='SUSPICIOUS';d.watchSuspicion=Math.min(d.watchSuspicion,.38);
    }
    if(d.watchState==='CHASE'&&!player.userData.disguised&&distanceXZ(w.position,player.position)<1.1) {
      if(!caughtUntil){caughtUntil=now+1650;setToast('Caught! Five helmets all want to see your apple-search credentials.',2);playSfx('caught');speakGuide('Caught. Take a breath, then try a different trick.');}
      if(now>caughtUntil)resetAfterCaught();
      return;
    }
    let target=null,speed=0;
    if(d.watchState==='CHASE'){target=sees?player.position:d.watchLastSeen;speed=2.7;}
    else if(d.watchState==='INVESTIGATE') {
      if(now<d.watchReactUntil) { const face=Math.atan2(d.watchTarget.x-w.position.x,d.watchTarget.z-w.position.z);w.rotation.y=rotateToward(w.rotation.y,face,5*dt); }
      else if(distanceXZ(w.position,d.watchTarget)>.48){target=d.watchTarget;speed=1.8;}
      else if(now>d.watchHoldUntil)d.watchState='RETURN';
    }
    if(d.watchState==='SUSPICIOUS') {
      const face=Math.atan2(d.watchLastSeen.x-w.position.x,d.watchLastSeen.z-w.position.z);w.rotation.y=rotateToward(w.rotation.y,face,3.6*dt);
      if(d.watchSuspicion<.08)d.watchState='RETURN';
    }
    if(d.watchState==='RETURN') {
      if(distanceXZ(w.position,d.postPosition)>.22){target=d.postPosition;speed=1.35;} else d.watchState='PATROL';
    }
    if(d.watchState==='PATROL')w.rotation.y=rotateToward(w.rotation.y,d.postYaw,3.4*dt);
    if(target)moveActor(w,target,speed,dt);
    const ringColor = d.watchState === 'CHASE' ? 0xe64c3d : d.watchState === 'INVESTIGATE' || d.watchState === 'SUSPICIOUS' ? 0xe6a844 : d.watchState === 'RETURN' ? 0x93a76b : 0xd8ad5e;
    d.postRingMaterial.color.setHex(ringColor);
    d.postRingMaterial.opacity = d.watchState === 'CHASE' ? .82 + Math.sin(now * .012) * .12 : d.watchState === 'PATROL' ? .32 : .62;
    states.push(d.watchState);maxSuspicion=Math.max(maxSuspicion,d.watchSuspicion);
  }
  const priority=['CHASE','INVESTIGATE','SUSPICIOUS','RETURN','PATROL'];
  guardState=priority.find(s=>states.includes(s))??'PATROL';
  suspicion=maxSuspicion;ui.guardState.textContent=guardState;ui.awareness.style.width=`${Math.round(suspicion*100)}%`;
  if(decoy&&now>decoyUntil){scene.remove(decoy);decoy=null;setToast('The emerald afterimage fizzles out.',1.9);}
  if(sound&&now>sound.until)sound=null;
}
function moveActor(actor, target, speed, dt) {
  if (!target) return;
  const delta = target.clone().sub(actor.position).setY(0); const d = delta.length();
  if (d > .08) {
    actor.position.addScaledVector(delta.normalize(), Math.min(d, speed * dt));
    const dest = Math.atan2(target.x - actor.position.x, target.z - actor.position.z);
    actor.rotation.y = rotateToward(actor.rotation.y, dest, 5 * dt);
    actor.userData.phase = (actor.userData.phase ?? 0) + dt * (speed > 2 ? 11 : 6);
    animateActor(actor, actor.userData.phase, speed, dt, actor === player);
  }
}
function resetAfterCaught() {
  caughtUntil = 0; guardState = 'PATROL'; suspicion = .1;
  const start = district.playerStart; player.position.set(start.x, 0, start.z); moveVelocity.set(0, 0, 0);
  for(const w of watchers){const d=w.userData;w.position.copy(d.postPosition);w.rotation.y=d.postYaw;d.watchState='PATROL';d.watchSuspicion=0;d.watchTarget.copy(d.postPosition);d.watchLastSeen.copy(d.postPosition);d.watchHoldUntil=0;d.lastHeard=null;d.lastDecoy=null;}
  player.userData.disguised = false; setToast('Loki is released with a warning and a tiny lecture about apples.', 3.2);
}

function updateCamera(dt) {
  const target = player.position.clone().add(new THREE.Vector3(0, 1.32, 0));
  const orbit = new THREE.Vector3(-Math.sin(yaw) * Math.cos(pitch), Math.sin(pitch), -Math.cos(yaw) * Math.cos(pitch));
  raycaster.set(target, orbit.clone().normalize()); raycaster.far = cameraDistance;
  const hits = raycaster.intersectObjects(cameraBlockers, true);
  const hit = hits.find(h => {
    if (h.distance <= .55 || h.object.userData.noCameraCollision) return false;
    for (let o = h.object; o; o = o.parent) if (o === player || watchers.includes(o) || npcs.some(n => n.object === o)) return false;
    return true;
  });
  const targetDistance = hit ? Math.max(1.25, hit.distance - .28) : cameraDistance;
  if(fadedFoliage) for(const leaf of fadedFoliage) { leaf.material.opacity=1; leaf.material.depthWrite=true; }
  fadedFoliage=null;
  if(hit) for(let o=hit.object;o;o=o.parent) if(o.userData.foliage) {
    fadedFoliage=o.userData.foliage;
    for(const leaf of fadedFoliage) { leaf.material.opacity=.28; leaf.material.depthWrite=false; }
    break;
  }
  // Pull in decisively near cover; ease out more slowly to keep framing calm.
  const rate = targetDistance < cameraActualDistance ? 18 : 3.2;
  cameraActualDistance = THREE.MathUtils.damp(cameraActualDistance, targetDistance, rate, dt);
  const desired = target.clone().add(orbit.multiplyScalar(cameraActualDistance));
  camera.position.lerp(desired, 1 - Math.exp(-12 * dt));
  camera.lookAt(target);
}

function checkEscape(now) {
  if (!crisis || escaped) return;
  if (!bridgeStarted && bridgeGateOpen && player.position.z > 50.4) {
    bridgeStarted=true;
    ui.objective.textContent='CROSS THE RAINBOW BRIDGE';
    ui.subtitle.textContent='The Bifrost pavilion is at the far end.';
    setToast('Bridge Gate cleared. The guard stays on the courtyard side.',3);
    lastGuideKey = '';
  }
  for (const exit of district.escape) {
    if (distanceXZ(player.position, exit) > exit.radius) continue;
    if(exit.id==='west-side-passage' && !bridgeStarted) {
      bridgeStarted=true; ui.objective.textContent='CROSS THE RAINBOW BRIDGE'; ui.subtitle.textContent='The West Passage joins the bridge beyond the gate.'; lastGuideKey = '';
      setToast('The West Passage skips the gate and joins the bridge. The watch is still searching.',3.4);
      return;
    }
    if(exit.id==='bifrost-pavilion' && bridgeStarted) { finishTrouble(); return; }
  }
}
function finishTrouble() {
  if (teleporting || escaped) return;
  if (!bifrostArrival) {
    console.error('BifrostArrival is missing; keeping Loki in the playable scene.');
    setToast('The Bifrost destination is unavailable. Loki is still here.', 3.5);
    return;
  }
  closeDialogue();
  escaped = true; playing = false; setMap(false); ui.map.classList.add('hidden'); ui.hud.classList.add('hidden'); ui.crosshair.classList.add('hidden');
  playSfx('portal'); if ('speechSynthesis' in window) window.speechSynthesis.cancel();
  moveVelocity.set(0, 0, 0); teleporting=true; teleportStarted=performance.now(); montageQueued=false;
  const flash=document.createElement('div'); flash.className='bifrost-flash'; document.body.append(flash); setTimeout(()=>flash.remove(),950);
  setToast('The Bifrost takes Loki somewhere unexpected…',1.4);
}
function startMontage() {
  const cover = document.createElement('section'); cover.id = 'montage'; cover.className = 'comic-montage'; document.body.append(cover);
  const panels = [
    ['SOMETIME EARLIER…', 'A chandelier. Three guards. One extremely convincing owl.'],
    ['THE APPLE AFFAIR', 'Odin swears it was on the table. The table has no comment.'],
    ['A SMALL DETOUR', 'Loki leaves by the side passage. Technically, the gate was never opened.']
  ];
  let i = 0;
  const show = () => {
    if (i >= panels.length) { cover.remove(); showPortalEnding(); return; }
    cover.innerHTML = `<div class="comic-frame" style="--panel:${i}"><span>${panels[i][0]}</span><strong>${panels[i][1]}</strong><small>FWIP! · CLANG! · WHOOSH!</small><b>${i + 1} / 3</b></div>`;
    i++; setTimeout(show, 1450);
  };
  show();
}
function showPortalEnding() {
  // Reappear at the marked destination so the transition reads as a teleport, not a disappearance.
  if (player && bifrostArrival) {
    const arrival = bifrostArrival.getWorldPosition(new THREE.Vector3());
    player.position.set(arrival.x, arrival.y, arrival.z);
    player.rotation.y = bifrostArrival.rotation.y;
    player.visible = true; player.userData.visual.visible = true; player.userData.visual.scale.setScalar(1);
    camera.position.set(arrival.x, 7.4, arrival.z - 11.5); camera.lookAt(arrival.x, 1.25, arrival.z - 1.5);
  }
  portal = group(scene, new THREE.Vector3(3.2, 2.2, 3.5));
  const hoop = torus(portal, [0, 0, 0], 1.45, .12, M.gold); hoop.scale.y = 1.18;
  const glow = torus(portal, [0, 0, .05], 1.3, .085, M.red); glow.scale.y = 1.18;
  const flare = sphere(portal, [0, 0, -.15], [1.2, 1.6, .1], new THREE.MeshBasicMaterial({ color: 0xeab356, transparent: true, opacity: .22, side: THREE.DoubleSide }), 18);
  guest = createGuest(); guest.position.set(3.2, 0, 3.5); scene.add(guest);
  const line = document.createElement('div'); line.id = 'cameo-line'; line.className = 'cameo-line'; line.innerHTML = '<span class="label">A PORTAL OPENS</span><p>“I leave for a minute and you are already making a scene.”</p><small>THE RED-AND-BLACK VISITOR HAS ENTERED THE CHAT.</small>'; document.body.append(line);
  setTimeout(() => { line.remove(); portal?.parent?.remove(portal); if (guest) scene.remove(guest); ui.ending.textContent = 'The Bifrost is still closed. The apple is still missing. Loki is, technically, elsewhere.'; ui.end.classList.remove('hidden'); }, 4300);
}
function createGuest() {
  const g = group(null);
  const body = mesh(new THREE.CapsuleGeometry(.38, .6, 5, 12), M.redBlack, g, { position: new THREE.Vector3(0, 1.12, 0) }); outlined(body, 1.05);
  for (const s of [-1, 1]) sphere(g, [s * .12, 2.05, .28], [.13, .07, .05], M.cream, 12);
  sphere(g, [0, 2.15, 0], [.35, .36, .31], M.black, 16);
  for (const s of [-1,1]) { const lens = sphere(g,[s*.12,2.16,.28],[.12,.06,.04],M.cream,12); lens.rotation.z=s*.2; }
  const belt = torus(g, [0, .72, 0], .39, .07, M.black); belt.rotation.x = Math.PI / 2;
  for (const s of [-1, 1]) { capsulePart(g,[s*.24,.25,0],.15,.42,M.black); sphere(g,[s*.48,1.45,0],[.22,.22,.22],M.black,12); }
  const sign = textSprite('GUEST STAR', '#fff0cf', '#a33c38'); sign.scale.set(2.1, .52, 1); sign.position.set(0, 3.25, 0); g.add(sign);
  return g;
}

function drawMap(full = false) {
  const canvas = ui.mini, ctx = canvas.getContext('2d');
  const targetWidth = full ? 720 : 440, targetHeight = full ? 560 : 352;
  if (canvas.width !== targetWidth || canvas.height !== targetHeight) { canvas.width = targetWidth; canvas.height = targetHeight; }
  const w = canvas.width, h = canvas.height, pad = full ? 44 : 26;
  ctx.clearRect(0, 0, w, h);
  ctx.fillStyle = '#263b38'; ctx.fillRect(0, 0, w, h);
  const to = (p) => [pad + (p.x - district.bounds.minX) / (district.bounds.maxX - district.bounds.minX) * (w - pad * 2), h - pad - (p.z - district.bounds.minZ) / (district.bounds.maxZ - district.bounds.minZ) * (h - pad * 2)];
  // A restrained drawn map makes the space easy to read without hiding the game.
  ctx.fillStyle = '#d7c49e'; ctx.strokeStyle = '#9d8256'; ctx.lineWidth = full ? 8 : 5;
  ctx.beginPath(); ctx.roundRect(pad - 8, pad - 8, w - 2 * pad + 16, h - 2 * pad + 16, 20); ctx.fill(); ctx.stroke();
  for (const route of district.routes) {
    ctx.beginPath(); const first = to(route.points[0]); ctx.moveTo(...first);
    route.points.slice(1).forEach(p => ctx.lineTo(...to(p)));
    ctx.lineCap = 'round'; ctx.lineJoin = 'round'; ctx.lineWidth = Math.max(full ? 13 : 8, route.width / 31 * (w - pad * 2)); ctx.strokeStyle = '#efe1be'; ctx.stroke();
    ctx.lineWidth = 2; ctx.strokeStyle = '#b89a66'; ctx.stroke();
  }
  // Fountain landmark and its approach.
  const fountain = district.landmarks.find(p => p.id === 'fountain');
  const [fx, fy] = to(fountain); ctx.fillStyle = '#71d0cb'; ctx.beginPath(); ctx.arc(fx, fy, full ? 20 : 12, 0, Math.PI * 2); ctx.fill(); ctx.strokeStyle = '#f7e9cd'; ctx.lineWidth = 4; ctx.stroke();
  ctx.fillStyle = '#765b3f'; ctx.font = `800 ${full ? 14 : 10}px system-ui`; ctx.fillText('FOUNTAIN', fx + 15, fy + 4);
  for (const loc of district.landmarks) {
    if (loc.id === 'fountain') continue;
    const [x, y] = to(loc); const gate = loc.id === 'bifrost'; const home = loc.id === 'study' || loc.id === 'house';
    ctx.fillStyle = gate ? '#4ab9b5' : home ? '#637f54' : '#b07e43';
    ctx.beginPath(); ctx.arc(x, y, gate ? (full ? 13 : 9) : (full ? 8 : 5), 0, Math.PI * 2); ctx.fill();
    if (full || gate || loc.id === 'market' || loc.id === 'west-gallery' || loc.id === 'east-orchard') {
      ctx.fillStyle = '#4e4335'; ctx.font = `800 ${full ? 13 : 9}px system-ui`; ctx.fillText(loc.label, x + 9, y - 7);
    }
  }
  // Side passage is a separate, unmistakable way out.
  const ex = district.escape.find(e => e.id === 'west-side-passage'), [sx, sy] = to(ex);
  ctx.strokeStyle = '#af4c40'; ctx.lineWidth = full ? 5 : 3; ctx.setLineDash([5, 5]); ctx.beginPath(); ctx.arc(sx, sy, full ? 13 : 9, 0, Math.PI * 2); ctx.stroke(); ctx.setLineDash([]);
  if (full) { ctx.fillStyle = '#8a403a'; ctx.font = '800 13px system-ui'; ctx.fillText('SIDE PASSAGE', sx + 18, sy + 4); }
  for (const npc of npcs.filter(n => n.type === 'vendor')) {
    const [x,y]=to(npc.object.position); ctx.fillStyle='#5d874e'; ctx.strokeStyle='#f8e5b7'; ctx.lineWidth=full?3:2;
    ctx.beginPath(); ctx.arc(x,y,full?7:5,0,Math.PI*2); ctx.fill(); ctx.stroke();
  }
  watchers.forEach((w,index)=>{
    const [x,y]=to(w.position);ctx.fillStyle=w.userData.watchState==='CHASE'?'#de4437':w.userData.watchState==='INVESTIGATE'?'#e3a93e':'#ac5143';ctx.strokeStyle='#fff2d5';ctx.lineWidth=full?3:2;
    ctx.beginPath();ctx.arc(x,y,full?8:6,0,Math.PI*2);ctx.fill();ctx.stroke();
    if(full){ctx.fillStyle='#553932';ctx.font='800 10px system-ui';ctx.fillText(`W${index+1}`,x+8,y-7);}
  });
  if (player) {
    const [x, y] = to(player.position); ctx.fillStyle = '#216a43'; ctx.strokeStyle = '#fff1c8'; ctx.lineWidth = 3; ctx.beginPath(); ctx.arc(x, y, full ? 11 : 7, 0, Math.PI * 2); ctx.fill(); ctx.stroke();
    ctx.strokeStyle = '#216a43'; ctx.lineWidth = 3; ctx.beginPath(); ctx.moveTo(x, y); ctx.lineTo(x + Math.sin(player.rotation.y) * 19, y - Math.cos(player.rotation.y) * 19); ctx.stroke();
  }
  ctx.fillStyle = '#5c4a35'; ctx.font = `800 ${full ? 15 : 10}px system-ui`; ctx.fillText('N ↑', pad, 22);
}

function updateHud(now) {
  if (!playing) return;
  const talkNpc = nearestNpc();
  const nearby = movingObjects.find(o => ['bell','crate','bridgeGate'].includes(o.userData.kind) && distanceXZ(player.position, o.position) < (o.userData.range ?? 2.25));
  for (const npc of npcs) npc.object.traverse(o => { if (o.userData.interactionLabel) o.visible = !!talkNpc && talkNpc.object === npc.object; });
  for (const obj of movingObjects) obj.traverse(o => { if (o.userData.interactionLabel) o.visible = crisis && !talkNpc && distanceXZ(player.position,obj.position) < (obj.userData.range ?? 2); });
  if(nearby?.userData.kind==='bridgeGate' && !bridgeGateOpen) ui.prompt.innerHTML='<b>WALK CLOSER</b> THE GATE OPENS AUTOMATICALLY · <b>SPACE</b> OPEN NOW';
  else if(nearby?.userData.kind==='bridgeGate' && bridgeGateOpen) ui.prompt.innerHTML='BRIDGE GATE OPEN  ·  CONTINUE TO THE PAVILION';
  else if(!bridgeStarted && distanceXZ(player.position,district.escape.find(e=>e.id==='west-side-passage'))<2.5) ui.prompt.innerHTML='<b>WEST PASSAGE</b>  ·  SHORT WALKWAY TO BRIDGE';
  else if (talkNpc) ui.prompt.innerHTML = '<b>T</b> TALK TO ' + talkNpc.name.toUpperCase();
  else if (!crisis) ui.prompt.innerHTML = 'Wander the courtyards. <b>WASD</b> move · <b>RMB</b> orbit';
  else if (nearby) ui.prompt.innerHTML = '<b>SPACE</b>  ' + nearby.userData.label + '  ·  <b>Q</b> illusion  ·  <b>E</b> sound';
  else if(!bridgeStarted) ui.prompt.innerHTML = '<b>Q</b> illusion · <b>E</b> sound · <b>F</b> disguise · Bridge Gate or West Passage';
  else ui.prompt.innerHTML='<b>Q</b> illusion · <b>E</b> sound · <b>F</b> disguise · reach the pavilion';
  if (player.userData.disguised) ui.prompt.innerHTML += ` <span style="color:#ecd27a">· BORROWED FACE ${Math.ceil((disguisedUntil - now) / 1000)}s</span>`;
  updateGuide();
  if (now > toastUntil) ui.toast.classList.add('hidden');
  if (mapOpen) drawMap(true);
  else if (playing) drawMap(false);
}

function onPointerMove(e) {
  const rect = renderer.domElement.getBoundingClientRect();
  pointer.x = ((e.clientX - rect.left) / rect.width) * 2 - 1;
  pointer.y = -((e.clientY - rect.top) / rect.height) * 2 + 1;
  const box = ui.crosshair.getBoundingClientRect();
  if (ui.crosshair && !ui.crosshair.classList.contains('hidden')) { ui.crosshair.style.left = `${e.clientX}px`; ui.crosshair.style.top = `${e.clientY}px`; }
  raycaster.setFromCamera(pointer, camera);
  const point = new THREE.Vector3(); if (raycaster.ray.intersectPlane(groundPlane, point)) mouseGround.copy(point);
  if (isDragging && e.pointerId === activePointer) { yaw -= e.movementX * .0052; pitch = THREE.MathUtils.clamp(pitch + e.movementY * .0036, .12, 1.02); }
}
renderer.domElement.addEventListener('pointerdown', e => {
  if (e.button === 2) { isDragging = true; activePointer = e.pointerId; renderer.domElement.setPointerCapture(e.pointerId); }
});
renderer.domElement.addEventListener('pointerup', e => { if (e.pointerId === activePointer) { isDragging = false; activePointer = -1; } });
renderer.domElement.addEventListener('pointermove', onPointerMove);
renderer.domElement.addEventListener('contextmenu', e => e.preventDefault());
renderer.domElement.addEventListener('wheel', e => { cameraDistance = THREE.MathUtils.clamp(cameraDistance + Math.sign(e.deltaY) * .65, 4.2, 9.2); }, { passive: true });
window.addEventListener('keydown', e => {
  if (['ArrowUp','ArrowDown','ArrowLeft','ArrowRight','Space'].includes(e.code)) e.preventDefault();
  if (dialogueOpen) {
    if (e.code === 'Escape') closeDialogue();
    else if (['Digit1','Digit2','Digit3'].includes(e.code) && !e.repeat) chooseDialogue(Number(e.code.slice(-1)) - 1);
    return;
  }
  keys.add(e.code);
  if (e.repeat) return;
  if (e.code === 'KeyQ') castIllusion();
  if (e.code === 'KeyE') misdirectSound();
  if (e.code === 'KeyF') shapeshift();
  if (e.code === 'KeyT') talkToNearestNpc();
  if (e.code === 'Space') interact();
  if (e.code === 'KeyM' && playing) setMap(!mapOpen);
  if ((e.code === 'Escape' || e.code === 'KeyP') && playing) pauseGame();
  if (e.code === 'KeyR' && playing) beginGame();
});
window.addEventListener('keyup', e => keys.delete(e.code));
window.addEventListener('blur', () => keys.clear());
window.addEventListener('resize', () => { camera.aspect = innerWidth / innerHeight; camera.updateProjectionMatrix(); renderer.setSize(innerWidth, innerHeight); renderer.setPixelRatio(Math.min(devicePixelRatio, 1.7)); });

function bindUi() {
  $('new-game').onclick = beginGame;
  $('continue-game').onclick = beginGame;
  $('profile-button').onclick = () => ui.profile.classList.remove('hidden');
  $('options-button').onclick = () => ui.options.classList.remove('hidden');
  $('return-menu').onclick = () => { ui.end.classList.add('hidden'); ui.menu.classList.remove('hidden'); escaped = false; crisis = false; if (guest) scene.remove(guest); if (portal) scene.remove(portal); };
  document.querySelectorAll('[data-close]').forEach(b => b.onclick = () => b.parentElement.classList.add('hidden'));
  ui.map.addEventListener('click', e => { if (e.target === ui.map) setMap(false); });
  $('dialogue-close').onclick = closeDialogue;
  guideUi.voice.onclick = () => { voiceEnabled = !voiceEnabled; guideUi.voice.textContent = voiceEnabled ? 'VOICE ON' : 'VOICE OFF'; guideUi.voice.setAttribute('aria-pressed', String(voiceEnabled)); if (!voiceEnabled && 'speechSynthesis' in window) window.speechSynthesis.cancel(); else { lastGuideKey = ''; updateGuide(); } unlockAudio(); };
  guideUi.sfx.onclick = () => { sfxEnabled = !sfxEnabled; guideUi.sfx.textContent = sfxEnabled ? 'SFX ON' : 'SFX OFF'; guideUi.sfx.setAttribute('aria-pressed', String(sfxEnabled)); unlockAudio(); if (sfxEnabled) playSfx('illusion'); };
}

async function init() {
  bindUi();
  district = await fetch('/data/asgard-district.json').then(r => { if (!r.ok) throw new Error('Could not load the district map'); return r.json(); });
  buildWorld();
  player.userData.originalVisual = player.userData.visual;
  // Start framed on the hero and the Bifrost landmark, under the title screen.
  const startTarget = player.position.clone().add(new THREE.Vector3(0, 1.25, 0));
  camera.position.set(0, 7.3, -18.2); camera.lookAt(startTarget);
  yaw = 0; pitch = .3; cameraActualDistance = cameraDistance;
  animate();
}
function animate() {
  animationRequest = requestAnimationFrame(animate);
  const dt = Math.min(clock.getDelta(), .035), now = performance.now();
  if (playing && !paused && !mapOpen) {
    if (!dialogueOpen) updateMovement(dt); else { moveVelocity.set(0,0,0); animateActor(player, walkTime, 0, dt, true); }
    // The gate opens as Loki approaches, with Space retained as a close-range fallback.
    if (crisis && !bridgeGateOpen && distanceXZ(player.position, bridgeGate.position) < 4.4) openBridgeGate(true);
    if (bridgeGate?.userData.doors) {
      const [westLeaf, eastLeaf] = bridgeGate.userData.doors;
      westLeaf.rotation.y = THREE.MathUtils.damp(westLeaf.rotation.y, bridgeGateOpen ? -1.48 : 0, 7, dt);
      eastLeaf.rotation.y = THREE.MathUtils.damp(eastLeaf.rotation.y, bridgeGateOpen ? 1.48 : 0, 7, dt);
    }
    updateCamera(dt);
    if (!crisis && distanceXZ(player.position, district.crisisTrigger) < district.crisisTrigger.radius) showCrisis();
    updateGuard(dt, now);
    if (player.userData.disguised && now > disguisedUntil) endDisguise();
    for (const o of movingObjects) {
      if (o.userData.kind === 'crate' && o.userData.target) {
        const old = o.position.clone(); o.position.lerp(o.userData.target, 1 - Math.exp(-9 * dt));
        const prop = obstacles.find(p => p.object === o); if (prop) { prop.x = o.position.x; prop.z = o.position.z; }
        o.rotation.z = THREE.MathUtils.lerp(o.rotation.z, (o.position.x - old.x) * 1.2, 1 - Math.exp(-12 * dt));
      }
      if (o.userData.pulse) { o.userData.age += dt; o.scale.setScalar(1 + o.userData.age * 2.8); o.material.opacity = Math.max(0, 1 - o.userData.age * 1.8); if (o.userData.age > .6) { scene.remove(o); movingObjects.splice(movingObjects.indexOf(o), 1); } }
    }
    for (const g of movingObjects) if (g.userData.kind === 'bell') g.userData.bell.rotation.z = THREE.MathUtils.lerp(g.userData.bell.rotation.z, 0, 1 - Math.exp(-5 * dt));
    if (guest) { guest.rotation.y += dt * .12; guest.position.y = Math.abs(Math.sin(now * .003)) * .08; }
    checkEscape(now); updateHud(now);
  } else if (teleporting && player) {
    const t=THREE.MathUtils.clamp((now-teleportStarted)/850,0,1), eased=t*t*(3-2*t);
    player.userData.visual.scale.setScalar(Math.max(.025,1-eased*.975));
    player.rotation.y += dt*2.2;
    const target=player.position.clone().add(new THREE.Vector3(0,1.25,0));
    const orbit=new THREE.Vector3(-Math.sin(yaw)*Math.cos(pitch),Math.sin(pitch),-Math.cos(yaw)*Math.cos(pitch));
    camera.position.lerp(target.clone().add(orbit.multiplyScalar(cameraActualDistance)),1-Math.exp(-12*dt)); camera.lookAt(target);
    if(t>=1&&!montageQueued) { montageQueued=true; teleporting=false; startMontage(); }
  } else if (!playing && player) {
    // Menu orbit is intentionally gentle, leaving the environment as a living cover illustration.
    const t = now * .0001, target = player.position.clone().add(new THREE.Vector3(0, 1.3, 0));
    camera.position.lerp(new THREE.Vector3(Math.sin(t) * 10, 7.6, -12), .012); camera.lookAt(target);
    if (portal) portal.rotation.y = Math.sin(now * .001) * .08;
  }
  if (bifrostPortal) bifrostPortal.rotation.z=Math.sin(now*.0012)*.055;
  renderer.render(scene, camera);
}

init().catch(error => {
  console.error(error); ui.menu.innerHTML = `<div class="load-error"><h1>THE REALM IS STILL LOADING</h1><p>${error.message}</p><p>Start this page through <code>npm run dev</code> so the editable district map can load.</p></div>`;
});
