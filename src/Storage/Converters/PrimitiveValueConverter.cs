using Assistant.Net.Storage.Abstractions;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assistant.Net.Storage.Converters
{
    internal class PrimitiveValueConverter :
        IValueConverter<string>,
        IValueConverter<Guid>,
        IValueConverter<bool>,
        IValueConverter<int>,
        IValueConverter<float>,
        IValueConverter<double>,
        IValueConverter<decimal>,
        IValueConverter<TimeSpan>,
        IValueConverter<DateTime>,
        IValueConverter<DateTimeOffset>
    {
        Task<byte[]> IValueConverter<string>.Convert(string value, CancellationToken _) =>
            Task.FromResult(Encoding.UTF8.GetBytes(value));

        Task<string> IValueConverter<string>.Convert(byte[] valueContent, CancellationToken _) =>
            Task.FromResult(Encoding.UTF8.GetString(valueContent));

        Task<byte[]> IValueConverter<Guid>.Convert(Guid value, CancellationToken _) =>
            Task.FromResult(value.ToByteArray());

        Task<Guid> IValueConverter<Guid>.Convert(byte[] valueContent, CancellationToken _) =>
            Task.FromResult(new Guid(valueContent));

        Task<byte[]> IValueConverter<bool>.Convert(bool value, CancellationToken _) =>
            Task.FromResult(BitConverter.GetBytes(value));

        Task<bool> IValueConverter<bool>.Convert(byte[] valueContent, CancellationToken _) =>
            Task.FromResult(BitConverter.ToBoolean(valueContent));

        Task<byte[]> IValueConverter<int>.Convert(int value, CancellationToken _) =>
            Task.FromResult(BitConverter.GetBytes(value));

        Task<int> IValueConverter<int>.Convert(byte[] valueContent, CancellationToken _) => 
            Task.FromResult(BitConverter.ToInt32(valueContent));

        Task<byte[]> IValueConverter<double>.Convert(double value, CancellationToken _) => 
            Task.FromResult(BitConverter.GetBytes(value));

        Task<double> IValueConverter<double>.Convert(byte[] valueContent, CancellationToken _) => 
            Task.FromResult(BitConverter.ToDouble(valueContent));

        Task<byte[]> IValueConverter<decimal>.Convert(decimal value, CancellationToken _)
        {
            var valueContent = decimal.GetBits(value).SelectMany(BitConverter.GetBytes).ToArray();
            return Task.FromResult(valueContent);
        }

        /// <exception cref="ArgumentException"/>
        Task<decimal> IValueConverter<decimal>.Convert(byte[] valueContent, CancellationToken _)
        {
            if (valueContent.Length != sizeof(decimal))
                throw new ArgumentException($"Invalid content size {valueContent.Length}b. Expected {sizeof(decimal)}b.", nameof(valueContent));

            var bits = Enumerable.Range(0, sizeof(decimal)).Select(x => BitConverter.ToInt32(valueContent, x)).ToArray();
            return Task.FromResult(new decimal(bits));
        }

        Task<byte[]> IValueConverter<float>.Convert(float value, CancellationToken _) => 
            Task.FromResult(BitConverter.GetBytes(value));

        Task<float> IValueConverter<float>.Convert(byte[] valueContent, CancellationToken _) => 
            Task.FromResult(BitConverter.ToSingle(valueContent));

        Task<byte[]> IValueConverter<TimeSpan>.Convert(TimeSpan value, CancellationToken _) =>
            Task.FromResult(BitConverter.GetBytes(value.Ticks));

        Task<TimeSpan> IValueConverter<TimeSpan>.Convert(byte[] valueContent, CancellationToken _) =>
            Task.FromResult(new TimeSpan(BitConverter.ToInt32(valueContent)));

        Task<byte[]> IValueConverter<DateTime>.Convert(DateTime value, CancellationToken _) =>
            Task.FromResult(BitConverter.GetBytes(value.Ticks));

        Task<DateTime> IValueConverter<DateTime>.Convert(byte[] valueContent, CancellationToken _) =>
            Task.FromResult(new DateTime(BitConverter.ToInt32(valueContent)));

        Task<byte[]> IValueConverter<DateTimeOffset>.Convert(DateTimeOffset value, CancellationToken _)
        {
            var dateTimeContent = BitConverter.GetBytes(value.DateTime.Ticks);
            var offsetContent = BitConverter.GetBytes(value.Offset.Ticks);  
            var valueContent = dateTimeContent.Concat(offsetContent).ToArray();
            return Task.FromResult(valueContent);
        }

        Task<DateTimeOffset> IValueConverter<DateTimeOffset>.Convert(byte[] valueContent, CancellationToken _)
        {
            var dateTime = new DateTime(BitConverter.ToInt64(valueContent, 0));
            var offset = new TimeSpan(BitConverter.ToInt64(valueContent, sizeof(long)));
            return Task.FromResult(new DateTimeOffset(dateTime, offset));
        }
    }
}