export function getGlobal(): any {
  var target: any = null;
  if (typeof window !== "undefined") {
    target = window;
  } else if (typeof global !== "undefined") {
    target = global;
  }

  return target;
}
