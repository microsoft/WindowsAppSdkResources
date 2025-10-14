#!/bin/bash
#
# Ubuntu/Linux equivalent of dev.bat
# 
# Usage:
#   chmod +x refresh.sh
#   ./refresh.sh
#
# This script downloads the specs/ folders from specific branches of the
# Microsoft WindowsAppSDK repository into corresponding local directories.

set -e

REPO="https://github.com/microsoft/WindowsAppSDK.git"
WORKDIR="$(pwd)"
BRANCHES="release/1.7-stable release/1.8-stable release/2.0-stable"
RESOURCE_WINAPPSDK_SPECS_DIR="windowsappsdk-specs"

mkdir -p "$WORKDIR/$RESOURCE_WINAPPSDK_SPECS_DIR" && cd "$WORKDIR/$RESOURCE_WINAPPSDK_SPECS_DIR" || { echo "Failed to enter the $RESOURCE_WINAPPSDK_SPECS_DIR directory"; exit 1; }

for BRANCH in $BRANCHES; do
    FOLDER=$(basename "$BRANCH")
    echo "Processing branch $BRANCH..."

    rm -rf "$WORKDIR/$RESOURCE_WINAPPSDK_SPECS_DIR/$FOLDER"
    mkdir -p "$WORKDIR/$RESOURCE_WINAPPSDK_SPECS_DIR/$FOLDER"
    cd "$WORKDIR/$RESOURCE_WINAPPSDK_SPECS_DIR/$FOLDER"

    git init
    git remote add origin "$REPO"
    git config core.sparseCheckout true

    echo "specs/" > .git/info/sparse-checkout

    git fetch --depth 1 origin "$BRANCH"
    git checkout "$BRANCH"   # pwd at windowsappsdk-specs/1.7-stable

    rm -rf .git

    mv ./specs/* ./

done

echo "All specs folders have been downloaded."
