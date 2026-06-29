#!/usr/bin/env python3
"""Generate a Jellyfin plugin repository manifest from plugin meta.json files."""

import json
import os
import glob
from datetime import datetime, timezone


REPO_URL = os.environ.get("GITHUB_SERVER_URL", "https://github.com") + "/" + os.environ.get("GITHUB_REPOSITORY", "ccmpbll/jellyfin-plugins")
TAG = os.environ.get("GITHUB_REF_NAME", "v0.0.0")
TIMESTAMP = datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")


def main():
    plugins = []

    for meta_path in sorted(glob.glob("plugins/*/Jellyfin.Plugin.*/meta.json")):
        with open(meta_path) as f:
            meta = json.load(f)

        plugin_name = meta["name"].replace(" ", "")
        download_url = f"{REPO_URL}/releases/download/{TAG}/{plugin_name}.zip"

        md5_file = f"artifacts/{plugin_name}.md5"
        checksum = ""
        if os.path.exists(md5_file):
            with open(md5_file) as f:
                checksum = f.read().strip()

        versions = []
        for v in meta.get("versions", []):
            versions.append({
                "version": v["version"],
                "changelog": v.get("changelog", ""),
                "targetAbi": v.get("targetAbi", "10.11.0.0"),
                "sourceUrl": download_url,
                "checksum": checksum,
                "timestamp": TIMESTAMP,
            })

        plugins.append({
            "guid": meta["guid"],
            "name": meta["name"],
            "description": meta.get("description", ""),
            "overview": meta.get("overview", ""),
            "owner": meta.get("owner", ""),
            "category": meta.get("category", "General"),
            "versions": versions,
        })

    print(json.dumps(plugins, indent=2))


if __name__ == "__main__":
    main()
