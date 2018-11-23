using System;
using Tauron.Application.ImageOrganizer.Data.Entities;

namespace Tauron.Application.ImageOrganizer.BL
{
    public enum TriggerType
    {
        Update,
        Delete,
        Insert
    }

    public class EntityUpdate<TType> : EventArgs
    {
        public TType Data { get;  }

        public TriggerType TriggerType { get; }

        public EntityUpdate(TType data, TriggerType triggerType)
        {
            Data = data;
            TriggerType = triggerType;
        }
    }

    public static class DataTrigger
    {
        public static event EventHandler<EntityUpdate<ImageData>> ImageChangedEvent;
        public static event EventHandler<EntityUpdate<TagData>> TagChangedEvent;
        public static event EventHandler<EntityUpdate<TagTypeData>> TagTypeChangedEvent;

        public static void OnChangedEvent(TriggerType type, ImageEntity entity) => ImageChangedEvent?.Invoke(null, new EntityUpdate<ImageData>(new ImageData(entity), type));
        public static void OnChangedEvent(TriggerType type, TagEntity entity) => TagChangedEvent?.Invoke(null, new EntityUpdate<TagData>(new TagData(entity), type));
        public static void OnChangedEvent(TriggerType type, TagTypeEntity entity) => TagTypeChangedEvent?.Invoke(null, new EntityUpdate<TagTypeData>(new TagTypeData(entity), type));
    }
}