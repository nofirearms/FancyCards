using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Reflection.PortableExecutable;

namespace FancyCards.Audio
{
    public class AudioUtilities
    {

        //------------------------------------------------------------------------------------------------------------------------------------------- DEVICES -----------------------------//
        #region DEVICES
        private MMDeviceEnumerator _deviceEnumerator = new MMDeviceEnumerator();
        /// <summary>
        /// Get list of Wasapi devices, inputs and outputs
        /// </summary>
        public IEnumerable<MMDevice> GetRecordDevices()
        {
            var input_devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            var output_devices = _deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            List<MMDevice> devices = [.. input_devices, .. output_devices];

            return devices;
        }
        /// <summary>
        /// Get default input device
        /// </summary>
        public MMDevice GetDefaultInputDevice() => _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
        /// <summary>
        /// Get default outpute device for loopback recording
        /// </summary>
        public MMDevice GetDefaultOutputDevice() => _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
        /// <summary>
        /// Get device by ID
        /// </summary>
        public MMDevice GetDeviceById(string id) => _deviceEnumerator.GetDevice(id);
        /// <summary>
        /// Initializes a new istance of Wasapi capture class
        /// </summary>
        /// <param name="device"></param>
        /// <returns>Capture class instance, input or loopback</returns>
        public WasapiCapture GetWasapiCaptureInstance(MMDevice device) => device.DataFlow == DataFlow.Capture ? new WasapiCapture(device) : new WasapiLoopbackCapture(device);
        #endregion


        //------------------------------------------------------------------------------------------------------------------------------------------- AUDIO PROCESSING -------------------------
        #region AUDIO PROCESSING

        /// <summary>Обрезать 0 - 1</summary>
        public byte[] Trim(byte[] data, double startPosition, double endPosition, WaveFormat format)
        {
            int bytesPerSample = format.BlockAlign;
            int startByte = (int)(startPosition * data.Length);
            int endByte = (int)(endPosition * data.Length);

            startByte = startByte - (startByte % bytesPerSample);
            endByte = endByte - (endByte % bytesPerSample);

            if (data.Length == 0) return data;
            //SaveState();

            data = data.Skip(startByte).Take(endByte - startByte).ToArray();

            return data;
        }


        public byte[] Cut(byte[] data, double startPosition, double endPosition, WaveFormat format)
        {
            int bytesPerSample = format.BlockAlign;
            int startByte = (int)(startPosition * data.Length);
            int endByte = (int)(endPosition * data.Length);

            startByte = startByte - (startByte % bytesPerSample);
            endByte = endByte - (endByte % bytesPerSample);

            if (data.Length == 0) return data;
            //SaveState();

            data = data.Take(startByte)
                                       .Concat(data.Skip(endByte))
                                       .ToArray();

            return data;
        }

        /// <summary>
        /// Получаем RMS (float)
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public double GetRMS(ISampleProvider reader)
        {
            double sumSq = 0;
            long sampleCount = 0;

            // Pass 1: расчет среднего RMS по всему файлу
            float[] buffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
            int read;

            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < read; i++)
                {
                    sumSq += buffer[i] * buffer[i];
                }
                sampleCount += read;
            }

            double rms = Math.Sqrt(sumSq / sampleCount);
            if (rms < 1e-9) rms = 1e-9;

            return rms;
        }

        /// <summary>
        /// Получаем RMS(dbFS)
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public double GetRmsDB(ISampleProvider reader)
        {
            var rms = GetRMS(reader);
            return 20 * Math.Log10(rms);
        }

        #endregion

        //--------------------------------------------------------- EXPORT ------------------------------------------
        #region EXPORT

        /// <summary>Экспорт текущего PCM в WAV файл</summary>
        public void ExportToWav(string path, byte[] data, WaveFormat format)
        {
            using var writer = new WaveFileWriter(path, format);
            writer.Write(data, 0, data.Length);
        }

        public async Task<bool> ExportToMp3Async(string path, byte[] data, WaveFormat format, int bitRate = 128_000)
        {
            try
            {
                return await Task.Run(() =>
                {
                    CreateDirectory(path); 
                    using (var resampler = new MediaFoundationResampler(new RawSourceWaveStream(new MemoryStream(data), format), format))
                    {
                        MediaFoundationEncoder.EncodeToMp3(resampler, path, bitRate);
                    }
                    return true;
                });
            }
            catch
            {
                return false;
            }

        }

        #endregion


        public void CreateDirectory(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
