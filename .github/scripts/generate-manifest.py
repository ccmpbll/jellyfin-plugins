#!/usr/bin/env python3
"""Generate a Jellyfin plugin repository manifest from plugin meta.json files."""

import json
import os
import glob
import hashlib
import sys

REPO_URL = os.environ.get("GITHUB_SERVER_URL", "https://github.com") + "/" + os.environ.get("GITHUB_REPOSITORY", "ccmpbll/jellyfin-plugins")
TAG = os.environ.get("GITHUB_REF_NAME", "v0.0.0")


def get_checksum(filepath):
    with open(filepath, "rb") as f:
        return hashlib.md5(f.read()).hexdigest()


def main():
    plugins = []

    for meta_path in sorted(glob.glob("plugins/*/Jellyfin.Plugin.*/meta.json")):
        with open(meta_path) as f:
            meta = json.load(f)

        plugin_name = meta["name"]
        artifact_zip = f"artifacts/{plugin_name.replace(' ', '')}.zip"
        checksum = get_checksum(artifact_zip) if os.path.exists(artifact_zip) else ""
        download_url = f"{REPO_URL}/releases/download/{TAG}/{plugin_name.replace(' ', '')}.zip"

        versions = []
        for v in meta.get("versions", []):
            versions.append({
                "version": v["version"],
                "changelog": v.get("changelog", ""),
                "targetAbi": v.get("targetAbi", "10.11.0.0"),
                "sourceUrl": REPO_URL,
                "checksum": checksum,
                "timestamp": "",
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
