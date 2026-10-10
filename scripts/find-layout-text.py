import json
import re
import sys


def walk(node):
    yield node
    for child in node.get("children", []):
        yield from walk(child)


def center(bounds):
    match = re.fullmatch(r"\[(-?\d+),(-?\d+)\]\[(-?\d+),(-?\d+)\]", bounds)
    if not match:
        raise ValueError(f"invalid bounds: {bounds}")
    x1, y1, x2, y2 = map(int, match.groups())
    return (x1 + x2) // 2, (y1 + y2) // 2


def main():
    if len(sys.argv) != 3:
        raise SystemExit("usage: find-layout-text.py <layout.json> <text>")

    with open(sys.argv[1], encoding="utf-8") as file:
        layout = json.load(file)

    wanted = sys.argv[2]
    for node in walk(layout):
        attributes = node.get("attributes", {})
        text = attributes.get("text", "")
        original_text = attributes.get("originalText", "")
        if wanted in text or wanted in original_text:
            x, y = center(attributes["bounds"])
            print(f"{x} {y}")
            return

    raise SystemExit(1)


if __name__ == "__main__":
    main()
