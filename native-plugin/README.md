This is a personal fork of [AprilTag] used to build binaries for [the AprilTag Unity plugin project].
Please check [the original repository][AprilTag] for the details of AprilTag.

[AprilTag]: https://github.com/AprilRobotics/apriltag
[the AprilTag Unity plugin project]: https://github.com/keijiro/jp.keijiro.apriltag

How to build the binaries
-------------------------

### Windows

It requires WSL (Windows Subsystem for Linux) and `mingw-w64`.

On the WSL console:

```
$ sudo apt install mingw-w64
$ cd build && make -f Makefile.windows
```

### macOS

```
$ cd build && ./build.macos.sh
```

### Linux

```
$ cd build && make -f Makefile.linux
```

### iOS

```
$ cd build && make -f Makefile.ios
```

### Android

It requires a Linux system (WSL might be okay) and NDK to build the binary.

```
$ cd build && ANDROID_NDK_PATH=/path/to/ndk make -f Makefile.android
```
