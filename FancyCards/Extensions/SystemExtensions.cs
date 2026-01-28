using Newtonsoft.Json;
using System.Windows.Media;
using System.Windows;
using NAudio.Wave;
using System.IO;

namespace FancyCards.Extensions
{
    public static class SystemExtensions
    {
        public static T Clone<T>(this T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public static bool Equals<T>(this T source, T target)
        {
            var serialized_source = JsonConvert.SerializeObject(source);
            var serialized_target = JsonConvert.SerializeObject(target);

            return string.Equals(serialized_source, serialized_target);
        }

        public static WaveStream CloneRawWaveStream(this WaveStream source)
        {
            var memorystream = new MemoryStream();
            source.Position = 0;
            var buffer = new byte[source.WaveFormat.AverageBytesPerSecond];
            int bytes_read = 0;
            while((bytes_read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                memorystream.Write(buffer, 0, bytes_read);
            }
            memorystream.Position = 0;
            var outputstream  = new RawSourceWaveStream(memorystream, source.WaveFormat);
            memorystream.Dispose();
            return outputstream;
        }

        public static T FindVisualChild<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        public static T FindVisualParent<T>(this DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject is null) return null;
            var target = dependencyObject;

            while (target is not T)
            {
                target = VisualTreeHelper.GetParent(target);
            }

            return target as T;
        }


        public static T FindChild<T>(this DependencyObject depObj, string childName) where T : DependencyObject
        {
            // Confirm obj is valid. 
            if (depObj == null) return null;

            // success case
            if (depObj is T && ((FrameworkElement)depObj).Name == childName)
                return depObj as T;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                //DFS
                T obj = FindChild<T>(child, childName);

                if (obj != null)
                    return obj;
            }

            return null;
        }
    }
}
