#!/usr/bin/env python3
"""Regenerates docs/tuning.md from ConsoleMud/Definitions/tuning.json.

Run:  python3 docs/generate_tuning_doc.py
Keeps the tuning reference table in sync with the data file.
"""
import json
import os

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SRC = os.path.join(ROOT, "ConsoleMud", "Definitions", "tuning.json")
OUT = os.path.join(ROOT, "docs", "tuning.md")


def group(key):
    return key.split(".", 1)[0] if "." in key else "misc"


def main():
    with open(SRC) as f:
        data = json.load(f)

    lines = [
        "# Tuning Reference",
        "",
        "Engine-wide balance constants. Edit values in "
        "[`ConsoleMud/Definitions/tuning.json`](../ConsoleMud/Definitions/tuning.json); "
        "this file is generated from it by `docs/generate_tuning_doc.py`.",
        "",
        "Per-skill numbers (dice, charges, proc chances, durations) live with each skill in "
        "`Definitions/skills.json` instead. Timing constants (tick intervals, autosave) stay in "
        "`Core/TimeEngine.cs`.",
        "",
    ]

    for g in sorted({group(k) for k in data}):
        lines.append(f"## {g}")
        lines.append("")
        lines.append("| Key | Value | Description |")
        lines.append("|---|---|---|")
        for key in sorted(k for k in data if group(k) == g):
            entry = data[key]
            val = entry.get("value")
            desc = entry.get("desc", "").replace("|", "\\|")
            lines.append(f"| `{key}` | {val} | {desc} |")
        lines.append("")

    with open(OUT, "w") as f:
        f.write("\n".join(lines))
    print(f"wrote {OUT} ({len(data)} values)")


if __name__ == "__main__":
    main()
