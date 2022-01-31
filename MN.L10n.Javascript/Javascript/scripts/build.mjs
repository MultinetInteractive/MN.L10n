import esbuild from "esbuild";
import fs from "fs";

const args = process.argv.slice(2);
const minify = args.some((arg) => arg === "--min");

async function build() {
  await esbuild.build({
    entryPoints: ["src/L10n.ts"],
    bundle: true,
    minify,
    target: ["es5"],
    outdir: "dist",
    sourcemap: true,
  });

  fs.copyFileSync("src/publicTypes.d.ts", "dist/types.d.ts");
  fs.copyFileSync("dist/L10n.js", "L10n.js");
  fs.copyFileSync("dist/L10n.js.map", "L10n.js.map");
}

build().catch(() => process.exit(1));
