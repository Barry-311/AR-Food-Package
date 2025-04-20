using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class WavUtility
{
    /// <summary>
    /// 将 Unity 的 AudioClip 数据导出成带 RIFF fmt data 头的 WAV Byte[]
    /// </summary>
    public static byte[] FromAudioClip(AudioClip clip)
    {
        if (clip == null)
            throw new ArgumentNullException(nameof(clip));

        int sampleCount = clip.samples * clip.channels;
        float[] floatData = new float[sampleCount];
        clip.GetData(floatData, 0);

        // 1) 浮点[-1,1]转 PCM16 short，再转 byte[]
        short[] intData = new short[sampleCount];
        byte[] bytesData = new byte[sampleCount * 2];
        const int rescaleFactor = 32767;
        for (int i = 0; i < sampleCount; i++)
        {
            intData[i] = (short)(floatData[i] * rescaleFactor);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            Array.Copy(byteArr, 0, bytesData, i * 2, 2);
        }

        // 2) 写入 WAV 头 + data
        using (var stream = new MemoryStream())
        {
            // —— RIFF chunk —— 
            byte[] riff = Encoding.ASCII.GetBytes("RIFF");
            stream.Write(riff, 0, riff.Length);
            // ChunkSize = 36 + Subchunk2Size
            stream.Write(BitConverter.GetBytes(36 + bytesData.Length), 0, 4);
            byte[] wave = Encoding.ASCII.GetBytes("WAVE");
            stream.Write(wave, 0, wave.Length);

            // —— fmt sub-chunk —— 
            byte[] fmt = Encoding.ASCII.GetBytes("fmt ");
            stream.Write(fmt, 0, fmt.Length);
            stream.Write(BitConverter.GetBytes(16), 0, 4);                   // Subchunk1Size=16
            stream.Write(BitConverter.GetBytes((short)1), 0, 2);             // AudioFormat=1(PCM)
            stream.Write(BitConverter.GetBytes((short)clip.channels), 0, 2);
            stream.Write(BitConverter.GetBytes(clip.frequency), 0, 4);
            stream.Write(BitConverter.GetBytes(clip.frequency * clip.channels * 2), 0, 4); // ByteRate
            stream.Write(BitConverter.GetBytes((short)(clip.channels * 2)), 0, 2);          // BlockAlign
            stream.Write(BitConverter.GetBytes((short)16), 0, 2);            // BitsPerSample

            // —— data sub-chunk —— 
            byte[] dataString = Encoding.ASCII.GetBytes("data");
            stream.Write(dataString, 0, dataString.Length);
            stream.Write(BitConverter.GetBytes(bytesData.Length), 0, 4);
            stream.Write(bytesData, 0, bytesData.Length);

            return stream.ToArray();
        }
    }
}
