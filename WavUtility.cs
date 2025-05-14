// Decompiled with JetBrains decompiler
// Type: WavUtility
// Assembly: TrashItems, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 8DDB509A-2334-47C1-85BC-EE7C4294997C
// Assembly location: C:\Users\h2so4\Downloads\Trash Items-441-1-2-8-1705314319\TrashItems.dll

using System;
using System.IO;
using System.Text;
using UnityEngine;

#nullable disable
public class WavUtility
{
  private const int BlockSize_16Bit = 2;

  public static AudioClip ToAudioClip(string filePath)
  {
    if (filePath.StartsWith(Application.persistentDataPath) || filePath.StartsWith(Application.dataPath))
      return WavUtility.ToAudioClip(File.ReadAllBytes(filePath));
    Debug.LogWarning((object) "This only supports files that are stored using Unity's Application data path. \nTo load bundled resources use 'Resources.Load(\"filename\") typeof(AudioClip)' method. \nhttps://docs.unity3d.com/ScriptReference/Resources.Load.html");
    return (AudioClip) null;
  }

  public static AudioClip ToAudioClip(byte[] fileBytes, int offsetSamples = 0, string name = "wav")
  {
    int int32_1 = BitConverter.ToInt32(fileBytes, 16 /*0x10*/);
    WavUtility.FormatCode(BitConverter.ToUInt16(fileBytes, 20));
    ushort uint16_1 = BitConverter.ToUInt16(fileBytes, 22);
    int int32_2 = BitConverter.ToInt32(fileBytes, 24);
    ushort uint16_2 = BitConverter.ToUInt16(fileBytes, 34);
    int num = 20 + int32_1 + 4;
    int int32_3 = BitConverter.ToInt32(fileBytes, num);
    float[] audioClipData;
    switch (uint16_2)
    {
      case 8:
        audioClipData = WavUtility.Convert8BitByteArrayToAudioClipData(fileBytes, num, int32_3);
        break;
      case 16 /*0x10*/:
        audioClipData = WavUtility.Convert16BitByteArrayToAudioClipData(fileBytes, num, int32_3);
        break;
      case 24:
        audioClipData = WavUtility.Convert24BitByteArrayToAudioClipData(fileBytes, num, int32_3);
        break;
      case 32 /*0x20*/:
        audioClipData = WavUtility.Convert32BitByteArrayToAudioClipData(fileBytes, num, int32_3);
        break;
      default:
        throw new Exception(uint16_2.ToString() + " bit depth is not supported.");
    }
    AudioClip audioClip = AudioClip.Create(name, audioClipData.Length, (int) uint16_1, int32_2, false);
    audioClip.SetData(audioClipData, 0);
    return audioClip;
  }

  private static float[] Convert8BitByteArrayToAudioClipData(
    byte[] source,
    int headerOffset,
    int dataSize)
  {
    int int32 = BitConverter.ToInt32(source, headerOffset);
    headerOffset += 4;
    float[] audioClipData = new float[int32];
    sbyte maxValue = sbyte.MaxValue;
    for (int index = 0; index < int32; ++index)
      audioClipData[index] = (float) source[index] / (float) maxValue;
    return audioClipData;
  }

  private static float[] Convert16BitByteArrayToAudioClipData(
    byte[] source,
    int headerOffset,
    int dataSize)
  {
    int int32 = BitConverter.ToInt32(source, headerOffset);
    headerOffset += 4;
    int num1 = 2;
    int num2 = num1;
    int length = int32 / num2;
    float[] audioClipData = new float[length];
    short maxValue = short.MaxValue;
    for (int index = 0; index < length; ++index)
    {
      int startIndex = index * num1 + headerOffset;
      audioClipData[index] = (float) BitConverter.ToInt16(source, startIndex) / (float) maxValue;
    }
    return audioClipData;
  }

  private static float[] Convert24BitByteArrayToAudioClipData(
    byte[] source,
    int headerOffset,
    int dataSize)
  {
    int int32 = BitConverter.ToInt32(source, headerOffset);
    headerOffset += 4;
    int count = 3;
    int num = count;
    int length = int32 / num;
    int maxValue = int.MaxValue;
    float[] audioClipData = new float[length];
    byte[] dst = new byte[4];
    for (int index = 0; index < length; ++index)
    {
      int srcOffset = index * count + headerOffset;
      Buffer.BlockCopy((Array) source, srcOffset, (Array) dst, 1, count);
      audioClipData[index] = (float) BitConverter.ToInt32(dst, 0) / (float) maxValue;
    }
    return audioClipData;
  }

  private static float[] Convert32BitByteArrayToAudioClipData(
    byte[] source,
    int headerOffset,
    int dataSize)
  {
    int int32 = BitConverter.ToInt32(source, headerOffset);
    headerOffset += 4;
    int num1 = 4;
    int num2 = num1;
    int length = int32 / num2;
    int maxValue = int.MaxValue;
    float[] audioClipData = new float[length];
    for (int index = 0; index < length; ++index)
    {
      int startIndex = index * num1 + headerOffset;
      audioClipData[index] = (float) BitConverter.ToInt32(source, startIndex) / (float) maxValue;
    }
    return audioClipData;
  }

  public static byte[] FromAudioClip(AudioClip audioClip)
  {
    return WavUtility.FromAudioClip(audioClip, out string _, false);
  }

