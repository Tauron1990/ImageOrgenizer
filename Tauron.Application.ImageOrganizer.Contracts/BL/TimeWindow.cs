using System;
using System.IO;

namespace Tauron.Application.ImageOrganizer.BL
{
    public abstract class TimeAction
    {
        public abstract void Execute(DateTime? end);

        public abstract void Serialize(BinaryWriter writer);

        public abstract void Deserialize(BinaryReader reader);
    }

    public enum TimeWindowType
    {
        Unique = 0,
        Daly,
        Monthly,
        Yearly
    }
    public sealed class TimeWindow
    {
        public TimeWindowType TimeType { get; set; }

        public DateTime Start { get; set; }

        public DateTime? Stop { get; set; }

        public TimeAction TimeAction { get; set; }

        public byte[] Serialize()
        {
            using (var mem = new MemoryStream())
            {
                using (var writer = new BinaryWriter(mem))
                {
                    writer.Write((int)TimeType);
                    writer.Write(Start.Ticks);
                    if (Stop != null)
                    {
                        writer.Write(true);
                        writer.Write(Stop.Value.Ticks);
                    }
                    else
                        writer.Write(false);

                    if (TimeAction == null) return mem.ToArray();

                    writer.Write(TimeAction.GetType().AssemblyQualifiedName ?? throw new InvalidOperationException("TimeAction no Assembly Qualifid Name"));
                    TimeAction.Serialize(writer);
                }

                return mem.ToArray();
            }
        }

        public void Deserialize(byte[] array)
        {

        }
    }
}