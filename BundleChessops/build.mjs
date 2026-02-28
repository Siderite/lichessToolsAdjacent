import esbuild from "esbuild";

esbuild.build({
  entryPoints: ["chessops-entry.js"],
  bundle: true,
  minify: true,
  format: "esm",
  target: ["es2015"],
  outfile: "Output/chessops.bundle.mjs"
});
