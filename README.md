# AprilTag tag36h11 for Unity

**Fork of [jp.keijiro.apriltag](https://github.com/keijiro/jp.keijiro.apriltag) with tag36h11 support**

This Unity package provides native AprilTag detection using the **tag36h11** family, making it compatible with OpenCV's ArUco detector for hand-eye calibration and marker-based tracking.

## Why tag36h11?

- **OpenCV ArUco compatibility**: tag36h11 is the recommended AprilTag family for use with OpenCV's ArUco module
- **Hand-eye calibration**: Standard choice in robotics (e.g., ROS `apriltag_ros` defaults to tag36h11)
- **Robust detection**: Optimized for real-time performance with minimal false positives

## Differences from Original

| Feature | Original (`jp.keijiro.apriltag`) | This Fork |
|---------|----------------------------------|-----------|
| Tag Family | tagStandard41h12 | **tag36h11** |
| OpenCV Compatibility | ❌ No | ✅ Yes |
| Android Plugin | Standard41h12 only | **tag36h11 + Standard41h12** |

## Installation

### Via Unity Package Manager (Git URL)

1. Open **Window → Package Manager**
2. Click **+** → **Add package from git URL**
3. Enter:
   ```
   https://github.com/iamwyh2019/apriltag-tag36h11-unity.git
   ```

### Via manifest.json

Add to your `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.iamwyh2019.apriltag.tag36h11": "https://github.com/iamwyh2019/apriltag-tag36h11-unity.git"
  }
}
```

## System Requirements

- Unity 2021.3 or later
- Platforms: Windows (x86-64), macOS (x86-64), Linux (x86-64), iOS (arm64), Android (arm64)

## Usage

### 1. Get tag36h11 Markers

Download pre-generated tag images:
- https://github.com/AprilRobotics/apriltag-imgs/tree/master/tag36h11

Or generate custom tags:
- https://chaitanyantr.github.io/apriltag.html (Select "tag36h11")

**Recommended size:** 180mm (0.18m) for calibration

### 2. Basic Detection Code

```csharp
using AprilTag;
using UnityEngine;

public class TagDetectionExample : MonoBehaviour
{
    private TagDetector detector;
    private WebCamTexture webcamTexture;
    private Color32[] buffer;

    [SerializeField] private float tagSize = 0.18f; // 180mm tag
    [SerializeField] private float cameraFOV = 60f; // Horizontal FOV in degrees

    void Start()
    {
        // Initialize webcam
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();

        // Create detector
        detector = new TagDetector(
            webcamTexture.width,
            webcamTexture.height,
            decimation: 2
        );

        buffer = new Color32[webcamTexture.width * webcamTexture.height];
    }

    void Update()
    {
        // Get camera frame
        webcamTexture.GetPixels32(buffer);

        // Detect tags
        detector.ProcessImage(buffer, cameraFOV, tagSize);

        // Process detections
        foreach (var tag in detector.DetectedTags)
        {
            Debug.Log($"Tag {tag.ID}: pos={tag.Position}, rot={tag.Rotation}");
        }
    }

    void OnDestroy()
    {
        detector?.Dispose();
    }
}
```

### 3. Quest 3 Passthrough Camera Integration

For Meta Quest 3, integrate with Passthrough Camera API:

```csharp
using AprilTag;
using PassthroughCameraSamples;

public class QuestAprilTagDetector : MonoBehaviour
{
    [SerializeField] private WebCamTextureManager webCamTextureManager;
    [SerializeField] private float tagSize = 0.18f;

    private TagDetector detector;
    private Color32[] pixelBuffer;

    void Start()
    {
        var webCamTexture = webCamTextureManager.WebCamTexture;
        int width = webCamTexture.width;
        int height = webCamTexture.height;

        // Get actual camera intrinsics
        var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(PassthroughCameraEye.Left);
        float focalLengthX = intrinsics.FocalLength.x;
        float cameraFOV = 2.0f * Mathf.Atan(width / (2.0f * focalLengthX)) * Mathf.Rad2Deg;

        detector = new TagDetector(width, height, decimation: 2);
        pixelBuffer = new Color32[width * height];
    }

    void Update()
    {
        var webCamTexture = webCamTextureManager.WebCamTexture;
        webCamTexture.GetPixels32(pixelBuffer);

        detector.ProcessImage(pixelBuffer, cameraFOV, tagSize);

        foreach (var tag in detector.DetectedTags)
        {
            // Transform from camera space to world space
            var cameraPose = PassthroughCameraUtils.GetCameraPoseInWorld(PassthroughCameraEye.Left);
            Vector3 worldPos = cameraPose.position + cameraPose.rotation * tag.Position;
            Quaternion worldRot = cameraPose.rotation * tag.Rotation;

            Debug.Log($"Tag {tag.ID} in world: {worldPos}");
        }
    }
}
```

## Hand-Eye Calibration with OpenCV

This package is designed for hand-eye calibration workflows:

**Unity (Quest) Side:**
- Detects tag36h11 markers using this package
- Sends marker poses to calibration server

**OpenCV/Python Side:**
```python
import cv2
from cv2 import aruco

# Use matching dictionary
aruco_dict = aruco.getPredefinedDictionary(aruco.DICT_APRILTAG_36h11)
parameters = aruco.DetectorParameters()
detector = aruco.ArucoDetector(aruco_dict, parameters)

# Detect markers
corners, ids, _ = detector.detectMarkers(image)

# Perform hand-eye calibration
R, t = cv2.calibrateHandEye(R_gripper2base, t_gripper2base,
                             R_target2cam, t_target2cam,
                             method=cv2.CALIB_HAND_EYE_TSAI)
```

## Building Custom Native Plugin

If you need to modify the native plugin:

### Prerequisites
- Android NDK 25+
- macOS/Linux with build tools

### Build Steps

```bash
# Clone this repository
git clone https://github.com/iamwyh2019/apriltag-tag36h11-unity.git
cd apriltag-tag36h11-unity/native-plugin/build

# Set NDK path (macOS)
export ANDROID_NDK_PATH=~/Library/Android/sdk/ndk/25.1.8937393
export ARCH=arm64-v8a
mkdir -p arm64-v8a

# Build
make -f Makefile.android

# Copy to Unity package (already in same repo)
cp arm64-v8a/libAprilTag.so ../../Plugin/Android/
```

## Credits

- **Original package**: [jp.keijiro.apriltag](https://github.com/keijiro/jp.keijiro.apriltag) by Keijiro Takahashi
- **AprilTag library**: [AprilRobotics/apriltag](https://github.com/AprilRobotics/apriltag)
- **tag36h11 fork**: Modified by iamwyh2019 for OpenCV compatibility

## License

BSD-2-Clause (same as original AprilTag library)

## Repository Structure

```
apriltag-tag36h11-unity/
├── Plugin/              # Pre-built native plugins
│   └── Android/         # libAprilTag.so (tag36h11 + tagStandard41h12)
├── Runtime/             # Unity C# scripts
├── native-plugin/       # AprilTag C library source (for rebuilding)
├── package.json         # Unity package manifest
└── README.md
```

## Related Projects

- Original package: https://github.com/keijiro/jp.keijiro.apriltag
- Original native fork: https://github.com/keijiro/apriltag
- AprilTag project: https://april.eecs.umich.edu/software/apriltag
- Pre-generated tags: https://github.com/AprilRobotics/apriltag-imgs
- ROS apriltag_ros: https://github.com/christianrauch/apriltag_ros
