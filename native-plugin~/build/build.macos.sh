#!/bin/sh
set -x

mkdir -p arm64 x86_64

make ARCH=arm64 -f Makefile.macos
make ARCH=x86_64 -f Makefile.macos

lipo -create -output AprilTag.bundle arm64/libAprilTag.so x86_64/libAprilTag.so
