using System;

[Serializable]
public class DataStore  {

    public float[] aStarStartIndex = { 2f, 0f };
    public float[] aStarTargetIndex = { 15f, 9f };
    public float[] recursiveStartIndex = { 2f, 0f };
    public float[] recursiveTargetIndex = { 15f, 9f };
    public float[] aStarCameraGimbalRotation = {-0.2f, -0.4f, -0.1f, 0.9f };
    public float[] recursiveCameraGimbalRotation = { -0.2f, -0.4f, -0.1f, 0.9f };
    public float[] aStarCameraLocalPosition = { 0f, 480f, -201f };
    public float[] recursiveCameraLocalPosition = { 0f, 480f, -201f };
}
