#!/usr/bin/env python3
"""Send a Python file to the running Blender MCP bridge for execution.

Protocol mirrors blmcp/tools_helpers/connection.py: JSON {"type":"execute",
"code":..., "strict_json":bool} + NUL, read response until NUL.

Usage: blender_exec.py <script.py> [strict_json]
The target script can assign a JSON-serialisable dict to `result`.
"""
import json
import socket
import sys

HOST, PORT = "localhost", 9876
TIMEOUT = 300.0
BUF = 65536


def main() -> int:
    path = sys.argv[1]
    strict = len(sys.argv) > 2 and sys.argv[2].lower() in ("1", "true", "yes")
    with open(path, "r", encoding="utf-8") as fh:
        code = fh.read()
    req = json.dumps({"type": "execute", "code": code, "strict_json": strict}) + "\0"
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
        sock.settimeout(TIMEOUT)
        sock.connect((HOST, PORT))
        sock.sendall(req.encode("utf-8"))
        buf = bytearray()
        while True:
            chunk = sock.recv(BUF)
            if not chunk:
                break
            buf.extend(chunk)
            if b"\0" in buf:
                break
    line = buf.split(b"\0")[0].decode("utf-8")
    resp = json.loads(line)
    print(json.dumps(resp, indent=2))
    return 0 if resp.get("status") == "ok" else 1


if __name__ == "__main__":
    sys.exit(main())
