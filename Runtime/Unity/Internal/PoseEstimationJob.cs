using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace AprilTag {

//
// Job struct that wraps AprilTag pose estimator
//
struct PoseEstimationJob : Unity.Jobs.IJobParallelFor
{
    // Input data struct that simply wraps pointers to tag detection data
    public struct Input
    {
        unsafe Interop.Detection* p;

        unsafe public Input(ref Interop.Detection r)
          => p = (Interop.Detection*)Interop.Util.AsPointer(ref r);

        unsafe public ref Interop.Detection Ref
          => ref Interop.Util.AsRef<Interop.Detection>(p);
    }

    // I/O
    [ReadOnly] NativeArray<Input> _input;
    [WriteOnly] NativeArray<TagPose> _output;

    // Camera parameters (full pinhole intrinsics, in DETECTOR-image pixel space:
    // top-left origin, matching the corners produced by the AprilTag detector).
    double _tagSize;
    double _focalLengthX;
    double _focalLengthY;
    double2 _focalCenter;

    // Constructor — takes separate fx/fy and the true principal point (cx, cy).
    // Callers that only know the field of view can derive fx=fy and cx,cy=image center
    // before constructing this (see TagDetector.ProcessImage(image, fov, tagSize)).
    public PoseEstimationJob
      (NativeArray<Input> input, NativeArray<TagPose> output,
       double fx, double fy, double cx, double cy, float tagSize)
    {
        _input = input;
        _output = output;
        _tagSize = tagSize;
        _focalLengthX = fx;
        _focalLengthY = fy;
        _focalCenter = math.double2(cx, cy);
    }

    // Job execution method
    public void Execute(int i)
    {
        var info = new Interop.DetectionInfo(ref _input[i].Ref, _tagSize,
           _focalLengthX, _focalLengthY, _focalCenter.x, _focalCenter.y);

        using var pose = new Interop.Pose(ref info);

        var pos = pose.t.AsFloat3() * math.float3(1, -1, 1);

        // Apply XOR transformation to rotation matrix BEFORE converting to quaternion
        // This correctly transforms from OpenCV (Y-down) to Unity (Y-up) coordinates
        var R = pose.R.AsFloat3x3();

        // Column 0: negate row 1 only (row index 1)
        R.c0[1] = -R.c0[1];

        // Column 1: negate rows 0 and 2 (NOT row 1, because [1,1] shouldn't be negated)
        R.c1[0] = -R.c1[0];
        R.c1[2] = -R.c1[2];

        // Column 2: negate row 1 only
        R.c2[1] = -R.c2[1];

        var rot = math.quaternion(R);

        _output[i] = new TagPose(_input[i].Ref.ID, pos, rot);
    }
}

} // namespace AprilTag
