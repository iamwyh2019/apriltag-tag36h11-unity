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

    // Camera parameters
    double _tagSize;
    double _focalLength;
    double2 _focalCenter;

    // Constructor
    public PoseEstimationJob
      (NativeArray<Input> input, NativeArray<TagPose> output,
       int width, int height, float fov, float tagSize)
    {
        _input = input;
        _output = output;
        _tagSize = tagSize;
        _focalLength = height / 2 / math.tan(fov / 2);
        _focalCenter = math.double2(width, height) / 2;
    }

    // Job execution method
    public void Execute(int i)
    {
        var info = new Interop.DetectionInfo(ref _input[i].Ref, _tagSize,
           _focalLength, _focalLength, _focalCenter.x, _focalCenter.y);

        using var pose = new Interop.Pose(ref info);

        var pos = pose.t.AsFloat3() * math.float3(1, -1, 1);

        // Apply XOR transformation to rotation matrix BEFORE converting to quaternion
        // This correctly transforms from OpenCV (Y-down) to Unity (Y-up) coordinates
        var R = pose.R.AsFloat3x3();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                // XOR: negate if exactly one index is 1 (row 1 XOR column 1)
                if ((i == 1) != (j == 1))
                {
                    R[i, j] = -R[i, j];
                }
            }
        }

        var rot = math.quaternion(R);

        _output[i] = new TagPose(_input[i].Ref.ID, pos, rot);
    }
}

} // namespace AprilTag
