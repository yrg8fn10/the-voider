#!/usr/bin/env bash
# The Voider build for SteamOS / Bazzite / any Linux
# If this was started with plain "sh", restart it under bash (the script uses bash features).
if [ -z "$BASH_VERSION" ]; then
    exec bash "$0" "$@"
fi
set -u
cd "$(dirname "$0")"

echo "=== THE VOIDER BUILD ==="
echo "Starting the Voider build. Nothing is being sent anywhere; this runs on your Deck only."
echo

# 1) .NET SDK (the compiler)
if ! command -v dotnet >/dev/null 2>&1; then
    echo "PROBLEM: the .NET SDK is not installed."
    echo "  FIX (SteamOS, desktop mode, Konsole):"
    echo "    sudo steamos-readonly disable"
    echo "    sudo pacman-key --init"
    echo "    sudo pacman-key --populate archlinux holo"
    echo "    sudo pacman -S dotnet-sdk-8.0"
    echo "  FIX (Bazzite):"
    echo "    sudo rpm-ostree install dotnet-sdk-8.0    (then reboot)"
    echo "    (or use distrobox and install dotnet inside the box, without touching the system)"
    echo "  Then run ./build.sh again."
    read -r -p "Press Enter to close" _
    exit 1
fi
echo "[1/4] .NET SDK found: $(dotnet --version 2>/dev/null)"

# 2) Find the Subnautica install
CANDIDATES=()
for base in "$HOME/.steam/steam" "$HOME/.local/share/Steam"; do
    CANDIDATES+=("$base/steamapps/common/Subnautica")
done
for vdf in "$HOME/.steam/steam/steamapps/libraryfolders.vdf" "$HOME/.local/share/Steam/steamapps/libraryfolders.vdf"; do
    if [ -f "$vdf" ]; then
        while IFS= read -r p; do
            [ -n "$p" ] && CANDIDATES+=("$p/steamapps/common/Subnautica")
        done < <(sed -n 's/.*"path"[[:space:]]*"\([^"]*\)".*/\1/p' "$vdf" | sed 's|\\\\|/|g')
    fi
done
if [ -n "${1:-}" ]; then CANDIDATES=("$1" "${CANDIDATES[@]}"); fi
GAME=""
for c in "${CANDIDATES[@]}"; do
    if [ -d "$c/Subnautica_Data/Managed" ]; then GAME="$c"; break; fi
done
if [ -z "$GAME" ]; then
    echo "PROBLEM: couldn't find your Subnautica install automatically."
    echo "  FIX: run this script with your game folder, e.g.:"
    echo "    ./build.sh \"$HOME/.steam/steam/steamapps/common/Subnautica\""
    read -r -p "Press Enter to close" _
    exit 1
fi
echo "[2/4] Subnautica found: $GAME"

# 3) Stage the game files the compiler needs
MANAGED="$GAME/Subnautica_Data/Managed"
mkdir -p libs
MISSING=""
WANTED="UnityEngine.dll UnityEngine.CoreModule.dll UnityEngine.PhysicsModule.dll UnityEngine.InputLegacyModule.dll UnityEngine.AudioModule.dll UnityEngine.UIModule.dll UnityEngine.TextRenderingModule.dll Assembly-CSharp.dll Assembly-CSharp-firstpass.dll"
for d in $WANTED; do
    if [ -f "$MANAGED/$d" ]; then cp -f "$MANAGED/$d" "libs/$d"; else MISSING="$MISSING $d"; fi
done
if [ -n "$MISSING" ]; then
    echo "NOTE: not found in the game folder (usually harmless):$MISSING"
fi
echo "[3/4] Game files staged into libs/"

# 4) Build
echo "[4/4] Building VoiderMod..."
if dotnet build src/VoiderMod/VoiderMod.csproj -c Release --nologo; then
    echo
    echo "SUCCESS: the mod built."
    echo "  The DLL is at: src/VoiderMod/bin/Release/net48/VoiderMod.dll"
    echo "  On the Steam Deck it goes into your game's BepInEx/plugins folder (the same DLL runs on Windows)."
else
    echo
    echo "PROBLEM: the build failed."
    echo "  FIX: copy the red error lines above and paste them to Fo."
fi
read -r -p "Press Enter to close" _
