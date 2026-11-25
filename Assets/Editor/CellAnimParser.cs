using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class CellAnimData
{
    public uint RevisionDate;
    public bool UsePalette;
    public ushort SheetIndex;
    public ushort SheetW;
    public ushort SheetH;

    public List<CellAnimArrangement> Arrangements = new List<CellAnimArrangement>();
    public List<CellAnimAnimation> Animations = new List<CellAnimAnimation>();
}

public class CellAnimArrangement
{
    public List<CellAnimPart> Parts = new List<CellAnimPart>();
}

public class CellAnimPart
{
    public ushort CellX, CellY, CellW, CellH;
    public ushort TextureVarying; // rvl only
    public int PositionX, PositionY; 
    public float ScaleX, ScaleY;
    public float Angle;
    public bool FlipX, FlipY;
    public byte Opacity;
}

public class CellAnimAnimation
{
    public string Name;
    public List<CellAnimKey> Keys = new List<CellAnimKey>();
}

public class CellAnimKey
{
    public ushort ArrangementIndex;
    public ushort HoldFrames;
    public int PositionX, PositionY;
    public float ScaleX, ScaleY;
    public float Angle;
    public byte Opacity;
}

// credit to conhlee / toast idk how else this would be possible (Im Stupid)
// also genox / sinizano for making me go less crazy 

public static class CellAnimParser
{
    public static CellAnimData Parse(string filePath, string[] animNames)
    {
        CellAnimData data = new CellAnimData();

        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (BinaryReader reader = new BinaryReader(fs))
        {
            byte[] magicBytes = reader.ReadBytes(4);
            
            uint magicBE = (uint)((magicBytes[0] << 24) | (magicBytes[1] << 16) | (magicBytes[2] << 8) | magicBytes[3]);
            uint magicLE = (uint)((magicBytes[3] << 24) | (magicBytes[2] << 16) | (magicBytes[1] << 8) | magicBytes[0]);

            if (magicBE == 20100312)
            {
                ParseRVL(reader, data, animNames);
            }
            else if (magicLE == 20131007)
            {
                ParseCTR(reader, data);
            }
            else
            {
                UnityEngine.Debug.LogError("invalid binary (not rvl or ctr)");
            }
        }
        return data;
    }

    // !!!!!!!!!!!!!!!!!!!!!!!!!!! rvl (brcad) parser 

    private static void ParseRVL(BinaryReader reader, CellAnimData data, string[] animNames)
    {
        data.UsePalette = reader.ReadByte() != 0;
        reader.ReadBytes(3); 
        data.SheetIndex = ReadUInt16BE(reader);
        reader.ReadBytes(2); 
        data.SheetW = ReadUInt16BE(reader);
        data.SheetH = ReadUInt16BE(reader);
        
        ushort arrangementCount = ReadUInt16BE(reader);
        reader.ReadBytes(2); 

        for (int i = 0; i < arrangementCount; i++)
        {
            CellAnimArrangement arrangement = new CellAnimArrangement();
            ushort partsCount = ReadUInt16BE(reader);
            reader.ReadBytes(2); 

            for (int j = 0; j < partsCount; j++)
            {
                CellAnimPart part = new CellAnimPart();
                part.CellX = ReadUInt16BE(reader);
                part.CellY = ReadUInt16BE(reader);
                part.CellW = ReadUInt16BE(reader);
                part.CellH = ReadUInt16BE(reader);
                part.TextureVarying = ReadUInt16BE(reader);
                reader.ReadBytes(2); 

                part.PositionX = (int)ReadInt16BE(reader) - 512;
                part.PositionY = (int)ReadInt16BE(reader) - 512;
                part.ScaleX = ReadSingleBE(reader);
                part.ScaleY = ReadSingleBE(reader);
                part.Angle = ReadSingleBE(reader);

                part.FlipX = reader.ReadByte() != 0;
                part.FlipY = reader.ReadByte() != 0;
                part.Opacity = reader.ReadByte();
                reader.ReadBytes(1); 

                arrangement.Parts.Add(part);
            }
            data.Arrangements.Add(arrangement);
        }

        ushort animationCount = ReadUInt16BE(reader);
        reader.ReadBytes(2); 

        for (int i = 0; i < animationCount; i++)
        {
            CellAnimAnimation anim = new CellAnimAnimation();
            ushort keyCount = ReadUInt16BE(reader);
            reader.ReadBytes(2); 

            if (animNames != null && i < animNames.Length && !string.IsNullOrWhiteSpace(animNames[i]))
                anim.Name = animNames[i];
            else
                anim.Name = $"Main_Anim_{i}";

            for (int j = 0; j < keyCount; j++)
            {
                CellAnimKey key = new CellAnimKey();
                key.ArrangementIndex = ReadUInt16BE(reader);
                key.HoldFrames = ReadUInt16BE(reader);

                key.PositionX = (int)ReadInt16BE(reader);
                key.PositionY = (int)ReadInt16BE(reader);
                key.ScaleX = ReadSingleBE(reader);
                key.ScaleY = ReadSingleBE(reader);
                key.Angle = ReadSingleBE(reader);

                key.Opacity = reader.ReadByte();
                reader.ReadBytes(3); 

                anim.Keys.Add(key);
            }
            data.Animations.Add(anim);
        }
    }

