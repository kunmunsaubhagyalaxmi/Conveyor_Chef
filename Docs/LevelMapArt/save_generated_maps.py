"""Copy generated chapter maps into Resources at the exact game size.

Reads a JSON array of {source, destination} objects from standard input.
"""

import json
import sys
from pathlib import Path
from PIL import Image

root = Path(__file__).resolve().parents[2]
jobs = json.load(sys.stdin)
for job in jobs:
    source = Path(job["source"])
    destination = (root / job["destination"]).resolve()
    if not str(destination).lower().startswith(str(root.resolve()).lower()):
        raise ValueError(f"Destination outside project: {destination}")
    if not source.is_file():
        raise FileNotFoundError(source)
    destination.parent.mkdir(parents=True, exist_ok=True)
    with Image.open(source) as image:
        image.convert("RGB").resize((1080, 1920), Image.Resampling.LANCZOS).save(destination, optimize=True)
    print(destination.relative_to(root))
