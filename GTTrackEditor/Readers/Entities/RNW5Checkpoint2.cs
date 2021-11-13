using Syroot.BinaryData.Memory;

namespace GTTrackEditor.Readers.Entities;
public class RNW5Checkpoint2
{
    public static BaseRNWCheckpoint FromStream(ref SpanReader sr)
    {
        int basePos = sr.Position;

        BaseRNWCheckpoint checkpoint = new();

        float x = sr.ReadSingle();
        float y = sr.ReadSingle();
        float z = sr.ReadSingle();
        checkpoint.Left = new(x, y, z);

        x = sr.ReadSingle();
        y = sr.ReadSingle();
        z = sr.ReadSingle();
        checkpoint.Middle = new(x, y, z);

        x = sr.ReadSingle();
        y = sr.ReadSingle();
        z = sr.ReadSingle();
        checkpoint.Right = new(x, y, z);

        checkpoint.trackV = sr.ReadSingle();

        return checkpoint;
    }

    public static void ToStream(ref SpanWriter sw, BaseRNWCheckpoint check)
    {
        sw.WriteSingle(check.Left.X);
        sw.WriteSingle(check.Left.Y);
        sw.WriteSingle(check.Left.Z);

        sw.WriteSingle(check.Middle.X);
        sw.WriteSingle(check.Middle.Y);
        sw.WriteSingle(check.Middle.Z);

        sw.WriteSingle(check.Right.X);
        sw.WriteSingle(check.Right.Y);
        sw.WriteSingle(check.Right.Z);

        sw.WriteSingle(check.trackV);
    }
}
