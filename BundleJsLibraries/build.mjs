import esbuild from "esbuild";

esbuild.build({
  entryPoints: ["chessops-entry.js"],
  bundle: true,
  minify: true,
  format: "esm",
  target: ["es2015"],
  outfile: "Output/chessops.bundle.mjs"
});

esbuild.build({
  entryPoints: ["d3-entry.js"],
  bundle: true,
  minify: true,
  format: "esm",
  target: ["es2015"],
  outfile: "Output/d3.bundle.mjs",
});