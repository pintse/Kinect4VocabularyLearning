using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using log4net;

namespace Ryan.Content.Service
{
    /// <summary>
    /// TTS服務
    /// </summary>
    public class SpeechSynthesizeService
    {
        private SpeechSynthesizer _SpeechSynthesizer = new SpeechSynthesizer();
        private static volatile SpeechSynthesizeService _Myself;
        private static readonly object ticket = new object();
        private static ILog log = LogManager.GetLogger(typeof(SpeechSynthesizeService));
        private Queue<string[]> Waiting4Speech = new Queue<string[]>();

        public bool Speeching
        {
            get;
            private set;
        }

        private SpeechSynthesizeService()
        {
            Speeching = false;
            _SpeechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Child);
            _SpeechSynthesizer.Volume = 100;
            _SpeechSynthesizer.SetOutputToDefaultAudioDevice();
             
        }

        public static SpeechSynthesizeService getInstance()
        {
            if (_Myself == null)
            {
                lock (ticket)
                {
                    if (_Myself == null)
                    {
                        _Myself = new SpeechSynthesizeService();

                    }
                }

            }

            return _Myself;
        }

        public void speech(string words, int rate = -4 )
        {
            if (Speeching)
            {
                log.Info("TTS尚為結束時，傳入" + words);
                return;
            }

            try
            {
                Speeching = true;
                _SpeechSynthesizer.SpeakAsyncCancelAll();
                _SpeechSynthesizer.Rate = rate;
                _SpeechSynthesizer.Speak(words);
                Speeching = false;
            }
            catch (OperationCanceledException oce)
            {
                log.Info(oce);
            }
        }

        public void speech2(string words, int rate = -4)
        {
            try
            {
                Speeching = true;
                _SpeechSynthesizer.SpeakAsyncCancelAll();
                _SpeechSynthesizer.Rate = rate;
                _SpeechSynthesizer.Speak(words);
                Speeching = false;
            }
            catch (OperationCanceledException oce)
            {
                log.Info(oce);
            }
        }

        public void speakAsync(string words, int rate = -4)
        {
            Speeching = true;
            _SpeechSynthesizer.Rate = rate;
            _SpeechSynthesizer.SpeakAsync(words);
            Speeching = false;
        }

        public void speechFromQueue(List<string[]> speechMetaDatas)
        {
            foreach (string[] speechData in speechMetaDatas)
            {
                Waiting4Speech.Enqueue(speechData);
            }

            if (Speeching)
                return;

            while (Waiting4Speech.Count > 0)
            {
                try
                {
                    Speeching = true;
                    string[] speechData = Waiting4Speech.Dequeue();
                    _SpeechSynthesizer.Rate = int.Parse(speechData[1]);
                    _SpeechSynthesizer.Speak(speechData[0]);
                    _SpeechSynthesizer.SpeakAsyncCancelAll();
                    Thread.Sleep(300);
                }
                catch (OperationCanceledException oce)
                {
                    log.Info(oce);
                }
            }
            Speeching = false;
        }

        public void cancelSpeak()
        {
            Waiting4Speech.Clear();
            _SpeechSynthesizer.Pause();
            _SpeechSynthesizer.Resume();
            _SpeechSynthesizer.SpeakAsyncCancelAll();
            Speeching = false;
        }


    }
}
