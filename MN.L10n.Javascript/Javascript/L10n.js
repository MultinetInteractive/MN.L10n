(function(){function l(){var n=null;return typeof window!="undefined"?n=window:typeof global!="undefined"&&(n=global),n}function f(){var n=l();if(n==null)return null;var e=n.l10n;return typeof e=="undefined"?null:e}function u(n,e){typeof e=="undefined"&&(e={});var t=f();if(t==null)return n;var r=t.Phrases[n],i;return typeof e.__count!="undefined"&&(i=t.ruleEvaluator(e.__count).toString()),typeof r!="undefined"&&(typeof i!="undefined"&&typeof r.r[i]!="undefined"?n=r.r[i]:n=r.r["0"]),n}function d(n,e){var r,i;if(typeof e=="undefined")return n;for(var t in e)e.hasOwnProperty(t)&&(n=n.split("$"+t+"$").join((i=(r=e[t])==null?void 0:r.toString())!=null?i:""));return n}function o(n,e){return d(u(n,e),e)}typeof window!="undefined"&&(window._s=o);typeof global!="undefined"&&(global._s=o);})();
//# sourceMappingURL=L10n.js.map
