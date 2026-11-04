#!/usr/bin/env bash
# The Voider build for SteamOS / Bazzite / Linux / Windows (via Git Bash or WSL)
# Builds the mod WITHOUT a Subnautica installation: every dependency comes from
# the Subnautica modding NuGet feed. The output DLL drops into BepInEx/plugins.
# If this was started with plain "sh", restart it under bash (the script uses bash features).
if [ -z "$BASH_VERSION" ]; then
    exec bash "$0" "$@"
fi
set -eu
cd "$(dirname "$0")"

echo "=== THE VOIDER BUILD ==="
echo "Nothing is sent anywhere; this runs on your machine only."
echo

# 1) .NET SDK
if ! command -v dotnet >/dev/null 2>&1; then
    echo "PROBLEM: the .NET SDK is not installed."
    echo "  FIX (SteamOS, desktop mode, Konsole):"
    echo "    sudo steamos-readonly disable"
    echo "    sudo pacman-key --init"
    echo "    sudo pacman-key --populate archlinux holo"
    echo "    sudo pacman -S dotnet-sdk-8.0"
    echo "  FIX (Bazzite):"
    echo "    sudo rpm-ostree install dotnet-sdk-8.0    (then reboot)"
    echo "  Then run ./build.sh again."
    exit 1
fi
echo "[1/3] .NET SDK found: $(dotnet --version 2>/dev/null)"

# 2) Build VehicleFramework from source (pinned commit) into libs/
VF_COMMIT="f4d5c58041fbbdd1e507d48b19607e8acb7b0e1d"
if [ ! -f libs/VehicleFramework.dll ]; then
    echo "[2/3] Building VehicleFramework (pinned commit $VF_COMMIT)..."
    if [ ! -d ../VehicleFramework-src ]; then
        git clone --quiet https://github.com/NeisesMike/VehicleFramework.git ../VehicleFramework-src
        git -C ../VehicleFramework-src checkout --quiet "$VF_COMMIT"
    fi
    cat > ../VehicleFramework-src/NuGet.config <<'XML'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="bepinex.dev" value="https://nuget.bepinex.dev/v3/index.json" />
  </packageSources>
</configuration>
XML
    dotnet build ../VehicleFramework-src/VehicleFramework/VehicleFramework/VehicleFramework.csproj -c Release --nologo
    mkdir -p libs
    cp ../VehicleFramework-src/VehicleFramework/VehicleFramework/bin/Release/net472/VehicleFramework.dll libs/
else
    echo "[2/3] VehicleFramework.dll already built."
fi

# 3) Build the mod
echo "[3/3] Building VoiderMod..."
dotnet build src/VoiderMod/VoiderMod.csproj -c Release --nologo
echo
echo "SUCCESS: the mod built."
echo "  The DLL is at: src/VoiderMod/bin/Release/net472/VoiderMod.dll"
echo "  Install it (see INSTALL.md) into your game's BepInEx/plugins folder."
