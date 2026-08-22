import { existsSync, readFileSync } from "node:fs";
import { spawn } from "node:child_process";

const secretFile = "/run/secrets/functions-keys/host.json";
const functionKeyFile = "/run/secrets/functions-keys/host.function.default";
const waitLimit = Date.now() + 90_000;

function extractKey(document) {
  const candidates = [document.masterKey, ...(Array.isArray(document.functionKeys) ? document.functionKeys : Object.values(document.functionKeys ?? {}))];
  return candidates.map(value => typeof value === "string" ? value : value?.value).find(Boolean);
}

while (!process.env.EARTHQUAKE_FUNCTION_KEY && Date.now() < waitLimit) {
  if (existsSync(functionKeyFile)) {
    const key = readFileSync(functionKeyFile, "utf8").trim();
    if (key) process.env.EARTHQUAKE_FUNCTION_KEY = key;
  }
  if (existsSync(secretFile)) {
    try {
      const key = extractKey(JSON.parse(readFileSync(secretFile, "utf8")));
      if (key) process.env.EARTHQUAKE_FUNCTION_KEY = key;
    } catch {
      // The Functions host may still be writing the secret file. Retry shortly.
    }
  }
  if (!process.env.EARTHQUAKE_FUNCTION_KEY) await new Promise(resolve => setTimeout(resolve, 1000));
}

if (!process.env.EARTHQUAKE_FUNCTION_KEY) {
  console.error(`Functions key was not found at ${functionKeyFile}.`);
  process.exit(1);
}

const child = spawn("pnpm", ["run", "dev", "--host", "0.0.0.0"], { stdio: "inherit" });
child.on("exit", code => process.exit(code ?? 1));
