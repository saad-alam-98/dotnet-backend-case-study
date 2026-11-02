"""Copy the sanitized working tree to a new directory without Git history or secrets.

Run from any directory: python tools/export_portfolio.py [--output EMPTY_DIRECTORY]
Uses current files, including uncommitted edits. Never modifies the source or remote.
"""
import argparse
import pathlib
import re
import shutil
import subprocess

ROOT = pathlib.Path(__file__).resolve().parents[1]
DENIED_PARTS = {".git", ".secrets", ".vs", ".vscode", "bin", "obj", "node_modules", ".portfolio-export"}
DENIED_SUFFIXES = {".pem", ".pfx", ".p12", ".key", ".bak", ".sql", ".db", ".dll", ".exe", ".pdb"}
SIGNATURES = [
    rb"(?:AKIA|ASIA)[A-Z0-9]{16}",
    rb"AIza[0-9A-Za-z_-]{35}",
    rb"SG\.[a-zA-Z0-9_-]{20,}\.[a-zA-Z0-9_-]{20,}",
    rb"-----BEGIN (?:RSA |EC )?PRIVATE KEY-----",
]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=pathlib.Path, default=ROOT / ".portfolio-export")
    args = parser.parse_args()
    destination = args.output.resolve()
    if destination == ROOT or destination in ROOT.parents:
        parser.error("Output must not be the source directory or one of its parents.")
    if destination.exists() and (not destination.is_dir() or any(destination.iterdir())):
        parser.error("Output must be absent or empty. Existing files will not be removed.")

    # Per-command trust applies only to this explicitly selected source checkout.
    result = subprocess.run(
        ["git", "-c", f"safe.directory={ROOT.as_posix()}", "-C", str(ROOT),
         "ls-files", "-z", "--cached", "--others", "--exclude-standard"],
        check=True, capture_output=True,
    )
    files = []
    for raw in sorted(set(result.stdout.split(b"\0"))):
        if not raw:
            continue
        relative = pathlib.Path(raw.decode("utf-8"))
        name = relative.name.lower()
        if (any(p.lower() in DENIED_PARTS for p in relative.parts)
                or relative.suffix.lower() in DENIED_SUFFIXES
                or (name.startswith(".env") and name != ".env.example")
                or "service-key" in name or name == "firebase.json"):
            continue
        source = ROOT / relative
        if not source.is_file():  # A deleted tracked file must stay deleted.
            continue
        if source.is_symlink() or not source.resolve().is_relative_to(ROOT):
            raise SystemExit(f"Refusing linked or external file: {relative}")
        if source.resolve().is_relative_to(destination):
            continue
        if any(re.search(pattern, source.read_bytes()) for pattern in SIGNATURES):
            raise SystemExit(f"Credential signature detected in {relative}; nothing exported.")
        files.append(relative)

    # Preflight is complete before writing. This is a limited signature scan,
    # not a substitute for reviewing the output with a dedicated secret scanner.
    destination.mkdir(parents=True, exist_ok=True)
    for relative in files:
        target = destination / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(ROOT / relative, target)
    print(f"Exported {len(files)} files to {destination}")
    print("No Git history or local secret files copied. Review and scan before publishing.")


if __name__ == "__main__":
    main()
