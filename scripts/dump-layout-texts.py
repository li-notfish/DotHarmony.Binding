import json
import sys


def walk(node):
    yield node
    for child in node.get("children", []):
        yield from walk(child)


def main():
    if len(sys.argv) != 2:
        raise SystemExit("usage: dump-layout-texts.py <layout.json>")

    with open(sys.argv[1], encoding="utf-8") as file:
        layout = json.load(file)

    for node in walk(layout):
        attributes = node.get("attributes", {})
        text = attributes.get("text") or attributes.get("originalText")
        if text:
            print(text)


if __name__ == "__main__":
    main()
