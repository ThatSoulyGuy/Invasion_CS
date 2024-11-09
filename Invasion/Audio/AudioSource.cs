using Invasion.Util;
using System;
using System.IO;
using System.Threading.Tasks;
using Vortice.Multimedia;
using Vortice.XAudio2;

namespace Invasion.Audio
{
    public class AudioSource
    {
        public string Name { get; init; } = string.Empty;
        public DomainedPath Path { get; init; } = null!;
        public float Volume { get; set; } = 1.0f;
        public float Pitch { get; set; } = 1.0f;
        public bool IsPlaying { get; private set; }
        public bool Loop { get; set; }

        private static IXAudio2? XAudio = null;
        private static IXAudio2MasteringVoice? MasteringVoice = null;
        private IXAudio2SourceVoice? SourceVoice { get; set; }
        private AudioBuffer? AudioBuffer { get; set; }

        private AudioSource() { }

        public void Play()
        {
            if (IsPlaying)
                return;

            string fullAudioPath = Path.FullPath;

                try
                {
                    InitializeXAudio2(fullAudioPath);

                    SourceVoice?.SetVolume(Volume);
                    //SourceVoice?.SetFrequencyRatio(Pitch);

                    SourceVoice?.SubmitSourceBuffer(AudioBuffer!);
                    SourceVoice?.Start();

                    IsPlaying = true;

                    if (!Loop)
                    {
                        while (IsPlaying && SourceVoice?.State.BuffersQueued > 0)
                            System.Threading.Thread.Sleep(50);

                        Stop();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing audio: {ex.Message}");
                }
        }

        public void Stop()
        {
            if (!IsPlaying)
                return;

            SourceVoice?.Stop();
            SourceVoice?.FlushSourceBuffers();
            IsPlaying = false;
        }

        private void InitializeXAudio2(string fullAudioPath)
        {
            if (XAudio == null)
            {
            if (SourceVoice != null)
                return;

            using var audioStream = new SoundStream(new FileStream(fullAudioPath, FileMode.Open, FileAccess.Read));
            var format = audioStream.Format!;

            AudioBuffer = new AudioBuffer(audioStream.ToDataStream())
            {
                AudioBytes = (uint)audioStream.Length,
                Flags = BufferFlags.EndOfStream,
                LoopCount = Loop ? (uint)XAudio2.LoopInfinite : 0
            };

            SourceVoice = XAudio.CreateSourceVoice(format);
        }

        public static AudioSource Create(string name, bool loop, DomainedPath path)
        {
            return new AudioSource
            {
                Name = name,
                Path = path,
                Volume = 1.0f,
                Pitch = 1.0f,
                Loop = loop
            };
        }
    }
}