    // !!!!!!!!!!!!!!!!! ctr (bccad) parser

    private static void ParseCTR(BinaryReader reader, CellAnimData data)
    {
        data.SheetW = reader.ReadUInt16();
        data.SheetH = reader.ReadUInt16();
        ushort arrangementCount = reader.ReadUInt16();
        reader.ReadUInt16(); // _pad16

        for (int i = 0; i < arrangementCount; i++)
        {
            CellAnimArrangement arrangement = new CellAnimArrangement();
            ushort partsCount = reader.ReadUInt16();
            reader.ReadUInt16(); // _pad16

            for (int j = 0; j < partsCount; j++)
            {
                CellAnimPart part = new CellAnimPart();
                part.CellX = reader.ReadUInt16();
                part.CellY = reader.ReadUInt16();
                part.CellW = reader.ReadUInt16();
                part.CellH = reader.ReadUInt16();

                part.PositionX = (int)reader.ReadInt16() - 512;
                part.PositionY = (int)reader.ReadInt16() - 512;
                part.ScaleX = reader.ReadSingle();
                part.ScaleY = reader.ReadSingle();
                part.Angle = reader.ReadSingle();

                part.FlipX = reader.ReadByte() != 0;
                part.FlipY = reader.ReadByte() != 0;

                reader.ReadBytes(3); // foreColor
                reader.ReadBytes(3); // backColor

                part.Opacity = reader.ReadByte();

                reader.ReadBytes(12); // _unknown0
                reader.ReadByte(); // id
                reader.ReadByte(); // emitterId
                reader.ReadByte(); // _pad8

                reader.ReadSingle(); // depthTL
                reader.ReadSingle(); // depthBL
                reader.ReadSingle(); // depthTR
                reader.ReadSingle(); // depthBR

                arrangement.Parts.Add(part);
            }
            data.Arrangements.Add(arrangement);
        }

        ushort animationCount = reader.ReadUInt16();
        reader.ReadUInt16(); // _pad16

        for (int i = 0; i < animationCount; i++)
        {
            CellAnimAnimation anim = new CellAnimAnimation();
            
            // pascal case anim names from string
            long stringStartPos = reader.BaseStream.Position;
            byte strLen = reader.ReadByte();
            byte[] strBytes = reader.ReadBytes(strLen);
            anim.Name = Encoding.ASCII.GetString(strBytes);

            int chunkAligned = (1 + strLen + 1 + 3) & ~3;
            reader.BaseStream.Position = stringStartPos + chunkAligned;

            uint isInterpolated = reader.ReadUInt32();
            ushort keyCount = reader.ReadUInt16();
            reader.ReadUInt16(); // _pad16

            for (int j = 0; j < keyCount; j++)
            {
                CellAnimKey key = new CellAnimKey();
                key.ArrangementIndex = reader.ReadUInt16();
                key.HoldFrames = reader.ReadUInt16();

                key.PositionX = (int)reader.ReadInt16();
                key.PositionY = (int)reader.ReadInt16();
                reader.ReadSingle(); // positionZ
                key.ScaleX = reader.ReadSingle();
                key.ScaleY = reader.ReadSingle();
                key.Angle = reader.ReadSingle();

                reader.ReadBytes(3); // foreColor
                reader.ReadBytes(3); // backColor

                key.Opacity = reader.ReadByte();
                reader.ReadByte(); // _pad8

                anim.Keys.Add(key);
            }
            data.Animations.Add(anim);
        }
    }

    // rvl helper methods

    private static short ReadInt16BE(BinaryReader reader)
    {
        byte[] b = reader.ReadBytes(2);
        Array.Reverse(b);
        return BitConverter.ToInt16(b, 0);
    }

    private static ushort ReadUInt16BE(BinaryReader reader)
    {
        byte[] b = reader.ReadBytes(2);
        Array.Reverse(b);
        return BitConverter.ToUInt16(b, 0);
    }

    private static float ReadSingleBE(BinaryReader reader)
    {
        byte[] b = reader.ReadBytes(4);
        Array.Reverse(b);
        return BitConverter.ToSingle(b, 0);
    }
}