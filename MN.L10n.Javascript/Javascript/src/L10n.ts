import { _s } from "./_s";

if ("undefined" !== typeof window) {
  (window as any)._s = _s;
}

if ("undefined" !== typeof global) {
  (global as any)._s = _s;
}
