export function replaceKeywords(
  phrase: string,
  args?: {
    [key: string]: string | number;
  }
) {
  if ("undefined" === typeof args) {
    return phrase;
  }

  for (var p in args) {
    if (args.hasOwnProperty(p)) {
      phrase = phrase.split("$" + p + "$").join(args[p]?.toString() ?? "");
    }
  }
  return phrase;
}
