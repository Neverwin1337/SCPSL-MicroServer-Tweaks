#!/usr/bin/env bash
# Build SCPSL-MicroServer-Tweaks on Linux / macOS.
# Mirrors build.ps1 but uses bash + dotnet CLI.

set -euo pipefail

usage() {
    cat <<'EOF'
Usage: ./build.sh -s <server-path> [-d] [-p <plugin-dir>]

  -s, --server-path   Path to your SCP:SL dedicated server install
                      (the folder that contains SCPSL_Data/Managed).
  -d, --deploy        Copy the built DLL into the global LabAPI plugin folder.
  -p, --plugin-dir    Override the LabAPI global plugin folder.
  -h, --help          Show this help.

Typical paths:
  Server:    /home/<user>/SCP Secret Laboratory Dedicated Server
  Plugins:   ~/.config/SCP Secret Laboratory/LabAPI/plugins/global
EOF
}

SERVER_PATH=""
DEPLOY=0
PLUGIN_DIR="${HOME}/.config/SCP Secret Laboratory/LabAPI/plugins/global"

while [[ $# -gt 0 ]]; do
    case "$1" in
        -s|--server-path) SERVER_PATH="$2"; shift 2 ;;
        -d|--deploy)      DEPLOY=1; shift ;;
        -p|--plugin-dir)  PLUGIN_DIR="$2"; shift 2 ;;
        -h|--help)        usage; exit 0 ;;
        *) echo "Unknown argument: $1" >&2; usage; exit 1 ;;
    esac
done

if [[ -z "$SERVER_PATH" ]]; then
    echo "Error: --server-path is required." >&2
    usage
    exit 1
fi

ROOT="$(cd "$(dirname "$0")" && pwd)"
BIN_DIR="$ROOT/Binaries"
PROJECT_FILE="$ROOT/SmallVanillaFlow/SmallVanillaFlow.csproj"

# Resolve the Managed directory.
if [[ -d "$SERVER_PATH/SCPSL_Data/Managed" ]]; then
    MANAGED_DIR="$SERVER_PATH/SCPSL_Data/Managed"
elif [[ "$(basename "$SERVER_PATH")" == "Managed" ]]; then
    MANAGED_DIR="$SERVER_PATH"
else
    echo "Error: cannot find SCPSL_Data/Managed below: $SERVER_PATH" >&2
    exit 1
fi

mkdir -p "$BIN_DIR"

copy_required() {
    local dest="$1"; shift
    for name in "$@"; do
        if [[ -f "$MANAGED_DIR/$name" ]]; then
            cp -f "$MANAGED_DIR/$name" "$BIN_DIR/$dest"
            echo "Copied $name"
            return 0
        fi
    done
    echo "Error: missing required assembly in $MANAGED_DIR: $*" >&2
    exit 1
}

copy_required "Assembly-CSharp.dll"        "Assembly-CSharp.dll"
copy_required "LabApi.dll"                "LabApi.dll" "LabAPI.dll"
copy_required "UnityEngine.CoreModule.dll" "UnityEngine.CoreModule.dll"

echo "Building SCPSL-MicroServer-Tweaks..."
dotnet build "$PROJECT_FILE" -c Release

OUTPUT_DLL="$ROOT/SmallVanillaFlow/bin/Release/net48/SmallVanillaFlow.dll"
if [[ ! -f "$OUTPUT_DLL" ]]; then
    echo "Error: build completed but output DLL was not found: $OUTPUT_DLL" >&2
    exit 1
fi

echo ""
echo "Built: $OUTPUT_DLL"

if [[ "$DEPLOY" -eq 1 ]]; then
    mkdir -p "$PLUGIN_DIR"
    cp -f "$OUTPUT_DLL" "$PLUGIN_DIR/SmallVanillaFlow.dll"
    echo "Deployed to: $PLUGIN_DIR"
fi

echo ""
echo "Restart the SCP:SL server after deployment."