  public static byte[] FromAudioClip(
    AudioClip audioClip,
    out string filepath,
    bool saveAsFile = true,
    string dirname = "recordings")
  {
    MemoryStream stream = new MemoryStream();
    ushort bitDepth = 16 /*0x10*/;
    int fileSize = audioClip.samples * audioClip.channels * 2 + 44;
    WavUtility.WriteFileHeader(ref stream, fileSize);
    WavUtility.WriteFileFormat(ref stream, audioClip.channels, audioClip.frequency, bitDepth);
    WavUtility.WriteFileData(ref stream, audioClip, bitDepth);
    byte[] array = stream.ToArray();
    if (saveAsFile)
    {
      filepath = $"{Application.persistentDataPath}/{dirname}/{DateTime.UtcNow.ToString("yyMMdd-HHmmss-fff")}.{"wav"}";
      Directory.CreateDirectory(Path.GetDirectoryName(filepath));
      File.WriteAllBytes(filepath, array);
    }
    else
      filepath = (string) null;
    stream.Dispose();
    return array;
  }

  private static int WriteFileHeader(ref MemoryStream stream, int fileSize)
  {
    byte[] bytes1 = Encoding.ASCII.GetBytes("RIFF");
    int num1 = 0 + WavUtility.WriteBytesToMemoryStream(ref stream, bytes1, "ID");
    int num2 = fileSize - 8;
    int memoryStream1 = WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(num2), "CHUNK_SIZE");
    int num3 = num1 + memoryStream1;
    byte[] bytes2 = Encoding.ASCII.GetBytes("WAVE");
    int memoryStream2 = WavUtility.WriteBytesToMemoryStream(ref stream, bytes2, "FORMAT");
    return num3 + memoryStream2;
  }

  private static int WriteFileFormat(
    ref MemoryStream stream,
    int channels,
    int sampleRate,
    ushort bitDepth)
  {
    byte[] bytes = Encoding.ASCII.GetBytes("fmt ");
    int num1 = 0 + WavUtility.WriteBytesToMemoryStream(ref stream, bytes, "FMT_ID");
    int num2 = 16 /*0x10*/;
    int memoryStream1 = WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(num2), "SUBCHUNK_SIZE");
    int num3 = num1 + memoryStream1;
    ushort num4 = 1;
    int memoryStream2 = WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(num4), "AUDIO_FORMAT");
    int num5 = num3 + memoryStream2;
    ushort uint16_1 = Convert.ToUInt16(channels);
    int memoryStream3 = WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(uint16_1), "CHANNELS");
    int num6 = num5 + memoryStream3 + WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(sampleRate), "SAMPLE_RATE");
    int num7 = sampleRate * channels * WavUtility.BytesPerSample(bitDepth);
    int memoryStream4 = WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(num7), "BYTE_RATE");
    int num8 = num6 + memoryStream4;
    ushort uint16_2 = Convert.ToUInt16(channels * WavUtility.BytesPerSample(bitDepth));
    int memoryStream5 = WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(uint16_2), "BLOCK_ALIGN");
    return num8 + memoryStream5 + WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(bitDepth), "BITS_PER_SAMPLE");
  }

  private static int WriteFileData(ref MemoryStream stream, AudioClip audioClip, ushort bitDepth)
  {
    float[] data = new float[audioClip.samples * audioClip.channels];
    audioClip.GetData(data, 0);
    byte[] int16ByteArray = WavUtility.ConvertAudioClipDataToInt16ByteArray(data);
    byte[] bytes = Encoding.ASCII.GetBytes("data");
    int num = 0 + WavUtility.WriteBytesToMemoryStream(ref stream, bytes, "DATA_ID");
    int int32 = Convert.ToInt32(audioClip.samples * audioClip.channels * 2);
    int memoryStream = WavUtility.WriteBytesToMemoryStream(ref stream, BitConverter.GetBytes(int32), "SAMPLES");
    return num + memoryStream + WavUtility.WriteBytesToMemoryStream(ref stream, int16ByteArray, "DATA");
  }

  private static byte[] ConvertAudioClipDataToInt16ByteArray(float[] data)
  {
    MemoryStream memoryStream = new MemoryStream();
    int count = 2;
    short maxValue = short.MaxValue;
    for (int index = 0; index < data.Length; ++index)
      memoryStream.Write(BitConverter.GetBytes(Convert.ToInt16(data[index] * (float) maxValue)), 0, count);
    byte[] array = memoryStream.ToArray();
    memoryStream.Dispose();
    return array;
  }

  private static int WriteBytesToMemoryStream(ref MemoryStream stream, byte[] bytes, string tag = "")
  {
    int length = bytes.Length;
    stream.Write(bytes, 0, length);
    return length;
  }

  public static ushort BitDepth(AudioClip audioClip)
  {
    return Convert.ToUInt16((float) (audioClip.samples * audioClip.channels) * audioClip.length / (float) audioClip.frequency);
  }

  private static int BytesPerSample(ushort bitDepth) => (int) bitDepth / 8;

  private static int BlockSize(ushort bitDepth)
  {
    if (bitDepth == (ushort) 8)
      return 1;
    if (bitDepth == (ushort) 16 /*0x10*/)
      return 2;
    if (bitDepth == (ushort) 32 /*0x20*/)
      return 4;
    throw new Exception(bitDepth.ToString() + " bit depth is not supported.");
  }

  private static string FormatCode(ushort code)
  {
    switch (code)
    {
      case 1:
        return "PCM";
      case 2:
        return "ADPCM";
      case 3:
        return "IEEE";
      case 7:
        return "μ-law";
      case 65534:
        return "WaveFormatExtensable";
      default:
        Debug.LogWarning((object) ("Unknown wav code format:" + code.ToString()));
        return "";
    }
  }
}
