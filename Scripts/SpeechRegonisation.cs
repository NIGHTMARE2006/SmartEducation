using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using HuggingFace.API;

public class SpeechRegonisation : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private TextMeshProUGUI text;



    private AudioClip clip;
    private byte[] bytes;
    private bool recording;



    private void Start()
    {
        startButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);

    }

    private void StartRecording()
    {
        clip = Microphone.Start(null, false, 10, 44100);
        recording = true;


    }

    private void StopRecording()
    {
        var position = Microphone.GetPosition(null);
        Microphone.End(null);
        var Samples = new float[position * clip.channels];
        clip.GetData(Samples, 0);
        bytes = EncodeAsWAV(Samples, clip.frequency, clip.channels);
        recording = false;
        SendRecording();

    }

    private void SendRecording()
    {
        HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response =>
        {
            text.text = response;
        }, error =>
        {
            text.text = error;
        });


    }

    private void Update()
    {
        if (recording && Microphone.GetPosition(null) >= clip.samples)
        {
            StopRecording();
        }
    }

    private byte[] EncodeAsWAV(float[] Samples,int frequency, int channels)
        {
            using (var memoryStream = new MemoryStream(44+ Samples.Length * 2))
            {
                using (var writer = new BinaryWriter(memoryStream))
                {
                    writer.Write("RIFF".ToCharArray());
                    writer.Write(36 + Samples.Length * 2);
                    writer.Write("WAVE".ToCharArray());
                    writer.Write("fmt".ToCharArray());
                    writer.Write(16);
                    writer.Write((ushort)1);
                    writer.Write((ushort)channels);
                    writer.Write(frequency);
                    writer.Write(frequency * channels * 2);
                    writer.Write((ushort)(channels * 2));
                    writer.Write((ushort)16);
                    writer.Write("data".ToCharArray());
                    writer.Write(Samples.Length * 2);


                    foreach (var Sample in Samples )
                    {
                        writer.Write((short)(Sample * short.MaxValue));

                    }

                    return memoryStream.ToArray();

                }
            }


        }

    }
